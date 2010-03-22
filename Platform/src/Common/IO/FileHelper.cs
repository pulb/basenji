// FileHelper.cs
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

using System;
#if UNIX
using Platform.Unix.IO;
#endif
namespace Platform.Common.IO
{
	// required to be in sync with Platform.Unix.IO.UnixFileType
	public enum FileType
	{
		Directory,
		CharacterDevice,
		BlockDevice,
		RegularFile,
		Fifo,
		SymbolicLink,
		Socket
	}
	
	public static class FileHelper
	{
		public static FileType GetFileType(string path, bool followSymLinks) {
#if WIN32
			// TODO : test me, is it working?
			System.IO.FileAttributes attr = System.IO.File.GetAttributes(path);
			return	(attr & System.IO.FileAttributes.Directory) != 0 ? FileType.Directory : FileType.RegularFile; 
#else
			Mono.Unix.Native.Stat stat;
			if (followSymLinks)
				stat = UnixFileHelper.GetStat(path);
			else
				stat = UnixFileHelper.GetLStat(path);
				
			FileType ft;
			UnixFileType uft = UnixFileHelper.GetFileType(stat);
			switch(uft) {
				case UnixFileType.Directory:
					ft = FileType.Directory;
					break;
				case UnixFileType.CharacterDevice:
					ft = FileType.CharacterDevice;
					break;
				case UnixFileType.BlockDevice:
					ft = FileType.BlockDevice;
					break;
				case UnixFileType.RegularFile:
					ft = FileType.RegularFile;
					break;
				case UnixFileType.Fifo:
					ft = FileType.Fifo;
					break;
				case UnixFileType.SymbolicLink:
					ft = FileType.SymbolicLink;
					break;
				case UnixFileType.Socket:
					ft = FileType.Socket;
					break;
				default:
					throw new NotImplementedException(string.Format("UnixFileType {0} has no equivalent FileType value yet", uft.ToString()));
			}
			return ft;
#endif		
		}
		
		public static string GetCanonicalSymLinkTarget(string symLinkPath) {
#if WIN32
			throw new NotImplementedException();
#else
			return UnixFileHelper.ReadLink(symLinkPath, true);
#endif		
		}
	}
}
