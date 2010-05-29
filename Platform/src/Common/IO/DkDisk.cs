//
// DkDisk.cs
//
// Author:
//   Gabriel Burt <gburt@novell.com>
//   Patrick Ulbrich <zulu99@gmx.net>
//
// Copyright (C) 2009 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !WIN32
using System;

using NDesk.DBus;
using System.Collections.Generic;

namespace Platform.Common.IO
{
	// modified/extended DeviceKit binding from Banshee
	// (http://banshee-project.org/)
    internal class DkDisk
    {
		// required by gtk applications 
		// to avoid multithreading issues
		public static void InitBusG() {
			BusG.Init();
		}
		
		public static DkDisk[] EnumerateDevices ()
		{			
			if (disks == null)
                return null;
			
			List<DkDisk> lst = new List<DkDisk>();
			string[] disk_paths = disks.EnumerateDevices();
			
			foreach (string path in disk_paths) {
				DkDisk d = new DkDisk(path);
				lst.Add(d);
			}
			
			return lst.ToArray();
		}
		
        public static DkDisk FindByDevice (string device_path)
        {
            if (device_path == null)
                return null;

            if (disks == null)
                return null;


            string disk_path = null;
            try {
                disk_path = disks.FindDeviceByDeviceFile (device_path);
            } catch {}

            if (disk_path == null)
                return null;

            try {
                return new DkDisk (disk_path);
            } catch {}

            return null;
        }

        private IDkDisk disk;
        private org.freedesktop.DBus.Properties props;

        public DkDisk (string obj_path)
        {
            disk = Bus.System.GetObject<IDkDisk>("org.freedesktop.UDisks",
                new ObjectPath(obj_path));

            props = Bus.System.GetObject<org.freedesktop.DBus.Properties>("org.freedesktop.UDisks",
                new ObjectPath(obj_path));
        }

        public bool IsMounted {
            get {
                return (bool) props.Get ("org.freedesktop.UDisks.Device", "DeviceIsMounted");
            }
        }

        public bool IsReadOnly {
            get {
                return (bool) props.Get ("org.freedesktop.UDisks.Device", "DeviceIsReadOnly");
            }
        }

        public string MountPoint {
            get {
                var ary = (string[])props.Get ("org.freedesktop.UDisks.Device", "DeviceMountPaths");
                return ary != null && ary.Length > 0 ? ary[0] : null;
            }
        }
		
		public bool IsMediaAvailable {
            get {
                return (bool) props.Get ("org.freedesktop.UDisks.Device", "DeviceIsMediaAvailable");
            }
        }
		
		public string Label {
            get {
                return (string) props.Get ("org.freedesktop.UDisks.Device", "IdLabel");
            }
        }
		
		public string IdType {
            get {
                return (string) props.Get ("org.freedesktop.UDisks.Device", "IdType");
            }
        }
		
		public bool IsPartitionTable {
            get {
                return (bool) props.Get ("org.freedesktop.UDisks.Device", "DeviceIsPartitionTable");
            }
        }
		
		public bool IsPartition {
            get {
                return (bool) props.Get ("org.freedesktop.UDisks.Device", "DeviceIsPartition");
            }
        }
		
		public string PartitionSlave {
            get {
                return ((ObjectPath) props.Get ("org.freedesktop.UDisks.Device", "PartitionSlave")).ToString();
            }
        }
		
		public bool DeviceIsLuksClearText {
            get {
                return (bool) props.Get ("org.freedesktop.UDisks.Device", "DeviceIsLuksCleartext");
            }
        }
		
		public string LuksCleartextSlave {
            get {
                return ((ObjectPath)props.Get ("org.freedesktop.UDisks.Device", "LuksCleartextSlave")).ToString();
            }
        }
		
		public ulong Size {
            get {
                return (ulong) props.Get ("org.freedesktop.UDisks.Device", "DeviceSize");
            }
        }
		
		public string DeviceFile {
            get {
                return (string) props.Get ("org.freedesktop.UDisks.Device", "DeviceFile");
            }
        }
		
		public bool IsDrive {
            get {
                return (bool) props.Get ("org.freedesktop.UDisks.Device", "DeviceIsDrive");
            }
        }
		
		public bool IsRemovable {
            get {
                return (bool) props.Get ("org.freedesktop.UDisks.Device", "DeviceIsRemovable");
            }
        }
		
		public string[] MediaCompatibility {
            get {
                return (string[]) props.Get ("org.freedesktop.UDisks.Device", "DriveMediaCompatibility");
            }
        }
		
		public uint NumAudioTracks {
			get {
				return (uint) props.Get ("org.freedesktop.UDisks.Device", "OpticalDiscNumAudioTracks");
			}
		}

        public void Eject ()
        {
            disk.DriveEject (new string [0]);
        }

        public void Unmount ()
        {
            disk.FilesystemUnmount (new string [0]);
        }

        private static IDkDisks disks;

        static DkDisk ()
        {
            try {
                disks = Bus.System.GetObject<IDkDisks>("org.freedesktop.UDisks",
                    new ObjectPath("/org/freedesktop/UDisks"));
            } catch {}
        }

        [Interface("org.freedesktop.UDisks")]
        internal interface IDkDisks
        {
            string FindDeviceByDeviceFile (string deviceFile);
			string[] EnumerateDevices ();
        }

    }

    [Interface("org.freedesktop.UDisks.Device")]
    public interface IDkDisk
    {
        bool DeviceIsMounted { get; }
        string [] DeviceMountPaths { get; }
        void DriveEject (string [] options);
        void FilesystemUnmount (string [] options);
    }
}
#endif