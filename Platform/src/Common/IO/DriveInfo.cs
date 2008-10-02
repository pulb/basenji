// DriveInfo.cs
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
using System.Collections;
using System.Collections.Generic;

namespace Platform.Common.IO
{
	/*
	 *	DriveInfo class
	 *	 
	 *	The purpose of this class is to provide similar drive information on all platforms.
	 *	It should behave similar to System.IO.DriveInfo on Windows.
	 *	E.g. on Gnome, is should return all drives that are listed in Nautilus:Computer.
	 *
	 *	On linux it requires Hal to return proper information for propterties 
	 *	like Device, DriveType and VolumeLabel.
	 *	Alternatively, the implementation could be based on Gnome.Vfs.VolumeMonitor, 
	 *	but this would make it gnome-dependent 
	 *	(BAD -> class is used in platform-independed MediaDB assembly).
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
			Hal.Manager.InitBusG();
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

			System.IO.DriveInfo di = new System.IO.DriveInfo(rootPath); // throws ArgumentException if drive cant me found
			FillDriveInfo(this, di);
#else

			// remove endling slash from path
			if ((rootPath.Length > 1) && (rootPath[rootPath.Length - 1] == System.IO.Path.DirectorySeparatorChar))
				rootPath = rootPath.Substring(0, rootPath.Length - 1);
			
//			Hal.Context ctx = new Hal.Context();
//			Hal.Device[] volumes = Hal.Device.FindByStringMatch(ctx, "volume.mount_point", rootPath);
			Hal.Manager mgr = new Hal.Manager();
			Hal.Device[] volumes = mgr.FindDeviceByStringMatchAsDevice("volume.mount_point", rootPath);
			
			if (volumes.Length == 0)
				throw new ArgumentException("Can't find drive for specified path", "rootPath");
	
			FillDriveInfo(this, volumes[0]);
#endif
		}
	
		// TODO : Test
		public static DriveInfo FromDevice(string device) {
			if (device == null)
				throw new ArgumentNullException("device");
#if WIN32
			string rootPath = device; // ctor adds an ending slash ("d:\")
			return new DriveInfo(rootPath); // throws ArgumentException if drive cant be found
#else

			//Hal.Context ctx = new Hal.Context();
			//Hal.Device[] devs = Hal.Device.FindByStringMatch(ctx, "block.device", device); // can return blockdevices (storage, volume) only
			Hal.Manager mgr = new Hal.Manager();
			Hal.Device[] devs = mgr.FindDeviceByStringMatchAsDevice("block.device", device); // can return blockdevices (storage, volume) only
			
			DriveInfo d = new DriveInfo();
			
			switch(devs.Length) {
				case 0:
					throw new ArgumentException("Can't find drive for specified device", "device");
					
				case 1:
					// device is a storage OR a volume device
					if (devs[0].GetPropertyBoolean("block.is_volume")) {
						Hal.Device volume = devs[0];
						FillDriveInfo(d, volume);
					} else { // storage device
						Hal.Device storage = devs[0];
						
						if (IsPartitionableStorage(storage)) {
							/* Do not return *harddisk*-like partitionable storage devices as they may have more than one volume (i.e. partitions).
							 * HD storage-block.device and volume-block.device differ (e.g. hda -> hda1), 
							 * so we do not get both, storage and volume for the specified device argument (as in case 2).
							 * We just get the HD storage device here instead of the volume/one of the volumes.
							 * DriveInfo.IsReady would misleadingly return false, even if the drive has a volume.
							 *
							 * (In case of a media storage device (e.g. a cdrom or a floppy), this one can not contain a volume, otherwise case 2 would have been matched...)
							 */
							throw new ArgumentException("Device is a harddisk drive and may have one ore more volumes (partitions) with different devices names. Please specify the devicename of one of its volumes instead", "device");
						}
						
						FillDriveInfo(d, storage);
					}
					break;
					
				case 2:
					// we have a storage AND a volume with the same block.device (media drive, e.g. cdrom, floppy, zip, ...) -> choose volume
					Hal.Device volume = devs[0].GetPropertyBoolean("block.is_volume") ? devs[0] : devs[1];
					FillDriveInfo(d, volume);					  
					break;
					
				default:
					// we should never get here
					System.Text.StringBuilder sb = new System.Text.StringBuilder();

					sb.AppendLine("Unexpected error: got more than 2 devices for specified device");
					for (int i = 0; i < devs.Length; i++)
						sb.AppendFormat("device {0}: product={1}, category={2}\n", i, devs[0].GetPropertyString("info.product"), devs[0].GetPropertyString("info.category"));
					sb.AppendLine("Please fill a bugreport!");
				
					throw new Exception(sb.ToString());
			}
			
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

			//Hal.Context ctx = new Hal.Context();
			Hal.Manager mgr = new Hal.Manager();
			Hashtable usedStorageDevices = null;
			
			if (!readyDrivesOnly)
				usedStorageDevices = new Hashtable();
			
			/*
			 * query all VOLUMES 
			 */
			//Hal.Device[] volumes = Hal.Device.FindByCapability(ctx, "volume");
			Hal.Device[] volumes = mgr.FindDeviceByCapabilityAsDevice("volume");
			foreach(Hal.Device volume in volumes) {
				// skip igored and fixed hd volumes that aren't mounted
				if (!IsIgnoredOrUnmountedFixedVolume(volume) && !IsBootOrHomePartition(volume)) {
					DriveInfo d = new DriveInfo();
					FillDriveInfo(d, volume);
						
					string key = volume.GetPropertyString("block.storage_device");
					if (usedStorageDevices != null && !usedStorageDevices.ContainsKey(key))
						usedStorageDevices.Add(key, null);
						
					drives.Add(d);
				}
			}
			
			/*
			 * query all *removable* *floppy/cd-like* media STORAGE devices that *do not have a volume present*
			 * (cdroms, floppies, zip-drives etc..)
			 *
			 * exclude removable harddisk-like/memorystick storage devices that are partitionable.
			 * If those appear here and thus don't have a volume, they are probably not partitioned, 
			 * the volume was unmounted or something else is wrong..
			 * (it's generally a bad idea to include them (even if they contain volume(s)), 
			 * since their devicename differs from that of their volume(s).)
			 */
			if (!readyDrivesOnly) {
				//Hal.Device[] storages = Hal.Device.FindByCapability(ctx, "storage");
				Hal.Device[] storages = mgr.FindDeviceByCapabilityAsDevice("storage");
				foreach(Hal.Device storage in storages) {
					if (storage.GetPropertyBoolean("storage.removable") && !IsPartitionableStorage(storage) && !usedStorageDevices.ContainsKey(storage.GetPropertyString("block.storage_device"))) {
						DriveInfo d = new DriveInfo();
						FillDriveInfo(d, storage);
						drives.Add(d);
					}
				}	
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
		
		private static void FillDriveInfo(DriveInfo d, Hal.Device dev) {
			if (dev.GetPropertyBoolean("block.is_volume")) {
				Hal.Device volume = dev;
				
				d.volumeLabel = volume.GetPropertyString("volume.label");
				d.totalSize = (long)volume.GetPropertyUInt64("volume.size");
				d.device = volume.GetPropertyString("block.device");
				d.rootPath = volume.GetPropertyString("volume.mount_point");
				d.filesystem = volume.GetPropertyString("volume.fstype");
				d.isMounted = volume.GetPropertyBoolean("volume.is_mounted");
				d.isReady = true;
				
				Hal.Device storage = volume.Parent;
				if (storage != null)
					d.driveType = GetDriveType(storage);

			} else { // fill from storage device
				Hal.Device storage = dev;
				
				d.isMounted = d.isReady = false;
				d.driveType = GetDriveType(storage);
				d.device = storage.GetPropertyString("block.device");
			}
		}
		
		private static DriveType GetDriveType(Hal.Device storage) {
			// TODO : add support for ram and network volumes
			DriveType dt = DriveType.Unknown;
			
			if (storage.GetPropertyString("storage.drive_type") == "cdrom")
				dt = DriveType.CDRom;
			else
				dt = storage.GetPropertyBoolean("storage.removable") ? DriveType.Removable : DriveType.Fixed;
				
			return dt;
		}
		
		private static bool IsIgnoredOrUnmountedFixedVolume(Hal.Device volume) {
			// volumes marked as "ignore" except the root filesystem
			if ((volume.PropertyExists("volume.ignore") && volume.GetPropertyBoolean("volume.ignore")) && volume.GetPropertyString("volume.mount_point") != "/")
				return true;

			// unmounted fixed harddisk volumes
			Hal.Device storage = volume.Parent;
			if (storage != null) {
				if (!volume.GetPropertyBoolean("volume.is_mounted") && !storage.GetPropertyBoolean("storage.removable"))
					return true;
			}
			
			return false;
		}
		
		private static bool IsBootOrHomePartition(Hal.Device volume) {
			// TODO : is there a smarter way than checking the mountpoint?
			string mountpoint = volume.GetPropertyString("volume.mount_point");				
			
			// boot partition
			// volume.partition.flags = { 'boot' } can't be used to identify the boot bartition
			// as usb keys have this flag set too.
			if (mountpoint == "/boot")
				return true;
			
			// home partition
			if (mountpoint == "/home")
				return true;
			
			return false;		 
		}
		
		private static bool IsPartitionableStorage(Hal.Device storage) {
			 // is there a smarter way to test for partitiobale storage drives?
			 // "storage.no_partition_hint" would return true on unpartitioned harddisks (?)
			return storage.GetPropertyString("storage.drive_type") == "disk";
		}

#endif

	}

}