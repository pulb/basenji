// DbData.cs
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
using System.IO;

namespace VolumeDB
{
	public static class DbData
	{
		public static string GetVolumeDataPath(string dbDataPath, long volumeID) {
			return Path.Combine(dbDataPath, volumeID.ToString());
		}
		
		public static string CreateVolumeDataPath(string dbDataPath, long volumeID) {
			string path = GetVolumeDataPath(dbDataPath, volumeID);
			
			// make sure there is no directory with the same name as the volume directory 
			// that is about to be created
			// (the volume directory may be deleted in a catch block on scanning/import failure, 
			// so make sure that no existing dir will be deleted)
			if (Directory.Exists(path))
				throw new ArgumentException("dbDataPath already contains a directory for this volume");
	
			Directory.CreateDirectory(path);
			
			return path;
		}
		
		public static string GetVolumeDataThumbsPath(string volumeDataPath) {
			return Path.Combine(volumeDataPath, "thumbs");
		}
		
		public static string CreateVolumeDataThumbsPath(string volumeDataPath) {
			string path = GetVolumeDataThumbsPath(volumeDataPath);
			
			if (Directory.Exists(path))
				throw new ArgumentException("volumeDataPath already contains a thumbs directory");
	
			Directory.CreateDirectory(path);
			
			return path;
		}
	}
}
