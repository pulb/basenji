// DkDriveInfoProvider.cs
// 
// Copyright (C) 2011, 2012 Patrick Ulbrich
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

#if !WIN32

using System;
using System.Collections.Generic;
using Platform.Common.IO;
using Platform.Common.Diagnostics;

namespace Platform.Unix.IO
{
	internal class DkDriveInfoProvider : IDriveInfoProvider
	{
		static DkDriveInfoProvider() {
			// required by gtk apps to prevent multithreading issues
			DkDisk.InitBusG();
		}
		
		public virtual void FromDevice(DriveInfo d, string device) {
			// dev can be a drive (e.g. cdrom with/without media, an usb-stick with a custom format (no partitions), ...), 
			// a partitiontable, a partition or a luks-holder representing an encrypted filesystem.
			DkDisk dev = DkDisk.FindByDevice(device);
			
			if (dev == null)
				throw new ArgumentException("Can't find drive for specified device", "device");
			
			if (dev.IsPartitionTable)
				throw new ArgumentException("Device is a harddisk drive and may have one ore more volumes (partitions) with different devices names. Please specify the devicename of one of its volumes instead", "device");
			
			FillDriveInfo(d, dev);
		}
		
		public virtual void FromPath(DriveInfo d, string rootPath) {
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
	
			FillDriveInfo(d, volume);
		}
		
		public virtual List<DriveInfo> GetAll(bool readyDrivesOnly)	{
			List<DriveInfo> drives = new List<DriveInfo>();
			
			// dev can be a drive (e.g. cdrom with/without media, an usb-stick with a custom format (no partitions), ...), 
			// a partitiontable, a partition or a luks-holder representing an encrypted filesystem.
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
				if ((dev.IsPartition || dev.DeviceIsLuksClearText) && 
				    (!dev.IsMounted || (dev.MountPoint == "/boot") || (dev.MountPoint == "/home")))
					continue;
				
				DriveInfo d = new DriveInfo();
				FillDriveInfo(d, dev);
				
				drives.Add(d);
			}

			return drives;
		}
		
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
				// unmounted media, partition or luks-holder
				d.volumeLabel = dev.Label;
				d.totalSize = (long)dev.Size;
				d.filesystem = dev.IdType;				
				
				d.isReady = true;
			} // else: empty drive
			
			if (dev.IsPartition) {
				string obj_path = dev.PartitionSlave;
				DkDisk parent = new DkDisk(obj_path);
				d.driveType = GetDriveType(parent);
			} else if (dev.DeviceIsLuksClearText) {
				// dev is a luks-holder representing an encrypted filesystem
				DkDisk parent = new DkDisk(dev.LuksCleartextSlave);
				if (parent.IsDrive) {
					d.driveType = GetDriveType(parent);
				} else {
					if (parent.IsPartition) {
						parent = new DkDisk(parent.PartitionSlave);
						d.driveType = GetDriveType(parent);
					} else {
						d.driveType = DriveType.Unknown;
					}
				}
			} else if (dev.IsDrive) {
				d.driveType = GetDriveType(dev);
			} else {
				throw new ArgumentException("DkDisk is of an unknown type");
			}
			
			d.device = dev.DeviceFile;
			d.hasAudioCdVolume = (dev.NumAudioTracks > 0);
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
	}
}
#endif