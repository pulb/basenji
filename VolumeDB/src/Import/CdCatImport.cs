// CdCatImport.cs
// 
// Copyright (C) 2012 Patrick Ulbrich
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
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Globalization;
using System.Linq;
using Platform.Common.Mime;
using Platform.Common.Diagnostics;
using VolumeDB;
using VolumeDB.Metadata;

namespace VolumeDB.Import
{
	public sealed class CdCatImport : AbstractImport
	{
		
		public const float MAX_SUPPORTED_VERSION = 2.1f;
		private const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
		
		private const int TOTAL_FILES = 0;
		private const int TOTAL_DIRS = 1;
		private const int TOTAL_SIZE = 2;
		
		private long[] counters;
		private long idCounter;
		private int mediaCounter;
		private int totalMedia;
		private Stack<string> path;
		private CultureInfo ci = CultureInfo.InvariantCulture;
		
		private VolumeDatabase targetDb;
		private BufferedVolumeItemWriter writer;
		
		
		public CdCatImport(string sourceDbPath,
		                          VolumeDatabase targetDb,
		                          string dbDataPath,
		                          int bufferSize)
		: base(sourceDbPath, targetDb, dbDataPath, bufferSize) {}
		
		private Dictionary<string, VolumeDriveType> driveTypeMapping = new Dictionary<string, VolumeDriveType>() {
			{ "CD", 			VolumeDriveType.CDRom		},
			{ "DVD",			VolumeDriveType.CDRom		},
			{ "HardDisc",		VolumeDriveType.Harddisk	},
			{ "NetworkPlace",	VolumeDriveType.Network		},
			{ "floppy",			VolumeDriveType.Removable	},
			{ "flashdrive",		VolumeDriveType.Removable	},
			{ "other",			VolumeDriveType.Removable	}
		};
		
		private Dictionary<string, MetadataType> metadataMapping = new Dictionary<string, MetadataType>() {
			{ "album",	MetadataType.ALBUM		},
			{ "title",	MetadataType.TITLE		},
			{ "artist",	MetadataType.ARTIST		},
			{ "year",	MetadataType.YEAR		}
		};
		
		internal override void ImportThreadMain(string sourceDbPath,
		                                         VolumeDatabase targetDb,
		                                         string dbDataPath,
		                                         BufferedVolumeItemWriter writer) {
			
			this.counters = new long[3];
			//idCounter = 2; // id 1 is the root item
			//totalMedia = 0;
			this.path = new Stack<string>();
			this.targetDb = targetDb;
			this.writer = writer;
			
			using (GZipStream s = new GZipStream(File.OpenRead(sourceDbPath), CompressionMode.Decompress)) {
				XmlReaderSettings settings = new XmlReaderSettings() {
					DtdProcessing = DtdProcessing.Ignore,
					ValidationType = ValidationType.None,
					CheckCharacters = false
				};
				
				XmlReader reader = XmlTextReader.Create(s, settings);
				 
				XmlDocument xml = new XmlDocument();
				xml.Load(reader);
				
				string dummy1 = null;
				MetadataStore dummy2 = MetadataStore.Empty;
				
				RecursiveDump(xml.DocumentElement, 0L, 0L, ref dummy1, ref dummy1, ref dummy2);
			}
		}		
		
		private void RecursiveDump(XmlNode node, 
		                           long volumeID, 
		                           long parentID,
		                           ref string comment, 
		                           ref string borrow,
		                           ref MetadataStore metadata) {
			
			long dirID = 0L;
			string b = null, c = null;
			MetadataStore md = MetadataStore.Empty;
			
			CheckForCancellationRequest();
			
			switch (node.Name) {
				case "catalog":
					mediaCounter = 0;
					totalMedia = GetMediaCount(node);
					break;
				case "media":
					volumeID = targetDb.GetNextVolumeID();
					dirID = 1L; // root item
					idCounter = 2L; // id 1 is the root item
					//path.Clear();
					for (int i = 0; i < counters.Length; i++)
						counters[i] = 0L;
					break;
				case "directory":
					dirID = idCounter++;
					path.Push(node.Attributes["name"].Value);
					break;
			}
			
			foreach (XmlNode n in node)
				RecursiveDump(n, volumeID, dirID, ref c, ref b, ref md);
			
			switch (node.Name) {
				case "media":
					ImportMedia(node, b, c, volumeID);
					PostProgressUpdate((++mediaCounter * 100.0) / totalMedia);
					break;
				case "directory":
					path.Pop();	
					ImportFile(node, c, volumeID, parentID, dirID, path, md);
					break;
				case "file":
					ImportFile(node, c, volumeID, parentID, idCounter++, path, md);
					break;
				case "comment":
					if (!ConvertMetaData(node, ref metadata))
						comment = node.InnerText.Trim();
					break;
				case "borrow":
					borrow = node.InnerText.Trim();
					break;
				case "mp3tag":
					ConvertMetaData(node, ref metadata);
					break;
				case "datafile":
					if (float.Parse(node.Attributes["version"].Value, ci.NumberFormat) > MAX_SUPPORTED_VERSION)
						throw new ImportException("Unsupported catalog version");
					break;
			}
			
		}
		
		private void ImportMedia(XmlNode node,
		                         string borrow,
		                         string comment,
		                         long volumeID) {
			
			FileSystemVolume v = new FileSystemVolume(targetDb);
			VolumeDriveType driveType = driveTypeMapping[node.Attributes["type"].Value];
			DateTime added;
			if (!DateTime.TryParseExact(node.Attributes["time"].Value, DATETIME_FORMAT, 
			                            ci.DateTimeFormat, DateTimeStyles.None, out added))
				added = DateTime.MinValue;
			
			v.SetVolumeFields(volumeID,
			                  node.Attributes["name"].Value,
			                  added,
			                  false,
			                  node.Attributes["number"].Value,
			                  driveType,
			                  borrow,
			                  DateTime.MinValue,
			                  DateTime.MinValue,
			                  null,
			                  comment,
			                  null);
			
			v.SetFileSystemVolumeFields(counters[TOTAL_FILES],
			                            counters[TOTAL_DIRS],
			                            counters[TOTAL_SIZE]);
			
			v.InsertIntoDB();
			
			// insert root item
			DirectoryVolumeItem item = new DirectoryVolumeItem(targetDb);
			
			item.SetFileSystemVolumeItemFields(null,
			                                   DateTime.MinValue,
			                                   VolumeDatabase.ID_NONE);
			
			item.SetVolumeItemFields(volumeID,
			                         1L,
			                         0L,
			                         "/",
			                         VolumeScanner.FilesystemVolumeScanner.MIME_TYPE_DIRECTORY,
			                         MetadataStore.Empty,
			                         null,
			                         null);
			
			item.InsertIntoDB();
		}
		
		private void ImportFile(XmlNode node,
		                        string comment,
		                        long volumeID,		                        
		                        long parentID,
		                        long itemID,
		                        Stack<string> path,
		                        MetadataStore metaData) {
			
			FileSystemVolumeItem item;
			string location = "/" + string.Join("/", path.Reverse());
			string name = node.Attributes["name"].Value;
			string mimeType;
			DateTime lastWriteTime;
			
			if (node.Name == "directory") {
				item = new DirectoryVolumeItem(targetDb);
				mimeType = VolumeScanner.FilesystemVolumeScanner.MIME_TYPE_DIRECTORY;
				counters[TOTAL_DIRS]++;
			} else {
				item = new FileVolumeItem(targetDb);
				mimeType = MimeType.GetMimeTypeForFile(name);
				long size = ConvertSize(node.Attributes["size"].Value);
				
				((FileVolumeItem)item).SetFileVolumeItemFields(size, null);
				counters[TOTAL_FILES]++;
				counters[TOTAL_SIZE] += size;
			}
			
			if (!DateTime.TryParseExact(node.Attributes["time"].Value, DATETIME_FORMAT, ci.DateTimeFormat, 
			                            DateTimeStyles.None, out lastWriteTime))
				lastWriteTime = DateTime.MinValue;
			
			item.SetFileSystemVolumeItemFields(location,
			                                   lastWriteTime,
			                                   VolumeDatabase.ID_NONE);
			item.SetVolumeItemFields(volumeID,
			                         itemID,
			                         parentID,
			                         name,
			                         mimeType,
			                         metaData,
			                         comment,
			                         null);
			
			writer.Write(item);
		}
		
		private static long ConvertSize(string size) {
			string[] pair = size.Split(' ');			
			double sz = double.Parse(pair[0]);
			
			switch (pair[1])
			{
				case "byte":
					break;
				case "Kb":
					sz*= 1024.0;
					break;
				case "Mb":
					sz*= (1024.0 * 1024.0);
					break;
				case "Gb":
					sz*= (1024.0 * 1024.0 * 1024.0);
					break;
				case "Tb":
					sz*= (1024.0 * 1024.0 * 1024.0 * 1024.0);
					break;				
			}
			
			return (long)sz;
		}
		
		private bool ConvertMetaData(XmlNode node, ref MetadataStore metadata) {
			List<MetadataItem> convertedData;
			StringSplitOptions opts = StringSplitOptions.RemoveEmptyEntries;
			
			Debug.Assert((node.Name == "mp3tag") || (node.Name == "comment"), 
			             string.Format("Expected 'mp3tag' or 'comment' node but got '{0}' node", node.Name));
			
			try {
				switch (node.Name) {
					case "mp3tag":
						convertedData = new List<MetadataItem>();
						
						foreach (var pair in metadataMapping) {
							string tmp = node.Attributes[pair.Key].Value.Trim();
							if (tmp.Length > 0)
								convertedData.Add(new MetadataItem(pair.Value, tmp));
						}
					
						string comment = node.InnerText.Trim();
						
						if (comment.Length > 0)
							convertedData.Add(new MetadataItem(MetadataType.COMMENT, comment));
						
						if (convertedData.Count > 0)
							metadata = new MetadataStore(convertedData);
						
						break;
					case "comment":
						string tmp = node.InnerText.Trim();
						// try to parse Video/Audio info from comments, 
						// e. g. "Video:#XVID MPEG-4#Gesamtzeit = 1:16:09#Framerate = 23.976 f/s#Aufloesung = 640x272##Audio:#ISO/MPEG Layer-3 
						//        #Kanaele = 2 #Sample/s = 48.0kHz #Bitrate = 123 kBit"
						if (tmp.StartsWith("Video:#")) {
							convertedData = new List<MetadataItem>();
							
							string[] streams = tmp.Split(new string[] { "##" }, opts);
							char[] pairSep = new char[] { '=' };
							string format;
						
							//
							// parse video info
							//
							string[] items = streams[0].Split(new char[] { '#' }, opts);
							string videoFormat;
							string framerate = null;							
							foreach (var s in items) {
								string[] pair = s.Split(pairSep);
								if (pair.Length == 2) {
									string val = pair[1].Trim();
									if (val.Contains(":")) {
										// duration
										TimeSpan duration = TimeSpan.Parse(val);
										convertedData.Add(new MetadataItem(MetadataType.DURATION, 
									                                   MetadataUtils.SecsToMetadataDuration(duration.TotalSeconds)));										
									} else if (val.Contains("x")) {
										// size (NxM)
										convertedData.Add(new MetadataItem(MetadataType.SIZE, val.Trim()));
									} else if (val.EndsWith("f/s")) {
										// framerate
										float fps;
										if (float.TryParse(val.Replace(" f/s", ""),NumberStyles.AllowDecimalPoint, ci.NumberFormat, out fps))
											framerate =  string.Format(ci.NumberFormat, "{0:F2} fps", fps);
									} else {
										// possibly number of channels, not sure since channel# is only a number without unit
									}
								}
							}
						
							videoFormat = items[1]; // e. g. "XVID MPEG-4"
							if (framerate != null)
								videoFormat += ", " + framerate;
							
							//
							// audio stream info available?
							//
							if (streams.Length > 1) {
								items = streams[1].Split(new char[] { '#' }, opts);
								string audioFormat;
								List<string> fmt = new List<string>();
								foreach (var s in items) {
									string[] pair = s.Split(pairSep);
									if (pair.Length == 2) {
										string val = pair[1].Trim();
										if (val.EndsWith("kHz")) {
											// frequency
											int freq = ((int)float.Parse(val.Replace("kHz", ""), ci.NumberFormat)) * 1000;
											fmt.Add(freq.ToString() + " Hz");
										} else if (val.EndsWith("kBit")) {
											fmt.Add(val.Replace("kBit", "") + " kb/s");
										}
									}
								}
							
								audioFormat = items[1];
								if (audioFormat.Length > 0)
									audioFormat += ", " + string.Join(", ", fmt);
							
								format = string.Format("Video: {0}; Audio: {1}", videoFormat, audioFormat);
							} else {
								format = videoFormat;
							}
						
							convertedData.Add(new MetadataItem(MetadataType.FORMAT, format));
						
							if (convertedData.Count > 0)
								metadata = new MetadataStore(convertedData);
						
						// try to parse audio only info
						// e. g. "0:7, 192 kbps#44100Hz, Simple stereo" 
						// (try to parse duration only)
						} else if (Regex.IsMatch(tmp, @"^\d+:\d{1,2}, \d+ kbps")) {
							string[] items = tmp.Split(new char[] { ',' }, opts);
							// don't use DateTime.ParseExact() since minutes may be > 59
							string[] time = items[0].Split(new char[] { ':' }, opts);
							int mins = int.Parse(time[0]);
							int secs = int.Parse(time[1]);
							double duration = (mins * 60.0) + secs;
							
							metadata = new MetadataStore(new MetadataItem[] { 
								new MetadataItem(MetadataType.DURATION, MetadataUtils.SecsToMetadataDuration(duration))
							});
						// try to parse audio only info
						// e. g. "VBR,44100Hz#Joint stereo" (didn't encounter ABR or CBR yet)
						} else if (Regex.IsMatch(tmp, @"^(VBR|ABR|CBR){1},\d+Hz")) {
							// ignored
						} else {
							return false;
						}
						
						break;
					default:
						return false;
				} // switch (node.Name)
			} catch (Exception ex) {
				Debug.WriteLine("Caught exception in ConvertMetaData():\n" + ex.ToString());
				return false;
			}
			
			return true;
		}
		
		private static int GetMediaCount(XmlNode catalogNode) {
			Debug.Assert(catalogNode.Name == "catalog", 
			             string.Format("Expected 'catalog' node but got '{0}' node", catalogNode.Name));
			
			int n = 0;
			foreach (XmlNode node in catalogNode.ChildNodes) {
				if (node.Name == "media")
					n++;
			}
			
			return n;
		}
	}
}
