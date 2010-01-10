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
using System.Data;
using System.IO;
using Platform.Common.DB;

namespace VolumeDB.Import
{
	public sealed class GnomeCatalogImport : AbstractImport
	{
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
			
//			List<long> volumeIDs = new List<long>();
			
//			target.TransactionBegin();
			
//			try {
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
								long volumeID = targetDb.GetNextVolumeID();
								
								ImportDisk(readerDisks, volumeID, targetDb);
								string volDBThumbsPath = CreateThumbsDir(dbDataPath, volumeID);
								AddNewVolumeID(volumeID);
								
								cmdFiles.CommandText = string.Format(sqlFiles, diskID);
								
								using (IDataReader readerFiles = cmdFiles.ExecuteReader()) {
									while (readerFiles.Read()) {
										ImportFile(readerFiles,
										           volumeID,
										           minFileID,
										           (string)readerDisks["root"], 
										           targetDb);
										
										long fileID = (long)readerFiles["id"];
										
										ImportThumb(fileID,
										            (2 + fileID - minFileID),
										        	thumbsPath,
									            	volDBThumbsPath);
									
										PostProgressUpdate((double)(++fileCounter / totalFiles) * 100);
										CheckForCancellationRequest();
									}
								}
							}
						}
					}
				}
			}
				
//				target.TransactionCommit();
				
//			} catch (Exception) {
//				target.TransactionRollback();
//				
//				foreach (long id in volumeIDs) {
//					string volumeDataPath = Path.Combine(dbDataPath, id.ToString());
//					Directory.Delete(volumeDataPath, true);
//				}
//				
//				throw;
//			}
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
			string sql = string.Format("SELECT MIN id FROM files WHERE iddisk = {0}", diskID);
			long minID;
			
			using (IDbCommand cmd = conn.CreateCommand()) {
				cmd.CommandText = sql;
				minID = (long)cmd.ExecuteScalar();
			}
			
			return minID;
		}
		
		private static void ImportDisk(IDataReader reader,
		                               long volumeID,
		                               VolumeDatabase db) {
			
			Volume v = new FileSystemVolume(db);
			
			v.SetVolumeFields(volumeID,
			                  (string)reader["name"],
			                  DateTime.MinValue,
			                  false,
			                  reader["id"].ToString(),
			                  VolumeDriveType.CDRom,
			                  (string)reader["borrow"],
			                  DateTime.MinValue,
			                  DateTime.MinValue,
			                  null,
			                  (string)reader["comment"],
			                  null);
			
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
		                               VolumeDatabase db) {
			
			FileSystemVolumeItem item;
			
			if ((string)reader["type"] == "directory") {
				item = new DirectoryVolumeItem(db);
			} else {
				item = new FileVolumeItem(db);
				((FileVolumeItem)item).SetFileVolumeItemFields((long)reader["size"], null);
			}
			
			string location = ((string)reader["location"]).Substring(rootPath.Length);
			long itemID = 2 + (long)reader["id"] - minFileID; // id 1 is the root item
			long parentID = 2 + (long)reader["idparent"] - minFileID;
			string metaData = null;
			
			item.SetFileSystemVolumeItemFields(location,
			                                   DateTime.MinValue,
			                                   VolumeDatabase.ID_NONE);
			
			item.SetVolumeItemFields(volumeID,
			                         itemID,
			                         parentID,
			                         (string)reader["name"],
			                         (string)reader["mime"],
			                         metaData,
			                         (string)reader["comment"],
			                         null);
			
			item.InsertIntoDB();
		}
		
		/*private static string CreateThumbsDir(string dbDataPath, long volumeID) {
			string volumeDataPath = Path.Combine(dbDataPath, volumeID.ToString());
					
			// make sure there is no directory with the same name as the volume directory 
			// that is about to be created
			// (the volume directory will be deleted in the catch block on failure, 
			// so make sure that no existing dir will be deleted)
			if (Directory.Exists(volumeDataPath))
				throw new ArgumentException("dbDataPath already contains a directory for this volume");
			
			// thumbnails will be stored in <dbdataPath>/<volumeID>/thumbs
			string thumbnailPath = Path.Combine(volumeDataPath, "thumbs");
			Directory.CreateDirectory(thumbnailPath);
			
			return thumbnailPath;
		}*/
		
		/*
		private static void ImportThumb(long fileID,
		                                long minFileID,
		                                string sourceThumbsPath,
		                                string targetThumbsPath) {
			
			string sourceThumb = Path.Combine(sourceThumbsPath, fileID.ToString());
			string targetThumb = Path.Combine(targetThumbsPath, (2 + fileID - minFileID).ToString());
			
			if (File.Exists(sourceThumb))
				File.Copy(sourceThumb, targetThumb);
		}*/
	}
}
