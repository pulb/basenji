// GioDriveInfoProvider.cs
// 
// Copyright (C) 2011, 2016 Patrick Ulbrich
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
#if UNIX
using System;
using System.Collections.Generic;
using GLib;
using Platform.Common.IO;

namespace Platform.Unix.IO
{
	internal class GioDriveInfoProvider : IDriveInfoProvider
	{	
		private const string G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE = "unix-device";

		public void FromDevice(DriveInfo d, string device)	{
			VolumeMonitor volmon = GLib.VolumeMonitor.Default;

			// Search in mounts first
			foreach (Mount m in volmon.Mounts) {
				if ((m.Volume != null) && (m.Volume.GetIdentifier(G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE) == device)) {
					FillDriveInfoFromMount(d, m, device);
					return;
				}
			}

			// Search in volumes (unmounted or else the device would have been found in volmon.Mounts)
			foreach (Volume v in volmon.Volumes) {
				if (v.GetIdentifier(G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE) == device) {
					d.device = device;
					d.driveType = GuessDriveType(null, v.Icon, v.Drive);
					return;
				}
			}

			// Search in drives (does not contain media or else the device 
			// would have been found in volmon.Mounts/Volumes) (e.g. cdrom-drives))
			foreach (Drive dr in volmon.ConnectedDrives) {
				if (dr.GetIdentifier(G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE) == device) {
					d.device = device;
					d.driveType = GuessDriveType(null, dr.Icon, dr);
					return;
				}
			}

			throw new ArgumentException("Can't find drive for specified device", "device");
		}
		
		public void FromPath(DriveInfo d, string rootPath)	{
			VolumeMonitor volmon = GLib.VolumeMonitor.Default;

			// Remove endling slash from path
			if ((rootPath.Length > 1) && (rootPath[rootPath.Length - 1] == System.IO.Path.DirectorySeparatorChar))
				rootPath = rootPath.Substring(0, rootPath.Length - 1);

			foreach (Mount m in volmon.Mounts) {
				if (m.Root.Path == rootPath) {
					FillDriveInfoFromMount (d, m, 
						(m.Volume != null) ? m.Volume.GetIdentifier (G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE) : string.Empty);
					return;
				}
			}

			throw new ArgumentException("Can't find drive for specified path", "rootPath");
		}
		
		public List<DriveInfo> GetAll(bool readyDrivesOnly) {
			List<DriveInfo> drives = new List<DriveInfo> ();
			VolumeMonitor volmon = GLib.VolumeMonitor.Default;

			foreach (Mount m in volmon.Mounts) {
				DriveInfo d = new DriveInfo();

				FillDriveInfoFromMount(d, m, 
					(m.Volume != null) ? m.Volume.GetIdentifier (G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE) : string.Empty);

				drives.Add(d);
			}
				
			if (!readyDrivesOnly) {
				foreach (Volume v in volmon.Volumes) {
					if (drives.FindIndex(di => (v.GetIdentifier(G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE) == di.Device)) == -1) {
						// Volume is unmounted or else it would have been referenced via volmon.Mounts
						DriveInfo d = new DriveInfo();

						d.device = v.GetIdentifier(G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE);
						d.driveType = GuessDriveType(null, v.Icon, v.Drive);

						drives.Add(d);
					}
				}

				foreach (Drive dr in volmon.ConnectedDrives) {
					if (dr.IsMediaRemovable && !dr.HasMedia) {
						DriveInfo d = new DriveInfo();

						d.device = dr.GetIdentifier(G_VOLUME_IDENTIFIER_KIND_UNIX_DEVICE);
						d.driveType = GuessDriveType(null, dr.Icon, dr);

						drives.Add(d);
					}
				}
			}

			return drives;
		}
		
		private static void FillDriveInfoFromMount(DriveInfo d, GLib.Mount m, string device) {
			if (m.Volume != null) {
				// Only get size and format info from physical volumes
				// (System.IO.DriveInfo throws an exception on network mounts)
				System.IO.DriveInfo di = new System.IO.DriveInfo (m.Root.Path);
				d.totalSize = di.TotalSize;
				d.filesystem = di.DriveFormat;
			}

			d.volumeLabel = m.Name;
			d.rootPath = m.Root.Path;
			d.device = device;
			d.driveType = GuessDriveType(m.Root.UriScheme, m.Icon, m.Drive);
			d.isMounted = true;
			d.isReady = true;
			d.hasAudioCdVolume = (m.Root.UriScheme == "cdda");
		}

		private static Platform.Common.IO.DriveType GuessDriveType(string uriScheme, GLib.Icon icon, Drive dr) {
			switch (uriScheme) {
				case "ssh":
				case "smb":
				case "ftp":
					return DriveType.Network;
				case "cdda":
					return DriveType.CDRom;
			}

			ThemedIcon themedIcon = icon as ThemedIcon;
			
			if ((themedIcon != null) && (Array.FindIndex(themedIcon.Names, i => i.Contains("optical")) != -1))
				return DriveType.CDRom;

			if (dr != null)
				return dr.IsMediaRemovable ? DriveType.Removable : DriveType.Fixed;
			
			return DriveType.Unknown;
		}


	}
}
#endif // UNIX
