// DriveInfo.cs
// 
// Copyright (C) 2008 - 2016 Patrick Ulbrich
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

namespace Platform.Common.IO
{
	/*
	 *	DriveInfo class
	 *	 
	 *	The purpose of this class is to provide similar drive information on all platforms.
	 *	It should behave similar to System.IO.DriveInfo on Windows.
	 *	E.g. on GNOME, is should return all drives that are listed in Nautilus:Computer.
	 *
	 *	On linux it requires Devicekit.Disks to return proper information for propterties 
	 *	like Device, DriveType and VolumeLabel.
	 *	Alternatively, the implementation could be based on Gnome.Vfs.VolumeMonitor, 
	 *	but this would make it GNOME-dependent 
	 *	(BAD -> class is used in platform-independed VolumeDB assembly).
	 */
	public class DriveInfo
	{
		internal string volumeLabel;
		internal long totalSize;
		internal string rootPath; // on linux -> mountpoint, on windows e.g. "D:\"
		internal string device; // on linux -> blockdevice, on windows e.g. "D:"
		internal DriveType driveType;
		internal string filesystem;
		internal bool isMounted;
		internal bool isReady;
		internal bool hasAudioCdVolume;
 		
		private static readonly IDriveInfoProvider dip;
		
		static DriveInfo()
		{
#if WIN32
			dip = new Platform.Win32.IO.Win32DriveInfoProvider();
#elif UNIX
			dip = new Platform.Unix.IO.GioDriveInfoProvider();
#else
			throw new NotImplementedException();
#endif
		}
		
		internal DriveInfo() {
			this.volumeLabel = string.Empty;
			this.totalSize = 0L;
			this.rootPath = string.Empty;
			this.device = string.Empty;
			this.driveType = DriveType.Unknown;
			this.filesystem = string.Empty;
			this.isMounted = false;
			this.isReady = false;
			this.hasAudioCdVolume = false;
		}
		
		public DriveInfo(string rootPath) : this() {
	
			if (rootPath == null)
				throw new ArgumentNullException("rootPath");
			
			dip.FromPath(this, rootPath);
		}
	
		public static DriveInfo FromDevice(string device) {
			if (device == null)
				throw new ArgumentNullException("device");
			
			DriveInfo d = new DriveInfo();
			dip.FromDevice(d, device);
			return d;
		}
		
		public static DriveInfo[] GetDrives() { return GetDrives(false); }
		public static DriveInfo[] GetDrives(bool readyDrivesOnly) {
			return dip.GetAll(readyDrivesOnly).ToArray();
		}
	
		public string VolumeLabel {
			get { 
				if (!isReady)
					throw new DriveNotReadyException();
				return volumeLabel;
			}
		}
	
		public long TotalSize {
			get {
				if (!isReady)
					throw new DriveNotReadyException();
				return totalSize;
			}
		}
		
		public string Device {
			get { return device; }
		}
		
		public string RootPath {
			get {
				if (!isMounted)
					throw new DriveNotMountedException();
				return rootPath;
			}
		}
	
		public DriveType DriveType {
			get { return driveType; }
		}
	
		public string FileSystem {
			get {
				if (!isReady)
					throw new DriveNotReadyException();
				return filesystem;
			}
		}
		
		/* E.g. audio cds will not be mounted */
		public bool IsMounted {
			get { return isMounted; }
		}
		
		/*
		 * If drive isn't ready, only volume independent properties may be accessed.
		 * (i.e. only properties related to the drive itself)
		*/
		public bool IsReady {
			get { return isReady; }
		}
		
		public bool HasAudioCdVolume {
			get { return hasAudioCdVolume; }
		}
	}
}