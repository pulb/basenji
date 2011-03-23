// GioDriveInfoProvider.cs
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

#if GNOME

using System;
using System.Collections.Generic;
using GLib;
using Platform.Common.IO;
using Platform.Unix.IO;

namespace Platform.Gnome.IO
{
	internal class GioDriveInfoProvider : DkDriveInfoProvider
	{	
		private static readonly Dictionary<string, string> supportedSchemes 
			= new Dictionary<string, string>()
		{
			{ "file",		"FILE" },
			{ "smb",		"SMB" },
			{ "ssh",		"SSH" },
			{ "archive",	"ARCHIVE" }
		};
		
		public override void FromDevice(DriveInfo d, string device)	{
			base.FromDevice(d, device);
		}
		
		public override void FromPath(DriveInfo d, string rootPath)	{
			try	{
				base.FromPath(d, rootPath);
			} catch (ArgumentException) {
				// rootpath was not found in devicekit disks
				// try to find a GIO mountpoint with that path
				var mounts = GLib.VolumeMonitor.Default.Mounts;
				foreach (var m in mounts) {
					if (supportedSchemes.ContainsKey(m.Root.UriScheme) && 
					  	(m.Root.Path == rootPath))
					{
						FillDriveInfo(d, m);
						return;
					}
				}
				
				throw new ArgumentException("Can't find drive for specified path", "rootPath");
			}
		}
		
		public override List<DriveInfo> GetAll(bool readyDrivesOnly) {
			List<DriveInfo> drives = base.GetAll(readyDrivesOnly);
			
			// base.GetAll() has returned all mounted (and unmounted) fixed and removable volumes.
			// the remaining mounts are expected to be network shares, virtual volumes
			// (mounted iso images, dirs mounted with bindfs) etc.
			var mounts = GLib.VolumeMonitor.Default.Mounts;
			
			foreach (var m in mounts) {
				if (supportedSchemes.ContainsKey(m.Root.UriScheme) && 
				    (drives.FindIndex(e => IsEqual(e, m)) == -1))
				{
					DriveInfo d = new DriveInfo();
					FillDriveInfo(d, m);
					drives.Add(d);
				}
			}
			
			return drives;
		}
		
		private static void FillDriveInfo(DriveInfo d, GLib.Mount m) {
			d.volumeLabel = m.Name;
			d.totalSize = 0L;
			d.rootPath = m.Root.Path;
			d.device = null;
			d.driveType = Platform.Common.IO.DriveType.Unknown;
			d.filesystem = null;			
			d.isMounted = true;
			d.isReady = true;
			d.hasAudioCdVolume = false;
		}
		
		private static bool IsEqual(DriveInfo d, GLib.Mount m) {
			return d.IsMounted && (d.RootPath == m.Root.Path);
		}
	}
}
#endif