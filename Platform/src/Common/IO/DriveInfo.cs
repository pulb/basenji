// DriveInfo.cs
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
using System.Collections.Generic;
using Platform.Common.Diagnostics;

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
		private string volumeLabel;
		private long totalSize;
		private string rootPath; // on linux -> mountpoint, on windows e.g. "D:\"
		private string device; // on linux -> blockdevice, on windows e.g. "D:"
		private DriveType driveType;
		private string filesystem;
		private bool isMounted;
		private bool isReady;
 
#if !WIN32		
		static DriveInfo() {
			// required by gtk apps to prevent multithreading issues
			DkDisk.InitBusG();
		}
#endif
		
		private DriveInfo() {
			this.volumeLabel = string.Empty;
			this.totalSize = 0L;
			this.rootPath = string.Empty;
			this.device = string.Empty;
			this.driveType = DriveType.Unknown;
			this.filesystem = string.Empty;
			this.isMounted = false;
			this.isReady = false;
		}
		
		public DriveInfo(string rootPath) : this() {
	
			if (rootPath == null)
				throw new ArgumentNullException("rootPath");
			
#if WIN32
			if (!rootPath.EndsWith("\\")) // e.g. "D:" -> "D:\"
				rootPath += "\\";

			// throws ArgumentException if drive can't be found
			System.IO.DriveInfo di = new System.IO.DriveInfo(rootPath);
			FillDriveInfo(this, di);
#else

			// remove endling slash from path
			if ((rootPath.Length > 1) && (rootPath[rootPath.Length - 1] == System.IO.Path.DirectorySeparatorChar))
				rootPath = rootPath.Substring(0, rootPath.Length - 1);
			
			DkDisk volume = null;
			DkDisk[] devs = DkDisk.EnumerateDevices();
			foreach (DkDisk dev in devs) {
				if (dev.IsMounted && dev.MountPoint == rootPath) {
					volume = dev;
					break;
				}
			}
			
			if (volume == null)
				throw new ArgumentException("Can't find drive for specified path", "rootPath");
	
			FillDriveInfo(this, volume);
#endif
		}
	
		public static DriveInfo FromDevice(string device) {
			if (device == null)
				throw new ArgumentNullException("device");
			
#if WIN32
			string rootPath = device; // ctor adds an ending slash ("d:\")
			return new DriveInfo(rootPath); // throws ArgumentException if drive cant be found
#else
			// dev can be a drive (e.g. cdrom with/without media), 
			// a partitiontable or a partition.
			DkDisk dev = DkDisk.FindByDevice(device);
			
			if (dev == null)
				throw new ArgumentException("Can't find drive for specified device", "device");
			
			if (dev.IsPartitionTable)
				throw new ArgumentException("Device is a harddisk drive and may have one ore more volumes (partitions) with different devices names. Please specify the devicename of one of its volumes instead", "device");
			
			DriveInfo d = new DriveInfo();			
			FillDriveInfo(d, dev);
			
			return d;
#endif
		}
		
		public static DriveInfo[] GetDrives() { return GetDrives(false); }
		public static DriveInfo[] GetDrives(bool readyDrivesOnly) {
		
			List<DriveInfo> drives = new List<DriveInfo>();
			
#if WIN32

			System.IO.DriveInfo[] ioDrives = System.IO.DriveInfo.GetDrives();
			foreach(System.IO.DriveInfo di in ioDrives) {
				if (!(readyDrivesOnly && !di.IsReady)) {
					DriveInfo d = new DriveInfo();
					FillDriveInfo(d, di);
					drives.Add(d);
				}
			}
#else
			// dev can be a drive (e.g. cdrom with/without media), 
			// a partitiontable or a partition.
			DkDisk[] devs = DkDisk.EnumerateDevices();
			
			foreach (DkDisk dev in devs) {
				// skip empty drives when readyDrivesOnly is set to true.
				// (ready means media present but not necessarily mounted, e.g. audio cds)
				if (readyDrivesOnly && !dev.IsMediaAvailable)
					continue;
				
				// skip partitiontables, e.g. sda, sdb (usb-stick).
				// (partitiontables are drives)
				if (dev.IsPartitionTable)
					continue;
				
				// skip unmounted partitions (e.g. swap) and 
				// boot and home partitions.
				if (dev.IsPartition && 
				    (!dev.IsMounted || (dev.MountPoint == "/boot") || (dev.MountPoint == "/home")))
					continue;
				
				DriveInfo d = new DriveInfo();
				FillDriveInfo(d, dev);
				
				drives.Add(d);
			}
#endif
			return drives.ToArray();
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
	

#if WIN32

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
			
			switch(di.DriveType) {
				case System.IO.DriveType.CDRom:
					d.driveType = DriveType.CDRom;
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
					break;
				case System.IO.DriveType.Unknown:
					d.driveType = DriveType.Unknown;
					break;
			}
		}
#else
		private static void FillDriveInfo(DriveInfo d, DkDisk dev) {
			Debug.Assert(!dev.IsPartitionTable, 
			             "dev must not be a partitiontable");
			
			if (dev.IsMounted) {
				d.volumeLabel = dev.Label;
				d.totalSize = (long)dev.Size;
				d.rootPath = dev.MountPoint;
				d.filesystem = dev.IdType;
				
				d.isMounted = true;
				d.isReady = true;
			} else if (dev.IsMediaAvailable) {
				// unmounted media or partition
				d.volumeLabel = dev.Label;
				d.totalSize = (long)dev.Size;
				d.filesystem = dev.IdType;				
				
				d.isReady = true;
			} // else: empty drive
			
			if (dev.IsPartition) {
				string obj_path = dev.PartitionSlave;
				DkDisk parent = new DkDisk(obj_path);
				d.driveType = GetDriveType(parent);
			} else {
				d.driveType = GetDriveType(dev);
			}
			
			d.device = dev.DeviceFile;
		}
		
		private static DriveType GetDriveType(DkDisk drive) {
			Debug.Assert(drive.IsDrive, "DkDisk is not a drive");
			
			// TODO : add support for ram and network volumes
			DriveType dt = DriveType.Unknown;			
			string[] compat = drive.MediaCompatibility;
			
			bool isOptical = false;
			foreach (string c in compat) {
				if (c.StartsWith("optical")) {
					isOptical = true;
					break;
				}
			}
			
			if (isOptical) 
				dt = DriveType.CDRom;
			else if (drive.IsRemovable)
				dt = DriveType.Removable;
			else
				dt = DriveType.Fixed;
			
			return dt;
		}
#endif

	}
}