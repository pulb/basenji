// GnomeCatalogImport.cs
// 
// Copyright (C) 2010 Patrick Ulbrich
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
	
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Globalization;
using Platform.Common.DB;
using LibExtractor;

namespace VolumeDB.Import
{
	public sealed class GnomeCatalogImport : AbstractImport
	{
		private const int TOTAL_FILES = 0;
		private const int TOTAL_DIRS = 1;
		private const int TOTAL_SIZE = 2;
		
		public GnomeCatalogImport(VolumeDatabase targetDb, string dbDataPath)
		: base(targetDb, dbDataPath) {}
		
		protected override void ImportThreadMain(VolumeDatabase targetDb, string dbDataPath) {
			
			string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string dbPath = Path.Combine(homeDir, "gnomeCatalog.db");
			string thumbsPath = Path.Combine(homeDir, "gnomeCatalog.db_thumbs");
			
			string sqlDisks = "SELECT * FROM disks ORDER BY id";
			string sqlFiles = "SELECT * FROM files WHERE iddisk = {0} ORDER BY iddisk, id";
			
			if (!File.Exists(dbPath))
				throw new FileNotFoundException("GnomeCatalog database not found");
			
			using (IDbConnection conn = SqliteDB.Open(dbPath, false)) {
				
				long totalFiles = CountFiles(conn);
				long fileCounter = 0;
				
				using (IDbCommand cmdDisks = conn.CreateCommand()) {
					using (IDbCommand cmdFiles = conn.CreateCommand()) {
						
						cmdDisks.CommandText = sqlDisks;
						
						using (IDataReader readerDisks = cmdDisks.ExecuteReader()) {
							while (readerDisks.Read()) {
								long diskID = (long)readerDisks["id"];
								long minFileID = GetMinFileID(conn, diskID);
								string rootPath = "file:///" + (string)readerDisks["root"];
								long volumeID = targetDb.GetNextVolumeID();
								long[] counters = { 0L, 0L, 0L };
								
								string volDBThumbsPath = CreateThumbsDir(dbDataPath, volumeID);
								
								cmdFiles.CommandText = string.Format(sqlFiles, diskID);
								
								using (IDataReader readerFiles = cmdFiles.ExecuteReader()) {
									while (readerFiles.Read()) {
										long fileID = (long)readerFiles["id"];
										
										ImportFile(readerFiles,
										           volumeID,
										           minFileID,
										           rootPath,
										           ConvertMetaData(conn, fileID),
										           targetDb,
										           counters);
										
										ImportThumb(fileID,
										            (2 + fileID - minFileID),
										        	thumbsPath,
									            	volDBThumbsPath);
									
										PostProgressUpdate((++fileCounter * 100.0) / totalFiles);
										CheckForCancellationRequest();
									}
								}
								
								ImportDisk(readerDisks,
								           volumeID,
								           targetDb,
								           counters);
							}
						}
					}
				}
			}
		}
		
		private static long CountFiles(IDbConnection conn) {
			string sql = "SELECT COUNT(id) FROM files";
			long count;
			
			using (IDbCommand cmd = conn.CreateCommand()) {
				cmd.CommandText = sql;
				count = (long)cmd.ExecuteScalar();
			}
			
			return count;
		}
		
		private static long GetMinFileID(IDbConnection conn, long diskID) {
			string sql = string.Format("SELECT MIN(id) FROM files WHERE iddisk = {0}", diskID);
			long minID;
			
			using (IDbCommand cmd = conn.CreateCommand()) {
				cmd.CommandText = sql;
				object val = cmd.ExecuteScalar();
				if (val == DBNull.Value)
					minID = 1;
				else
					minID = (long)cmd.ExecuteScalar();
			}
			
			return minID;
		}
		
		private static void ImportDisk(IDataReader reader,
		                               long volumeID,
		                               VolumeDatabase db,
		                               long[] counters) {
			
			FileSystemVolume v = new FileSystemVolume(db);
			long diskID = (long)reader["id"];
			
			// try to guess the drivetype
			VolumeDriveType driveType;
			string root = (string)reader["root"];
			if (root.ToUpper().Contains("CDROM") || root.ToUpper().Contains("DVD"))
				driveType = VolumeDriveType.CDRom;
			else if (root.StartsWith("/media"))
				driveType = VolumeDriveType.Removable;
			else
				driveType = VolumeDriveType.Harddisk;
			
			v.SetVolumeFields(volumeID,
			                  ReplaceDBNull<string>(reader["name"], null),
			                  DateTime.Now,
			                  false,
			                  diskID.ToString(),
			                  driveType,
			                  ReplaceDBNull<string>(reader["borrow"], null),
			                  DateTime.MinValue,
			                  DateTime.MinValue,
			                  null,
			                  ReplaceDBNull<string>(reader["comment"], null),
			                  null);
			
			v.SetFileSystemVolumeFields(counters[TOTAL_FILES],
			                            counters[TOTAL_DIRS],
			                            counters[TOTAL_SIZE]);
			
			v.InsertIntoDB();
			
			// insert root item
			DirectoryVolumeItem item = new DirectoryVolumeItem(db);
			
			item.SetFileSystemVolumeItemFields(null,
			                                   DateTime.MinValue,
			                                   VolumeDatabase.ID_NONE);
			
			item.SetVolumeItemFields(volumeID,
			                         1L,
			                         0L,
			                         "/",
			                         VolumeScanner.FilesystemVolumeScanner.MIME_TYPE_DIRECTORY,
			                         null,
			                         null,
			                         null);
			
			item.InsertIntoDB();
		}
		
		private static void ImportFile(IDataReader reader,
		                               long volumeID,
		                               long minFileID,
		                               string rootPath,
		                               string metaData,
		                               VolumeDatabase db,
		                               long[] counters) {
			
			FileSystemVolumeItem item;
			
			if ((string)reader["type"] == "directory") {
				item = new DirectoryVolumeItem(db);
				counters[TOTAL_DIRS]++;
			} else {
				item = new FileVolumeItem(db);
				long size = (long)reader["size"];
				
				((FileVolumeItem)item).SetFileVolumeItemFields(size, null);
				counters[TOTAL_FILES]++;
				counters[TOTAL_SIZE] += size;
			}
			
			string path = (string)reader["path"];			
			Assert(path.StartsWith("file:///"), "path starts with 'file://'");
			
			string location = path.Substring(rootPath.Length);
			long itemID = 2 + (long)reader["id"] - minFileID; // id 1 is the root item
			long parentID = Math.Max(1, 2 + (long)reader["idparent"] - minFileID);
			
			item.SetFileSystemVolumeItemFields(location,
			                                   DateTime.MinValue,
			                                   VolumeDatabase.ID_NONE);
			item.SetVolumeItemFields(volumeID,
			                         itemID,
			                         parentID,
			                         ReplaceDBNull<string>(reader["name"], null),
			                         ReplaceDBNull<string>(reader["mime"], null),
			                         metaData,
			                         ReplaceDBNull<string>(reader["comment"], null),
			                         null);
			
			item.InsertIntoDB();
		}
		
		private static string ConvertMetaData(IDbConnection conn, long fileID) {
			string sql = string.Format("SELECT * FROM metadata WHERE id = {0}", fileID);
			
			Dictionary<string, string> metaData = new Dictionary<string, string>();
			List<Keyword> keywords = new List<Keyword>();
			string tmp;
			
			using (IDbCommand cmd = conn.CreateCommand()) {
				cmd.CommandText = sql;
				using (IDataReader reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						metaData.Add((string)reader["key"], (string)reader["value"]);
					}
				}
			}
			
			if (metaData.Count == 0)
				return null;
			
			// import width / height
			string width = null;
			string height = null;
			
			if (!(metaData.TryGetValue("width", out width) &&
			      metaData.TryGetValue("height", out height))) {
				
				metaData.TryGetValue("video_width", out width);
				metaData.TryGetValue("video_height", out height);
			}
			
			if (!string.IsNullOrEmpty(width) && !string.IsNullOrEmpty(height)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_SIZE,
					keyword = string.Format("{0}x{1}", width, height)
				});
			}
			
			// import software			
			if (metaData.TryGetValue("software", out tmp)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_SOFTWARE,
					keyword = tmp
				});
			}
			
			// import duration
			if (metaData.TryGetValue("video_length", out tmp) ||
			    metaData.TryGetValue("length", out tmp)) {
				
				double val;
				if (double.TryParse(tmp,
				                    NumberStyles.AllowDecimalPoint,
				                    CultureInfo.InvariantCulture.NumberFormat,
				                    out val)) {
					
					keywords.Add(new Keyword() {
						keywordType = KeywordType.EXTRACTOR_DURATION,
						keyword = ((long)val).ToString()
					});
				}
			}
			
			// import comment
			if (metaData.TryGetValue("comment", out tmp)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_COMMENT,
					keyword = tmp
				});
			}			
			
			// import album
			if (metaData.TryGetValue("album", out tmp)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_ALBUM,
					keyword = tmp
				});
			}
			
			// import artist
			if (metaData.TryGetValue("artist", out tmp)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_ARTIST,
					keyword = tmp
				});
			}
			
			// import title
			if (metaData.TryGetValue("title", out tmp)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_TITLE,
					keyword = tmp
				});
			}
			
			// import genre
			if (metaData.TryGetValue("genre", out tmp)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_GENRE,
					keyword = tmp
				});
			}
			
			// import year
			if (metaData.TryGetValue("userdate", out tmp)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_YEAR,
					keyword = tmp
				});
			}
			
			// import publisher
			if (metaData.TryGetValue("publisher", out tmp)) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_PUBLISHER,
					keyword = tmp
				});
			}
			
			// import format
			// e.g.:
			// Codec: XVID / MP3, 25fps, 320kb/s, 2 channels, 48000Hz, stereo
			// Codec: MP3, 320kb/s, 2 channels, 48000Hz, stereo
			string codec = null;
			string video = null;
			string audio = null;
			
			foreach (string key in new string[] { "video_codec", "audio_codec", "codec" }) {
				if (metaData.TryGetValue(key, out tmp)) {
					if (codec == null)
						codec = string.Format("Codec: {0}", tmp);
					else
						codec += " / " + tmp;
				}
			}
			
			if (metaData.TryGetValue("vidoe_fps", out tmp)) {
				video = tmp + "fps";
			}
			
			string[][] keys = new string[][] {
				new string[] { "bitrate", "Kb/s"},
				new string[] { "audio_channels", " channels"},
				new string[] { "audio_samplerate", "Hz" },
				new string[] { "mode", "" }
			};
			
			foreach (string[] key in keys) {
				if (metaData.TryGetValue(key[0], out tmp)) {
					string val = tmp + key[1];
					if (audio == null)
						audio = val;
					else
						audio += ", " + val;
				}
			}
			
			string format = null;
			
			foreach (string i in new string[] { codec, video, audio }) {
				if (i != null) {
					if (format == null)
						format = i;
					else
						format += ", " + i;
				}
			}
			
			if (format != null) {
				keywords.Add(new Keyword() {
					keywordType = KeywordType.EXTRACTOR_FORMAT,
					keyword = format
				});
			}
			
			return MetaDataHelper.PackExtractorKeywords(keywords.ToArray());
		}
	}
}
