// PathUtil.cs
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

using System;
using System.IO;
using VolumeDB;

namespace Basenji
{
//	// paths to external db data like logs, thumbnails
	public static class PathUtil
	{
		public static string GetDbDataPath(VolumeDatabase db) {
			string settingsPath = App.Settings.GetSettingsDirectory().FullName;			   
			string guid = db.GetDBProperties().Guid;
			string dataPath = Path.Combine(Path.Combine(settingsPath, "dbdata"), guid);
			
			if (!Directory.Exists(dataPath))
				Directory.CreateDirectory(dataPath);
				
			return dataPath;
		}
/*
		public static string GetVolumeDataPath(VolumeDatabase db, long volumeID) {
			string volDataPath = Path.Combine(GetDbDataPath(db), volumeID.ToString());
			
//			if (!Directory.Exists(volDataPath))
//				Directory.CreateDirectory(volDataPath);
			
			return volDataPath;
		}
		
		public static string GetVolumeDataThumbsPath(VolumeDatabase db, long volumeID) {
			string thumbsPath = Path.Combine(GetVolumeDataPath(db, volumeID), "thumbs");
			
//			if (!Directory.Exists(thumbsPath))
//				Directory.CreateDirectory(thumbsPath);
			
			return thumbsPath;
}*/
	}
}
