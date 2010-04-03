// FilesystemVolumeScanner.cs
// 
// Copyright (C) 2008 - 2010 Patrick Ulbrich
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

/*#define DEBUG_FILE_VERBOSE*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Collections.Generic;
using Platform.Common;
using Platform.Common.IO;
using Platform.Common.Mime;
using Platform.Common.Diagnostics;
using VolumeDB.Searching;
using VolumeDB.Searching.ItemSearchCriteria;
using LibExtractor;

namespace VolumeDB.VolumeScanner
{
	// TODO : EnsureOpen() in public members?
	// TODO : override Dispose(bool) to e.g dispose/set null m_sbPathfixer?
	public sealed class FilesystemVolumeScanner
		: AbstractVolumeScanner<FileSystemVolume, FilesystemVolumeInfo, FilesystemScannerOptions>
	{
		private const char		PATH_SEPARATOR		= '/';
		internal const string	MIME_TYPE_DIRECTORY = "x-directory/normal";
		
		private bool				disposed;
		//private MimeInfo			  mimeInfo;
		private StringBuilder		sbPathFixer;
		private Paths				paths;
		private SymLinkHelper		symLinkHelper;
		private ThumbnailGenerator	thumbGen;
		private Extractor			extractor;

		// note:
		// do not allow to modify the constuctor parameters 
		// (i.e. database, options)
		// through public properties later, since the scanner 
		// may already use them after scanning has been started,
		// and some stuff has been initialized depending on the 
		// options in the ctor already.
		public FilesystemVolumeScanner(Platform.Common.IO.DriveInfo drive,
		                               VolumeDatabase database,
		                               FilesystemScannerOptions options)
			: base(drive, database, options)
		{
		
			if (!drive.IsMounted)
				throw new ArgumentException("Drive is not mounted", "drive");
			
			if (Options.GenerateThumbnails && string.IsNullOrEmpty(Options.DbDataPath))
				throw new ArgumentException("DbDataPath",
				                            "Thumbnail generation requires the DbDataPath option to be set");
			
			disposed				= false;
			//this.mimeInfo			  = new MimeInfo(false);
			this.sbPathFixer		= new StringBuilder(1024);
			this.paths				= new Paths(Options.DbDataPath, null, null);
			this.symLinkHelper		= new SymLinkHelper(this);
			this.thumbGen			= new ThumbnailGenerator();

			this.extractor			= null;
			if (Options.ExtractMetaData) {
				try {
					this.extractor	= Extractor.GetDefault();
					
					if (Options.ExtractionBlacklist != null) {
						foreach (string ext in Options.ExtractionBlacklist)
							this.extractor.RemoveLibrary("libextractor_" + ext);
					}
				} catch (DllNotFoundException) {
					// a warning will be sent in ScanningThreadMain().
				}
			}
		}
		
		internal override void ScanningThreadMain(Platform.Common.IO.DriveInfo drive,
		                                          FileSystemVolume volume,
		                                          BufferedVolumeItemWriter writer) {
			try {
				if (Options.GenerateThumbnails) {
					paths.volumeDataPath = DbData.CreateVolumeDataPath(paths.dbDataPath, volume.VolumeID);
					paths.thumbnailPath = DbData.CreateVolumeDataThumbsPath(paths.volumeDataPath);
				}

				if (Options.ExtractMetaData && (extractor == null)) {
					SendScannerWarning(S._("libExtractor not found. Metadata extraction disabled."));
				}
				
				string rootPath = drive.RootPath;
				// remove possible ending path seperator except for _system_ root paths
				rootPath = RemoveEndingSlash(rootPath);
	//			  if ((rootPath.Length > 1) && (rootPath[rootPath.Length - 1] == Path.DirectorySeparatorChar))
	//				  rootPath = rootPath.Substring(0, rootPath.Length - 1);
				
				// make sure the root path exists
				// (media may have been removed after scanner construction)
				if (!Directory.Exists(rootPath))
					throw new DirectoryNotFoundException("Root path does not exist");
				
				DirectoryInfo dir = new DirectoryInfo(rootPath);				
				RecursiveDump(rootPath, dir, writer, VolumeDatabase.ID_NONE);
				symLinkHelper.InsertSymLinkItems(writer, volume.VolumeID);
				
				volume.SetFileSystemVolumeFields(VolumeInfo.Files, VolumeInfo.Directories, VolumeInfo.Size);
			} catch (Exception) {
				// try to cleanup
				try {
					if((paths.volumeDataPath != null) && Directory.Exists(paths.volumeDataPath))
						Directory.Delete(paths.volumeDataPath, true);
				} catch (Exception) { /* just shut up */ }
				
				// rethrow initial exception
				throw;
			}
		}
		
		protected override void Reset() {
			//m_rootID = 0;
			//Media.SetFilesystemMediaFields(0, -1, 0); // -1 : subtract root dir
			symLinkHelper.Clear();
			
			base.Reset();
		}
		
		protected override void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					thumbGen.Dispose();

					if (extractor != null)
						extractor.Dispose();
				}

				thumbGen		= null;
				extractor		= null;
				sbPathFixer		= null;
				paths			= null;
				symLinkHelper	= null;
			}
			disposed = true;
			
			base.Dispose(disposing);
		}
		
		private void RecursiveDump(string rootPath,
		                           DirectoryInfo dir,
		                           BufferedVolumeItemWriter writer,
		                           long parentID) {
			
			CheckForCancellationRequest();
			
		   /* event is called before a _directory_ item is about to be scanned only. 
			* it could also be called everytime a _file_ item is about to be scanned, 
			* but this could result in a performance loss.
			*/
			PostBeforeScanItem(dir.FullName); //OnBeforeScanItem(new BeforeScanItemEventArgs(dir.FullName));

//			  bool dirIsSymLink = false;
//			  string symLinkTarget = null;
			FileType ft;
			
			// catch possible FileNotFoundExceptions
			// (e.g. on filesystems with wrong filename encoding or vanishing virtual files in /dev).
			try {
				ft = FileHelper.GetFileType(dir.FullName, false);
			} catch (FileNotFoundException ex) {
				/* may throw ScanCancelledException */
				SendScannerWarning(string.Format(S._("Directory '{0}' not found. (Wrong filename encoding?)"),
				                                 dir.FullName), ex);
				return;
			}
				
			bool dirIsSymLink = (ft == FileType.SymbolicLink);

			if ((ft != FileType.Directory) && !dirIsSymLink) {
				/* may throw ScanCancelledException */
				SendScannerWarning(string.Format(S._("Skipped item '{0}' as it doesn't seem to be a real directory."),
				                                 dir.FullName));
				return;    
			}
			
			if (dirIsSymLink) {
				if (!Options.DiscardSymLinks) {
					
					string symLinkTarget = null;
					
					try {
						// get real path with all symlinks resolved
						symLinkTarget = FileHelper
							.GetCanonicalSymLinkTarget(dir.FullName);
					} catch (FileNotFoundException) {}
					
					// Note:
					// this check seems to be useless since a broken link 
					// to a directory is identified as a broken link to a _file_ (a few lines below).
					if (symLinkTarget == null) {
						/* may throw ScanCancelledException */
						SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as the target does not exist."),
						                                 dir.FullName));
						return;
					}
					
					// skip symlinks outside of rootPath 
					// (in addition, GetLocation()/FixPath() need paths relative to rootPath)
					if (!symLinkTarget.StartsWith(rootPath)) {
						/* may throw ScanCancelledException */
						SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as it appears to point to a different drive ('{1}')."),
						                                 dir.FullName,
						                                 symLinkTarget));
						return;
					}
					
					symLinkHelper.AddSymLink(dir, symLinkTarget, rootPath, parentID, true);
				}
				/* do not dump symlinks to directories */
				return;
			}
				
			/* insert dirname */
			long dirID = InsertDir(rootPath, dir, writer, parentID);
			parentID = dirID;
			// TODO : check m_cancel here (?)
			
//			  /* do not dump symlinks to directories */
//			  if (dirIsSymlink)
//				  return;
				
			try {
				/* insert files of dir */
				FileInfo[] files = dir.GetFiles(); /* throws access exceptions (cant access _DIRECTORY_) */
				for (int i = 0; i < files.Length; i++) {
					CheckForCancellationRequest();

//					  bool isRegularFile  = true;
//					  bool isSymLink	  = false;
					
#if DEBUG && DEBUG_FILE_VERBOSE
					Debug.WriteLine(string.Format("Indexing file '{0}'", files[i].FullName));
#endif
					// catch possible FileNotFoundExceptions
					// (e.g. on filesystems with wrong filename encoding or vanishing virtual files in /dev).
					try {
						ft = FileHelper.GetFileType(files[i].FullName, false);
					} catch (FileNotFoundException ex) {
						/* may throw ScanCancelledException */
						SendScannerWarning(string.Format(S._("File '{0}' not found. (Wrong filename encoding?)"), 
						                                 files[i].FullName), ex);
						continue;
					}
					
					/* special files (fifos, blockdevices, chardevices) are skipped */
					bool isRegularFile	 = (ft == FileType.RegularFile);
					bool isSymLink		 = (ft == FileType.SymbolicLink);

					if (isRegularFile) {
						
						string	mimeType		= null;
						string	metaData		= null;
						string	hash			= null;
						bool	thumbGenerated	= false;
						
						FileStream fs = null;
						try {
							// OpenRead() must be called _before_ MimeInfo.GetMimeType(),
							// since this method returns a mimetype even if the file does not exist / can't be accessed.
							fs = File.OpenRead(files[i].FullName); /* throws access/IO exceptions (cant access _FILE_) */
							
							mimeType = MimeType.GetMimeTypeForFile(files[i].FullName);
							
							if (Options.ExtractMetaData && (extractor != null)) {
								Keyword[] keywords = extractor.GetKeywords(files[i].FullName);
								// removes duplicates like the same year in idv2 and idv3 tags,
								// does not remove keywords of the same type with different data (e.g. filename)
								keywords = Extractor.RemoveDuplicateKeywords(keywords, DuplicateOptions.DUPLICATES_REMOVE_UNKNOWN);
								// removes whitespace-only keywords
								keywords = Extractor.RemoveEmptyKeywords(keywords);
								metaData = MetaDataHelper.PackExtractorKeywords(keywords);
							}
							
							if (Options.ComputeHashs) {
								hash = ComputeHash(fs);
								// TODO : check m_cancel here? hashing can be a lengthy operation on big files.
							}
							
							if (Options.GenerateThumbnails) {
								thumbGenerated = thumbGen.GenerateThumbnail(files[i], mimeType);
							}
								
						} catch (Exception e) {
							// ### exception caught: hash, mime and/or metadata may be null 
							// and the thumbnail may not have been generated!
							if (e is UnauthorizedAccessException || e is IOException) {
								/* may throw ScanCancelledException */
								SendScannerWarning(string.Format(S._("Error opening file '{0}', can't retrieve any mime/metadata. ({1})"),
								                                 files[i].FullName,
								                                 e.Message),
								                   e);
							} else {
								throw;								  
							}
						} finally {
							if (fs != null)
								fs.Close();
						}
						
						long fileID = InsertFile(rootPath, files[i], writer, parentID, mimeType, metaData, hash);
						if (thumbGenerated)
							thumbGen.SaveThumbnail(Path.Combine(paths.thumbnailPath, string.Format("{0}.png", fileID)));
						
					} else if (isSymLink) {
						
						if (!Options.DiscardSymLinks) {
							
							string symLinkTarget = null;
							
							try {
								// get real path with all symlinks resolved
								symLinkTarget = FileHelper
									.GetCanonicalSymLinkTarget(files[i].FullName);
							} catch (FileNotFoundException) {}
							
							if (symLinkTarget == null) {
								/* may throw ScanCancelledException */
								SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as the target does not exist."),
								                                 files[i].FullName));
							
							// skip symlinks outside of rootPath
							// (in addition, GetLocation()/FixPath() need paths relative to rootPath)
							} else if (!symLinkTarget.StartsWith(rootPath)) {
								/* may throw ScanCancelledException */
								SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as it appears to point to a different drive ('{1}')."),
								                                 files[i].FullName,
								                                 symLinkTarget));
							
							// skip symlinks pointing to special files (only regular files are indexed)
							} else if (FileHelper.GetFileType(symLinkTarget, false) != FileType.RegularFile) {
								/* may throw ScanCancelledException */
								SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as it does not point to a regular file ('{1}')."),
								                                 files[i].FullName,
								                                 symLinkTarget));
							} else {
								symLinkHelper.AddSymLink(files[i], symLinkTarget, rootPath, parentID, false);
							}
						}
					} else {
						/* may throw ScanCancelledException */
						SendScannerWarning(string.Format(S._("Skipped item '{0}' as it appears to be some kind of special file."),
						                                 files[i].FullName));
					}
					
					// TODO : check m_cancel here (?)
					
				}  // end for
				
				/* recursively dump subdirs */
				DirectoryInfo[] childDirs = dir.GetDirectories(); /* throws access exceptions (cant access _DIRECTORY_) */
				for (int i = 0; i < childDirs.Length; i++)
					RecursiveDump(rootPath, childDirs[i], writer, parentID);

			} catch (UnauthorizedAccessException e) {
				//ScannerWarningEventArgs args = new ScannerWarningEventArgs("Unable to dump dir '" + dir.FullName + "'. (" + e.Message + ")", e);
				//OnScannerWarning(args); // may throw ScanCancelledException

				/* may throw ScanCancelledException */
				SendScannerWarning(string.Format(S._("Unable to dump dir '{0}'. ({1})"),
				                                 dir.FullName,
				                                 e.Message),
				                   e);
			}
		}
		
		private long InsertDir(string rootPath,
		                       DirectoryInfo dir,
		                       BufferedVolumeItemWriter writer,
		                       long parentID) {
		   /* if scanner has no db associated, just update the counters
			* and return */
			if (!this.HasDB) {
				// TODO :
				// increase dircounter for symlink to dirs as well?
				// nautilus refers to selected symlinks to dirs as dirs too.
				Interlocked.Increment(ref VolumeInfo.directories);
				return VolumeDatabase.ID_NONE;
			}
			
			string location;
			string name;
			
			/* if parentID is ID_NONE, the directory is the volumes root dir 
			 * -> location = null, name = "/" (analog System.IO.DirectoryInfo)
			 */
			if (parentID == VolumeDatabase.ID_NONE) {
				location	= null;
				name		= PATH_SEPARATOR.ToString();
			} else {				
				location	= GetLocation(dir.FullName, rootPath);
				name		= dir.Name;
			}
			
			DateTime lastWriteTime = GetLastWriteTime(dir);

			DirectoryVolumeItem item = GetNewVolumeItem<DirectoryVolumeItem>(parentID,
			                                                                 name,
			                                                                 MIME_TYPE_DIRECTORY,
			                                                                 null,
			                                                                 VolumeItemType.DirectoryVolumeItem);
			
			item.SetFileSystemVolumeItemFields(location, lastWriteTime, VolumeDatabase.ID_NONE);
			//item.Name = name; // set the items name (defined on VolumeItem baseclass)
 
//			  if (isSymlink) {
//				  /* don't dump symlink dirs directly into the database, 
//				   * they're required to have a target item assigned.
//				   * target items are resolved in an additional step.
//				   */
//				   symLinkItems.add(symLinkTarget, item);
//			  } else {
				writer.Write(item);
//			  }

			// TODO :
			// increase dircounter for symlink to dirs as well?
			// nautilus refers to selected symlinks to dirs as dirs too.
		   Interlocked.Increment(ref VolumeInfo.directories);
		   
			if (!Options.DiscardSymLinks)
				symLinkHelper.AddFile(dir.FullName, item.ItemID);
			
		   return item.ItemID;
		}

		private long InsertFile(string rootPath,
		                        FileInfo file,
		                        BufferedVolumeItemWriter writer,
		                        long parentID,
		                        string mimeType,
		                        string metaData,
		                        string hash) {
			/* if scanner has no db associated, just update the counters
			 * and return 
			 */
			if (!this.HasDB) {
				Interlocked.Increment(ref VolumeInfo.files);
				Interlocked.Add(ref VolumeInfo.size, file.Length);
				return VolumeDatabase.ID_NONE;
			}
			
			DateTime lastWriteTime = GetLastWriteTime(file);
			
			FileVolumeItem item = GetNewVolumeItem<FileVolumeItem>(parentID,
			                                                       file.Name,
			                                                       mimeType,
			                                                       metaData,
			                                                       VolumeItemType.FileVolumeItem);
			
			item.SetFileSystemVolumeItemFields(GetLocation(file.FullName, rootPath),
			                                   lastWriteTime,
			                                   VolumeDatabase.ID_NONE);
			
			item.SetFileVolumeItemFields(file.Length, hash);
			//item.Name = file.Name; // set the items name (defined on VolumeItem baseclass)
			
			writer.Write(item);
			
			Interlocked.Increment(ref VolumeInfo.files);
			Interlocked.Add(ref VolumeInfo.size, file.Length);
			
			if (!Options.DiscardSymLinks)
				symLinkHelper.AddFile(file.FullName, item.ItemID);
			
			return item.ItemID;
		}

		private DateTime GetLastWriteTime(FileSystemInfo f) {
			DateTime lastWriteTime;
			// TODO : LastWriteTime fails on folders burned on CD (both, .net and mono).
			// If it doesn't anymore this function can be removed.
			try {
				lastWriteTime = f.LastWriteTime;
			} catch (ArgumentOutOfRangeException e) {
				lastWriteTime = DateTime.MinValue;

				/* may throw ScanCancelledException */
				SendScannerWarning(string.Format(S._("Can't read LastWriteTime from item '{0}' ({1})."),
				                                 f.FullName,
				                                 e.Message));
			}
			return lastWriteTime;
		}
		
		// returns the location of a file/dir and fixes DirectorySeperatorChars
		// NOTE: requires a path _relative_ to rootPath!
		private string GetLocation(string fullName, string rootPath) {
			// remove possible ending slash from dirs
			fullName = RemoveEndingSlash(fullName);
//			  if ((fullName[fullName.Length - 1] == Path.DirectorySeparatorChar) && (fullName.Length > 1))
//				  fullName = fullName.Substring(0, fullName.Length - 1);
			
			// check if the path is the rootPath
			if (fullName.Length == rootPath.Length)
				return string.Empty;
			
			string dirName = Path.GetDirectoryName(fullName);
			return FixPath(dirName, rootPath); 
		}
		
		// removes rootPath and fixes DirectorySeperatorChars
		// NOTE: requires a path _relative_ to rootPath!
		private string FixPath(string fullName, string rootPath) {
			// TODO :  test under win32 and linux
			
			// remove possible ending slash from dirs
			fullName = RemoveEndingSlash(fullName);
//			  if ((fullName[fullName.Length - 1] == Path.DirectorySeparatorChar) && (fullName.Length > 1))
//				  fullName = fullName.Substring(0, fullName.Length - 1);
			
			// check if the path is the rootPath
			if (fullName.Length == rootPath.Length)
				return PATH_SEPARATOR.ToString();
			
			bool rootPathEqualsDirSeperator = (rootPath.Length == 1 && rootPath[0] == Path.DirectorySeparatorChar);
			
			// if path is seperated by our PATH_SEPERATOR...
			if (Path.DirectorySeparatorChar == PATH_SEPARATOR) {
				// ... just remove rootPath (if it doesn't equal the dir seperator by accident)
				if (!rootPathEqualsDirSeperator)
					fullName = fullName.Substring(rootPath.Length);
#if DEBUG
				System.Diagnostics.Debug.Assert(fullName[0] == PATH_SEPARATOR);
#endif					  
				return fullName;
			} else { // path is NOT seperated by our PATH_SEPERATOR...
				// reset stringbuilder
				sbPathFixer.Length = 0;
				// store fullname
				sbPathFixer.Append(fullName);
				// remove rootPath
				if (!rootPathEqualsDirSeperator) {
					sbPathFixer.Remove(0, rootPath.Length);
					sbPathFixer.Insert(0, PATH_SEPARATOR);
				}
				
				// replace platform dependent DirectorySeparatorChar by PATH_SEPERATOR
				sbPathFixer.Replace(Path.DirectorySeparatorChar, PATH_SEPARATOR);
#if DEBUG
				string s = sbPathFixer.ToString();
				System.Diagnostics.Debug.Assert(s[0] == PATH_SEPARATOR);
				System.Diagnostics.Debug.Assert(s.IndexOf(Path.DirectorySeparatorChar) == -1);
				return s;
#else
				return sbPathFixer.ToString();
#endif
			}		  
		}
		
		private static string RemoveEndingSlash(string path) {
			// remove ending path separator from dirs, 
			// except for _system_ root paths ("/" on unix, "C:\", "D:\", ... on windows)
			// (esp. important on windows as e.g. "D:" won't work with DirectoryInfo)
			if ((path[path.Length - 1] == Path.DirectorySeparatorChar) && (path != Path.GetPathRoot(path)))
				return path.Substring(0, path.Length - 1);
			else
				return path;
		}
		
		private static string ComputeHash(Stream s) {
			StringBuilder sb = new StringBuilder(); // TODO : define at class level like sbPathFixer?
			//using (FileStream fs = File.OpenRead(filePath)) {
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				byte[] hash = md5.ComputeHash(s);
				foreach (byte b in hash)
					sb.Append(b.ToString("X2"));
			//}
			return sb.ToString();
		}
		
#region SymLinkHelper class
		private class SymLinkHelper
		{
			private FilesystemVolumeScanner scanner;
			private Dictionary<string, long> files;
			private List<SymLinkItem> symLinkItems;
			
			public SymLinkHelper(FilesystemVolumeScanner scanner) {
				this.scanner		= scanner;
				this.files			= new Dictionary<string, long>();
				this.symLinkItems	= new List<SymLinkItem>();
			}
			
			public void AddFile(string path, long id) {
				files.Add(path, id);
			}
			
			public void AddSymLink(FileSystemInfo symLink,
			                       string fullTargetPath,
			                       string rootPath,
			                       long parentID,
			                       bool isDir) {
				
				SymLinkItem s = new SymLinkItem();
				s.parentID			= parentID;
				s.name				= symLink.Name;
				s.location			= scanner.GetLocation(symLink.FullName, rootPath);
				s.fullPath			= symLink.FullName;
				s.fullTargetPath	= fullTargetPath;
				s.isDir				= isDir;
				
				symLinkItems.Add(s);
			}
			
			public void Clear() {
				files.Clear();
				symLinkItems.Clear();
			}
			
			public void InsertSymLinkItems(BufferedVolumeItemWriter writer, long volumeID) {
				if (symLinkItems.Count == 0)
					return;
				
				/* if scanner has no db associated, just update the counters
				 * and return */
				if (!scanner.HasDB) {
					foreach(SymLinkItem sli in symLinkItems) {
						if (sli.isDir)
							Interlocked.Increment(ref scanner.VolumeInfo.directories);
						else
							Interlocked.Increment(ref scanner.VolumeInfo.files);
						
						// TODO : 
						// increase totalsize by size of symlinks too? (not size of target!)
						// or are symlinks as big as dirs, those aren't respected as well.. 
						//Interlocked.Add(ref VolumeInfo.size, sli.size);
					}
					return;
				}
				
				// make sure all files/dirs have been written to the database 
				// before searching for symlink targets.
				writer.Flush();
				
				foreach (SymLinkItem sli in symLinkItems) {
					
					scanner.CheckForCancellationRequest();
				
					long itemID;
					if (!files.TryGetValue(sli.fullTargetPath, out itemID)) {
						/* may throw ScanCancelledException */
						scanner.SendScannerWarning(string.Format(S._("Failed to resolve target item for symlink '{0}'."),
						                                 sli.fullPath));
					} else {						
						SearchCriteriaGroup g = new SearchCriteriaGroup(MatchRule.AllMustMatch);
						g.AddSearchCriteria(new IDSearchCriteria(volumeID, IDSearchField.VolumeID, CompareOperator.Equal));
						g.AddSearchCriteria(new IDSearchCriteria(itemID, IDSearchField.ItemID, CompareOperator.Equal));
						
						// query target item.
						// async BeginItemSearch() won't work here
						// (active transaction prevents other threads from accessing the database)
						VolumeItem[] queriedItems = scanner.Database.SearchItem(g);
						
						FileSystemVolumeItem targetItem = (FileSystemVolumeItem)queriedItems[0];
						FileSystemVolumeItem newItem;
						
						if (targetItem is FileVolumeItem) {
							newItem = scanner.GetNewVolumeItem<FileVolumeItem>(sli.parentID,
							                                                   sli.name,
							                                                   targetItem.MimeType,
							                                                   targetItem.MetaData,
							                                                   VolumeItemType.FileVolumeItem);
							
							((FileVolumeItem)newItem).SetFileVolumeItemFields( ((FileVolumeItem)targetItem).Size,
							                                                  ((FileVolumeItem)targetItem).Hash);
							
							Interlocked.Increment(ref scanner.VolumeInfo.files);
							
						} else { // DirectoryVolumeItem
							newItem = scanner.GetNewVolumeItem<DirectoryVolumeItem>(sli.parentID,
							                                                        sli.name,
						                                                            targetItem.MimeType,
						                                                            targetItem.MetaData,
						                                                            VolumeItemType.DirectoryVolumeItem);
							
							Interlocked.Increment(ref scanner.VolumeInfo.directories);
						}
						
						newItem.SetFileSystemVolumeItemFields(sli.location,
						                                      targetItem.LastWriteTime,
						                                      targetItem.ItemID);
					
						writer.Write(newItem);
						
						// TODO : 
						// increase totalsize by size of symlinks too? (not size of target!)
						// or are symlinks as big as dirs, those aren't respected as well.. 
						//Interlocked.Add(ref VolumeInfo.size, sli.size);
#if DEBUG
						Debug.WriteLine("Successfully resolved and saved symlink item: {0}/{1} -> {2}/{3}",
						                (sli.location == PATH_SEPARATOR.ToString() ? "" : sli.location),
						                sli.name,
						                (targetItem.Location == PATH_SEPARATOR.ToString() ? "" : targetItem.Location),
						                (targetItem.Name == PATH_SEPARATOR.ToString() ? "" : targetItem.Name));
#endif
					} // end if
				} // end foreach
			}
			
			private class SymLinkItem
			{
				public long parentID;
				public string name;
				public string location;
				public string fullPath;
				public string fullTargetPath;
				public bool isDir;
			}
		}
#endregion
		
		private class Paths
		{
			public string dbDataPath;
			public string volumeDataPath;
			public string thumbnailPath;
			
			public Paths(string dbDataPath, string volumeDataPath, string thumbnailPath) {
				this.dbDataPath		= dbDataPath;
				this.volumeDataPath	= volumeDataPath;
				this.thumbnailPath	= thumbnailPath;
			}
		}
		
	}
}
