// FilesystemVolumeInfo.cs
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
using System.Threading;

namespace VolumeDB.VolumeScanner
{
	/* 
	 * Type returned by the VolumeInfo property of the FilesystemVolumeScanner class.
	 * It provides basic readonly info about the volume being scanned.
	 * Properties have to be threadsafe (client may read while scanner writes)).
	 */
	public class FilesystemVolumeInfo : VolumeInfo
	{
		// FilesystemVolumeScanner does write to these properties directly (e.g. via Interlocked.Increment())
		internal long files;
		internal long directories;
		internal long size;
		
		internal FilesystemVolumeInfo(FileSystemVolume v) : base(v) {
			this.files			= v.Files;
			this.directories	= v.Directories;
			this.size			= v.Size;
		}
		
		internal override void Reset () {
			Interlocked.Exchange(ref files, 0);
			Interlocked.Exchange(ref directories, -1); // -1 : subtract root dir
			Interlocked.Exchange(ref size, 0);
		}

		public long Files		  { get { return Interlocked.Read(ref files);		  } }
		public long Directories   { get { return Interlocked.Read(ref directories);   } }
		public long Size		  { get { return Interlocked.Read(ref size);		  } }
	}
}
