//  AudioCdWin32
//  
//  Copyright (C) 2010 Patrick Ulbrich
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

# if WIN32
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Platform.Common.IO
{
	internal static class AudioCdWin32
	{
		private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
		private const uint OPEN_EXISTING = 3;
		
		private const uint CDROM_DISK_AUDIO_TRACK = 0x00001; 
		private const uint CDROM_DISK_DATA_TRACK = 0x00002;
		
		private const uint IOCTL_CDROM_DISK_TYPE = 0x20040;
		
		// GCHandle requires a reference type
		[StructLayout(LayoutKind.Sequential)]
        private class CDROM_DISK_DATA
		{
			public ulong DiskData;
		}
		
		public static bool IsAudioCd(string device) {
			
			int colon_idx = device.IndexOf(':');
            if (colon_idx == -1) {
                throw new ArgumentException("Invalid device name", "device");
			}
            
			string filename = string.Concat(@"\\.\", device.Substring(0, colon_idx + 1));
			
			using (SafeFileHandle file = CreateFile(filename,
			                                        GENERIC_READ,
			                                        FILE_SHARE_READ | FILE_SHARE_WRITE,
			                                        IntPtr.Zero,
			                                        OPEN_EXISTING,
			                                        0,
			                                        IntPtr.Zero)) {
				
				if (file.IsInvalid)
                    throw new IOException("Opening the CD device failed.");
				
				uint ret;
				CDROM_DISK_DATA cdd = new CDROM_DISK_DATA();
				GCHandle handle = GCHandle.Alloc(cdd, GCHandleType.Pinned);
				
				try {
					
					if (!DeviceIoControl(file,
					                     IOCTL_CDROM_DISK_TYPE,
					                     IntPtr.Zero,
					                     0,
					                     handle.AddrOfPinnedObject(),
					                     (uint)Marshal.SizeOf(cdd),
					                     out ret,
					                     IntPtr.Zero))
						throw new IOException("Error reading disk type");
				
				} finally {
					handle.Free();					
				}
				
				return ((cdd.DiskData & 0x03) == CDROM_DISK_AUDIO_TRACK);
			}
		}
		
		[DllImport ("kernel32.dll")]
        private static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode,
		                                   IntPtr lpInBuffer, uint nInBufferSize,
                                           IntPtr lpOutBuffer, uint nOutBufferSize,
                                           out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport ("kernel32.dll")]
        private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
                                                uint dwShareMode, IntPtr SecurityAttributes,
                                                uint dwCreationDisposition, uint dwFlagsAndAttributes,
                                                IntPtr hTemplateFile);
	}
}
#endif