// UnixFileHelper.cs
// 
// Copyright (C) 2008, 2010 Patrick Ulbrich
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

/*
 * This file contains code
 * from Mono SVN 
 * /trunk/mcs/class/Mono.Posix/Mono.Unix/UnixFileSystemInfo.cs (rev 59752)
 * /trunk/mcs/class/Mono.Posix/Mono.Unix/FileTypes.cs (rev 55255)
 * 
 * TODO : place appropriate copyright notice here
 */
#if UNIX
using System;
using System.Text;
using Mono.Unix;
using Mono.Unix.Native;

namespace Platform.Unix.IO
{
	/* from Mono SVN (/trunk/mcs/class/Mono.Posix/Mono.Unix/FileTypes.cs) */
	public enum UnixFileType
	{
		Directory		  = (int) FilePermissions.S_IFDIR,
		CharacterDevice   = (int) FilePermissions.S_IFCHR,
		BlockDevice		  = (int) FilePermissions.S_IFBLK,
		RegularFile		  = (int) FilePermissions.S_IFREG,
		Fifo			  = (int) FilePermissions.S_IFIFO,
		SymbolicLink	  = (int) FilePermissions.S_IFLNK,
		Socket			  = (int) FilePermissions.S_IFSOCK,
	}

	public static class UnixFileHelper
	{
	
		public static Stat GetStat(string path) {
			Stat stat;
			int r = Syscall.stat(path, out stat);
			UnixMarshal.ThrowExceptionForLastErrorIf(r);
			return stat;
		}
		
		public static Stat GetLStat(string path) {
			Stat stat;
			int r = Syscall.lstat(path, out stat);
			UnixMarshal.ThrowExceptionForLastErrorIf(r);
			return stat;
		}
		
		public static UnixFileType GetFileType(string path) {
			Stat stat = GetStat(path);
			return GetFileType(stat);
		}
		
		public static UnixFileType GetFileType(Stat stat) {
			return (UnixFileType)(stat.st_mode & FilePermissions.S_IFMT);
		}
		
		public static long GetFileSize(string path) {
			Stat stat = GetStat(path);
			return GetFileSize(stat);
		}
		
		public static long GetFileSize(Stat stat) {
			return stat.st_size;
		}
		
		public static DateTime GetLastAccessTime(string path) {
			Stat stat = GetStat(path);
			return GetLastAccessTime(stat);
		}
		
		public static DateTime GetLastAccessTime(Stat stat) {
			return NativeConvert.ToDateTime(stat.st_atime);
		}
		
		public static DateTime GetLastStatusChangeTime(string path) {
			Stat stat = GetStat(path);
			return GetLastStatusChangeTime(stat);
		}
		
		public static DateTime GetLastStatusChangeTime(Stat stat) {
			return NativeConvert.ToDateTime(stat.st_ctime);
		}
		
		public static DateTime GetLastWriteTime(string path) {
			Stat stat = GetStat(path);
			return GetLastWriteTime(stat);
		}
		
		public static DateTime GetLastWriteTime(Stat stat) {
			return NativeConvert.ToDateTime(stat.st_mtime);
		}
		
		public static string ReadLink(string symLinkPath, 
		                              /* see man readlink */
		                              bool canonicalize_existing) {
			
			if (!canonicalize_existing)
				return ReadLink(symLinkPath);
			
			// throws FileNotFoundException if a path component does not exsist,
			// including the last one.
			string path = UnixPath.GetCompleteRealPath(symLinkPath);
			string tmp;
			
			while (path != (tmp = UnixPath.GetCompleteRealPath(path)))
				path = tmp;
			
			return path;
		}
		
		public static string ReadLink(string symLinkPath) {
			StringBuilder buffer = new StringBuilder(256);
		   /* if charcount returned by readlink() equals the buffersize
			* assume the path was clipped and increase the buffersize.
			*/
			while(ReadLink(symLinkPath, buffer) == buffer.Capacity)
				buffer.EnsureCapacity(buffer.Capacity * 2);

			return buffer.ToString();
		}
		
		private static int ReadLink(string symLinkPath, StringBuilder buffer) {
			int r;
			r = Syscall.readlink(symLinkPath, buffer);
			UnixMarshal.ThrowExceptionForLastErrorIf(r);
			return r;
		}
	}
}
#endif