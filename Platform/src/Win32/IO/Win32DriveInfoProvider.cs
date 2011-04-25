// Win32DriveInfoProvider.cs
// 
// Copyright (C) 2011 Patrick Ulbrich
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

#if WIN32

using System;
using System.Collections.Generic;
using Platform.Common.IO;

namespace Platform.Win32.IO
{
	internal class Win32DriveInfoProvider : IDriveInfoProvider
	{		
		public virtual void FromDevice(DriveInfo d, string device) {
			string rootPath = device; // FromPath() adds an ending slash ("d:\")
			FromPath(d, rootPath); // throws ArgumentException if drive cant be found
		}
		
		public virtual void FromPath(DriveInfo d, string rootPath) {
			if (!rootPath.EndsWith("\\")) // e.g. "D:" -> "D:\"
				rootPath += "\\";

			// throws ArgumentException if drive can't be found
			System.IO.DriveInfo di = new System.IO.DriveInfo(rootPath);
			
			FillDriveInfo(d, di);
		}
		
		public virtual List<DriveInfo> GetAll(bool readyDrivesOnly) {
			List<DriveInfo> drives = new List<DriveInfo>();
			
			System.IO.DriveInfo[] ioDrives = System.IO.DriveInfo.GetDrives();
			foreach (System.IO.DriveInfo di in ioDrives) {
				if (!(readyDrivesOnly && !di.IsReady)) {
					DriveInfo d = new DriveInfo();
					FillDriveInfo(d, di);
					drives.Add(d);
				}
			}

			return drives;
		}
		
		private static void FillDriveInfo(DriveInfo d, System.IO.DriveInfo di) {
			if (di.IsReady) {
				d.volumeLabel = di.VolumeLabel;
				d.totalSize = di.TotalSize;
				d.filesystem = di.DriveFormat;
			}
			
			d.rootPath = di.RootDirectory.FullName;
			// should return e.g. "D:", not "D:\"
			d.device = d.rootPath[d.rootPath.Length - 1] == System.IO.Path.DirectorySeparatorChar ? d.rootPath.Substring(0, d.rootPath.Length - 1) : d.rootPath;
			d.isMounted = true;
			d.isReady = di.IsReady;
			
			switch (di.DriveType) {
				case System.IO.DriveType.CDRom:
					d.driveType = DriveType.CDRom;
					if (d.isReady)
						d.hasAudioCdVolume = AudioCdWin32.IsAudioCd(d.device);
					break;
				case System.IO.DriveType.Fixed:
					d.driveType = DriveType.Fixed;
					break;
				case System.IO.DriveType.Network:
					d.driveType = DriveType.Network;
					break;
				case System.IO.DriveType.Ram:
					d.driveType = DriveType.Ram;
					break;
				case System.IO.DriveType.Removable:
					d.driveType = DriveType.Removable;
					break;
				case System.IO.DriveType.NoRootDirectory:
					d.driveType = DriveType.Unknown;
					d.isMounted = false;
					break;
				case System.IO.DriveType.Unknown:
					d.driveType = DriveType.Unknown;
					break;
			}
		}
	}
}
#endif