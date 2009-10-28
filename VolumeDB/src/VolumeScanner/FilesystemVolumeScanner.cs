// FilesystemVolumeScanner.cs
// 
// Copyright (C) 2008 Patrick Ulbrich
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
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Collections.Generic;
using Platform.Common;
using Platform.Common.IO;
using Platform.Common.Mime;
using VolumeDB.Searching;
using VolumeDB.Searching.ItemSearchCriteria;
using LibExtractor;

namespace VolumeDB.VolumeScanner
{
	// TODO : EnsureOpen() in public members?
	// TODO : override Dispose(bool) to e.g dispose/set null m_sbPathfixer?
	public sealed class FilesystemVolumeScanner : VolumeScannerBase<FileSystemVolume, FilesystemVolumeInfo>
	{
		private const char		PATH_SEPARATOR		= '/';
		private const string	MIME_TYPE_DIRECTORY = "x-directory/normal";
		
		private bool				disposed;
		//private MimeInfo			  mimeInfo;
		private StringBuilder		sbPathFixer;
		private List<SymLinkItem>	symLinkItems;
		private bool				discardSymLinks;
		private bool				generateThumbnails;
		private bool				extractMetaData;
		private Paths				paths;
		private ThumbnailGenerator	thumbGen;
		private Extractor			extractor;

		// note:
		// do not allow to modify the constuctor parameters (i.e. discardSymlinks, generateThumbnails, dbDataPath)
		// through public properties later, since the scanner may already use them after scanning has been started.
		public FilesystemVolumeScanner(string device, VolumeDatabase database, int bufferSize, bool computeHashs)
			: this(device, database, bufferSize, computeHashs, false, false, false, null) {}
		
		public FilesystemVolumeScanner(string device, VolumeDatabase database, int bufferSize, bool computeHashs, bool discardSymLinks, bool generateThumbnails, bool extractMetaData, string dbDataPath)
			: base(device, true, database, bufferSize, computeHashs)
		{
		
			if (generateThumbnails && string.IsNullOrEmpty(dbDataPath))
				throw new ArgumentException("dbDataPath", "Thumbnail generation requires dbDataPath to be set");
			
			disposed				= false;
			//this.mimeInfo			  = new MimeInfo(false);
			this.sbPathFixer		= new StringBuilder(1024);
			this.symLinkItems		= new List<SymLinkItem>();
			this.discardSymLinks	= discardSymLinks;			
			this.generateThumbnails	= generateThumbnails;
			this.extractMetaData	= extractMetaData;
			this.paths				= new Paths(dbDataPath, null, null);
			this.thumbGen			= new ThumbnailGenerator();

			this.extractor			= null;
			if (extractMetaData) {
				try {
					this.extractor	= Extractor.GetDefault();
				} catch(DllNotFoundException) {
					// a warning will be sent in ScanningThreadMain().
				}
			}
		}
		
		internal override void ScanningThreadMain(Platform.Common.IO.DriveInfo driveInfo, FileSystemVolume volume, BufferedVolumeItemWriter writer, bool computeHashs) {
			try {
				if (generateThumbnails) {
					paths.volumeDataPath = Path.Combine(paths.dbDataPath, volume.VolumeID.ToString());
					
					// make sure there is no directory with the same name as the volume directory 
					// that is about to be created
					// (the volume directory will be deleted in the catch block on failure, 
					// so make sure that no existing dir will be deleted)
					if (Directory.Exists(paths.volumeDataPath))
						throw new ArgumentException("dbDataPath already contains a directory for this volume");
					
					// thumbnails will be stored in <dbdataPath>/<volumeID>/thumbs
					paths.thumbnailPath = Path.Combine(paths.volumeDataPath, "thumbs");
					Directory.CreateDirectory(paths.thumbnailPath);
				}

				if (extractMetaData && extractor == null) {
					SendScannerWarning(S._("libExtractor not found. Metadata extraction disabled."));
				}
				
				string rootPath = driveInfo.RootPath;
				// remove possible ending path seperator except for _system_ root paths
				rootPath = RemoveEndingSlash(rootPath);
	//			  if ((rootPath.Length > 1) && (rootPath[rootPath.Length - 1] == Path.DirectorySeparatorChar))
	//				  rootPath = rootPath.Substring(0, rootPath.Length - 1);
				
				DirectoryInfo dir = new DirectoryInfo(rootPath);				
				RecursiveDump(rootPath, dir, writer, computeHashs, VolumeDatabase.ID_NONE);
				InsertSymLinkItems(writer, volume.VolumeID);
				
				volume.SetFileSystemVolumeFields(VolumeInfo.Files, VolumeInfo.Directories, VolumeInfo.Size);
			} catch(Exception) {
				// try to cleanup
				try {
					if((paths.volumeDataPath != null) && Directory.Exists(paths.volumeDataPath))
						Directory.Delete(paths.volumeDataPath, true);
				} catch(Exception) { /* just shut up */ }
				
				// rethrow initial exception
				throw;
			}
		}
		
		protected override void Reset() {
			//m_rootID = 0;
			//Media.SetFilesystemMediaFields(0, -1, 0); // -1 : subtract root dir
			symLinkItems.Clear();
			
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
				symLinkItems	= null;
				paths			= null;
			}
			disposed = true;
			
			base.Dispose(disposing);
		}
		
		private void RecursiveDump(string rootPath, DirectoryInfo dir, BufferedVolumeItemWriter writer, bool computeHashs, long parentID) {
			CheckForCancellationRequest();
			
		   /* event is called before a _directory_ item is about to be scanned only. 
			* it could also be called everytime a _file_ item is about to be scanned, 
			* but this could result in a performance loss.
			*/
			PostBeforeScanItem(dir.FullName); //OnBeforeScanItem(new BeforeScanItemEventArgs(dir.FullName));

//			  bool dirIsSymLink = false;
//			  string symLinkTarget = null;
			FileType ft;
			
			// TODO : catch FileNotFounException? when is it thrown? (e.g. at /dev/fd/21)
			// is this the case withwith "dead" symlinks (only)?
			ft = FileHelper.GetFileType(dir.FullName, false);
				
			bool dirIsSymLink = (ft == FileType.SymbolicLink);

			if ((ft != FileType.Directory) && !dirIsSymLink) {
				/* may throw ScanCancelledException */
				SendScannerWarning(string.Format(S._("Skipped item '{0}' as it doesn't seem to be a real directory."), dir.FullName));
				return;    
			}
			
			if (dirIsSymLink) {
				if (!discardSymLinks) {
					string symLinkTarget = GetFullSymLinkTargetPath(FileHelper.GetSymLinkTarget(dir.FullName), Path.GetDirectoryName(dir.FullName)); // TODO : may this throw an exception when accessing dead symlinks? (same question appears a couple of lines below). Edit: apparently not.

					if (!Directory.Exists(symLinkTarget)) {
						/* may throw ScanCancelledException */
						SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as the target does not exist."), dir.FullName));
						return;
					}
					
					if (!symLinkTarget.StartsWith(rootPath)) {	// skip symlinks outside of rootPath (in addition, GetLocation()/FixPath() need paths relative to rootPath)
						/* may throw ScanCancelledException */
						SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as it appears to point to a different drive ('{1}')."), dir.FullName, symLinkTarget));
						return;
					}
					
					//symLinkItems.add(new SymLinkItem(parentID, dir.FullName, dir.Name, GetLocation(dir.FullName, rootPath), FixPath(symLinkTarget, rootPath), true));
					symLinkItems.Add(SymLinkItem.CreateInstance(dir, symLinkTarget, parentID, true, rootPath, this));
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
					
					// TODO : catch FileNotFounException? when is it thrown? (e.g. at /dev/fd/21)
					ft = FileHelper.GetFileType(files[i].FullName, false);

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
							
							if (extractMetaData && (extractor != null)) {
								Keyword[] keywords = extractor.GetKeywords(files[i].FullName);
								// removes duplicates like the same year in idv2 and idv3 tags,
								// does not remove keywords of the same type with different data (e.g. filename)
								keywords = Extractor.RemoveDuplicateKeywords(keywords, DuplicateOptions.DUPLICATES_REMOVE_UNKNOWN);
								// removes whitespace-only keywords
								keywords = Extractor.RemoveEmptyKeywords(keywords);
								metaData = MetaDataHelper.PackExtractorKeywords(keywords);
							}
							
							if (computeHashs) {
								hash = ComputeHash(fs);
								// TODO : check m_cancel here? hashing can be a lengthy operation on big files.
							}
							
							if (generateThumbnails) {
								thumbGenerated = thumbGen.GenerateThumbnail(files[i], mimeType);
							}
								
						} catch (Exception e) {
							// ### exception caught: hash, mime and/or metadata may be null 
							// and the thumbnail may not have been generated!
							if (e is UnauthorizedAccessException || e is IOException) {
								/* may throw ScanCancelledException */
								SendScannerWarning(string.Format(S._("Error opening file '{0}', can't retrieve any mime/metadata. ({1})"), files[i].FullName, e.Message), e);
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
						
						if (!discardSymLinks) {
							string symLinkTarget = GetFullSymLinkTargetPath(FileHelper.GetSymLinkTarget(files[i].FullName), dir.FullName);	// TODO : may this throw an exception when accessing dead symlinks?
							
							// TODO : remove this fix when the bug has been fixed in the next ubuntu mono package							 
							// see https://bugzilla.novell.com/show_bug.cgi?id=385765
							if (Directory.Exists(symLinkTarget)) { /* START fix to bug #385765 */
								if (!symLinkTarget.StartsWith(rootPath)) {	// skip symlinks outside of rootPath (in addition, GetLocation()/FixPath() need paths relative to rootPath)
									/* may throw ScanCancelledException */
									SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as it appears to point to a different drive ('{1}')."), files[i].FullName, symLinkTarget));
								} else {
										symLinkItems.Add(SymLinkItem.CreateInstance(files[i], symLinkTarget, parentID, true, rootPath, this));
								}
							} else { /* END fix to bug #385765 */							 
								
								if (!File.Exists(symLinkTarget)) {
									/* may throw ScanCancelledException */
									SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as the target does not exist."), files[i].FullName));
								} else if (!symLinkTarget.StartsWith(rootPath)) { // skip symlinks outside of rootPath (in addition, GetLocation()/FixPath() need paths relative to rootPath)
									/* may throw ScanCancelledException */
									SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as it appears to point to a different drive ('{1}')."), files[i].FullName, symLinkTarget));
								} else if (FileHelper.GetFileType(symLinkTarget, false) != FileType.RegularFile) { // also skipps symlinks pointing to symlinks (hard to implement)
									/* may throw ScanCancelledException */
									SendScannerWarning(string.Format(S._("Skipped symlink item '{0}' as it does not point to a regular file ('{1}')."), files[i].FullName, symLinkTarget));
								} else {
									//symLinkItems.add(new SymLinkItem(parentID, files[i].fullName, files[i].Name, GetLocation(files[i].FullName, rootPath), FixPath(symLinkTarget, rootPath), false));
									symLinkItems.Add(SymLinkItem.CreateInstance(files[i], symLinkTarget, parentID, false, rootPath, this));
								}
							}
						}
					} else {
						/* may throw ScanCancelledException */
						SendScannerWarning(string.Format(S._("Skipped item '{0}' as it appears to be some kind of special file."), files[i].FullName));
					}
					// TODO : check m_cancel here (?)
				}
				
				/* recursively dump subdirs */
				DirectoryInfo[] childDirs = dir.GetDirectories(); /* throws access exceptions (cant access _DIRECTORY_) */
				for (int i = 0; i < childDirs.Length; i++)
					RecursiveDump(rootPath, childDirs[i], writer, computeHashs, parentID);

			} catch (UnauthorizedAccessException e) {
				//ScannerWarningEventArgs args = new ScannerWarningEventArgs("Unable to dump dir '" + dir.FullName + "'. (" + e.Message + ")", e);
				//OnScannerWarning(args); // may throw ScanCancelledException

				/* may throw ScanCancelledException */
				SendScannerWarning(string.Format(S._("Unable to dump dir '{0}'. ({1})"), dir.FullName, e.Message), e);
			}
		}
		
		private long InsertDir(string rootPath, DirectoryInfo dir, BufferedVolumeItemWriter writer, long parentID) {
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

			DirectoryVolumeItem item = GetNewVolumeItem<DirectoryVolumeItem>(parentID, name, MIME_TYPE_DIRECTORY, null, VolumeItemType.DirectoryVolumeItem);
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
		   
		   return item.ItemID;
		}

		private long InsertFile(string rootPath, FileInfo file, BufferedVolumeItemWriter writer, long parentID, string mimeType, string metaData, string hash) {
			/* if scanner has no db associated, just update the counters
			 * and return 
			 */
			if (!this.HasDB) {
				Interlocked.Increment(ref VolumeInfo.files);
				Interlocked.Add(ref VolumeInfo.size, file.Length);
				return VolumeDatabase.ID_NONE;
			}
			
			DateTime lastWriteTime = GetLastWriteTime(file);
			
			FileVolumeItem item = GetNewVolumeItem<FileVolumeItem>(parentID, file.Name, mimeType, metaData, VolumeItemType.FileVolumeItem);
			item.SetFileSystemVolumeItemFields(GetLocation(file.FullName, rootPath), lastWriteTime, VolumeDatabase.ID_NONE);
			item.SetFileVolumeItemFields(file.Length, hash);
			//item.Name = file.Name; // set the items name (defined on VolumeItem baseclass)
			
			writer.Write(item);
			
			Interlocked.Increment(ref VolumeInfo.files);
			Interlocked.Add(ref VolumeInfo.size, file.Length);
			
			return item.ItemID;
		}
		
		private void InsertSymLinkItems(BufferedVolumeItemWriter writer, long volumeID) {
			if (symLinkItems.Count == 0)
				return;
			
			/* if scanner has no db associated, just update the counters
			 * and return */
			if (!this.HasDB) {
				foreach(SymLinkItem sli in symLinkItems) {
					if (sli.isDir)
						Interlocked.Increment(ref VolumeInfo.directories);
					else
						Interlocked.Increment(ref VolumeInfo.files);
					
					// TODO : 
					// increase totalsize by size of symlinks too? (not size of target!)
					// or are symlinks as big as dirs, those aren't respected as well.. 
					//Interlocked.Add(ref VolumeInfo.size, sli.size);
				}
				return;
			}
			
			// make sure all files/dirs have been written to the database 
			// before searching for symlink targets
			writer.Flush();
			
			const int partSize = 20;
			for (int from = 0, to = (partSize - 1); from < symLinkItems.Count; from += partSize, to += partSize) {
				InsertSymLinkItemsPart(writer, volumeID, symLinkItems, from, to);				 
				CheckForCancellationRequest();			  
			}
		}
		
		private void InsertSymLinkItemsPart(BufferedVolumeItemWriter writer, long volumeID, List<SymLinkItem> symLinkItems, int from, int to) {
			// resolve symlink targets
			// build a query like e.g. '(VolumeID = 223) AND ((Location="/dir1" AND Name="file1") OR ((Location="/dir2" AND Name="file2") ...)'
			SearchCriteriaGroup g = new SearchCriteriaGroup(MatchRule.AllMustMatch);
			g.AddSearchCriteria(new IDSearchCriteria(volumeID, IDSearchField.VolumeID, CompareOperator.Equal));
			
			SearchCriteriaGroup g2 = new SearchCriteriaGroup(MatchRule.AnyMustMatch);
			g.AddSearchCriteria(g2);
			
			//foreach(SymLinkItem sli in symLinkItems) {
			for(int i = from; (i <= to) && (i < symLinkItems.Count); i++) {
				SymLinkItem sli = symLinkItems[i];

				SearchCriteriaGroup locationNameGroup = new SearchCriteriaGroup(MatchRule.AllMustMatch);
				FreeTextSearchCriteria ftsc;
				
				if (sli.targetLocation.Length > 0) { // symlinks pointing to the root do not have a location (only a name, see the SymLinkItem class definition)
					ftsc = new FreeTextSearchCriteria(sli.targetLocation, FreeTextSearchField.Location, TextCompareOperator.IsEqual);
					locationNameGroup.AddSearchCriteria(ftsc);
				}
				
				ftsc = new FreeTextSearchCriteria(sli.targetName, sli.isDir ? FreeTextSearchField.DirectoryName : FreeTextSearchField.FileName, TextCompareOperator.IsEqual);
				locationNameGroup.AddSearchCriteria(ftsc);
				
				g2.AddSearchCriteria(locationNameGroup);
			}
			
			// query target items
			VolumeItem[] queriedItems = Database.SearchItem(g); // async BeginItemSearch() won't work here (active transaction prevents other threads from accessing the database)

			// store queried target items in a dictionary for faster access
			Dictionary<string, FileSystemVolumeItem> targetItems = new Dictionary<string, FileSystemVolumeItem>();
			foreach(FileSystemVolumeItem item in queriedItems)
				targetItems.Add(item.Location + item.Name, item);

			//foreach(SymLinkItem sli in symLinkItems) {
			for(int i = from; (i <= to) && (i < symLinkItems.Count); i++) {
				SymLinkItem sli = symLinkItems[i];
				FileSystemVolumeItem targetItem;
				
				if (!targetItems.TryGetValue(sli.targetLocation + sli.targetName, out targetItem)) {
					/* may throw ScanCancelledException */
					SendScannerWarning(string.Format(S._("Failed to resolve target item for symlink '{0}'."), sli.fullSourceName));
				} else {
					
					FileSystemVolumeItem newItem;
					
					if (targetItem is FileVolumeItem) {
						
						newItem = GetNewVolumeItem<FileVolumeItem>(sli.parentID, sli.sourceName, targetItem.MimeType, targetItem.MetaData, VolumeItemType.FileVolumeItem);
						((FileVolumeItem)newItem).SetFileVolumeItemFields( ((FileVolumeItem)targetItem).Size, ((FileVolumeItem)targetItem).Hash);
						
						Interlocked.Increment(ref VolumeInfo.files);
						
					} else { // DirectoryVolumeItem
						
						newItem = GetNewVolumeItem<DirectoryVolumeItem>(sli.parentID, sli.sourceName, targetItem.MimeType, targetItem.MetaData, VolumeItemType.DirectoryVolumeItem);
						
						Interlocked.Increment(ref VolumeInfo.directories);
					}
					
					newItem.SetFileSystemVolumeItemFields(sli.sourceLocation, targetItem.LastWriteTime, targetItem.ItemID);
					writer.Write(newItem);
					
					// TODO : 
					// increase totalsize by size of symlinks too? (not size of target!)
					// or are symlinks as big as dirs, those aren't respected as well.. 
					//Interlocked.Add(ref VolumeInfo.size, sli.size);
					
					Platform.Common.Diagnostics.Debug.WriteLine("Successfully resolved and saved symlink item: {0}/{1} -> {2}/{3}", (sli.sourceLocation == PATH_SEPARATOR.ToString() ? "" : sli.sourceLocation), sli.sourceName, (targetItem.Location == PATH_SEPARATOR.ToString() ? "" : targetItem.Location), (targetItem.Name == PATH_SEPARATOR.ToString() ? "" : targetItem.Name));
				}
			}
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
				SendScannerWarning(string.Format(S._("Can't read LastWriteTime from item '{0}' ({1})."), f.FullName, e.Message));
			}
			return lastWriteTime;
		}
		
		private void CheckForCancellationRequest() {
			if (CancellationRequested)
				throw new ScanCancelledException();
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
		
		private static string GetFullSymLinkTargetPath(string symLinkTarget, string currentDir) {
			string fullTargetPath;
			if (Path.IsPathRooted(symLinkTarget))
				fullTargetPath = symLinkTarget;
			else
				fullTargetPath = Path.GetFullPath(Path.Combine(currentDir, symLinkTarget));  //GetFullPath() removes relative dots, eg "/dir1/dir2/../file1" becomes "/dir1/file1"
		   
			// remove possible ending slash from directory targets
			fullTargetPath = RemoveEndingSlash(fullTargetPath);
//			 if ((fullTargetPath[fullTargetPath.Length - 1] == Path.DirectorySeparatorChar) && (fullTargetPath.Length > 1))
//				  fullTargetPath = fullTargetPath.Substring(0, fullTargetPath.Length - 1);
		   
		   return fullTargetPath;
		}
		
		private class SymLinkItem
		{
			private SymLinkItem(long parentID, bool isDir, string fullSourceName, string sourceName, string sourceLocation, string targetName, string targetLocation) {
				this.parentID		= parentID;
				this.isDir			= isDir;
				this.fullSourceName = fullSourceName;
				this.sourceName		= sourceName;
				this.sourceLocation = sourceLocation;
				this.targetName		= targetName;
				this.targetLocation = targetLocation;
			}
			
			public static SymLinkItem CreateInstance(FileSystemInfo source, string target, long parentID, bool isDir, string rootPath, FilesystemVolumeScanner scanner) {
				string targetLocation, targetName;

				if (target == rootPath) { // if target is rootPath Path.GetFileName(target) won't work
					targetLocation	= string.Empty;
					targetName		= PATH_SEPARATOR.ToString();
				} else {
					targetLocation	= scanner.GetLocation(target, rootPath);
					targetName		= Path.GetFileName(target); // Path.GetFileName() also works with directory targets
				}
				return new SymLinkItem(parentID, isDir, source.FullName, source.Name, scanner.GetLocation(source.FullName, rootPath), targetName, targetLocation);
			}
		
			public long		parentID;			 
			public bool		isDir;
			public string	fullSourceName;
			public string	sourceName;
			public string	sourceLocation;			   
			public string	targetName;
			public string	targetLocation;
		}
		
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
