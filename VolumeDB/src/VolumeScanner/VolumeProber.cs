// VolumeProber.cs
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

// TODO :
// use inline docs for GetScanner() in MediaScannerBase ctor and GenericMediaScanner as well
using System;

namespace VolumeDB.VolumeScanner
{
	/// <summary>
	/// Values returned by VolumeProber.ProbeVolume()
	/// </summary>
	public enum VolumeProbeResult {
		Unknown = 0,
		FilesystemVolume = 1
		//CDDAVolume...
	}
	
	public static class VolumeProber
	{
		public static VolumeProbeResult ProbeVolume(string device) {
			if (device == null)
				throw new ArgumentNullException("device");
			
			if (device.Length == 0)
				throw new ArgumentException("Invalid devicename");
				
			// TODO : implement me!
			// use Platform.Common.IO.DriveInfo.FromDevice(device);
			const bool IS_FS_MEDIA = true, IS_CDDA_MEDIA = false;
			if (IS_FS_MEDIA) {
				return VolumeProbeResult.FilesystemVolume;
			} else if (IS_CDDA_MEDIA) {
				// ...
			}
			
			return VolumeProbeResult.Unknown;
		}
		
		/// <summary>
		/// Probes a volume, creates the appropriate VolumeScanner and returns a general interface to it.
		/// </summary>
		/// <param name="device">Devicefile of the volume to be scanned</param>
		/// <param name="database">VolumeDatabase that will be filled with scanning results</param>
		/// <returns>Interface to the proper VolumeScanner</returns>
		public static IVolumeScanner GetScanner(string device, VolumeDatabase database) {
			VolumeProbeResult	result	= ProbeVolume(device);
			IVolumeScanner		scanner = null;
			
			// create specific volumescanner
			switch(result) {
				case VolumeProbeResult.FilesystemVolume:
					scanner =  new FilesystemVolumeScanner(device, database, new FilesystemScannerOptions());
					break;
				// case VolumeProbeResult.CDDAVolume ..
				case VolumeProbeResult.Unknown:
					throw new NotSupportedException("Volume is of an unknown type");
				  default:
					throw new NotImplementedException(string.Format("VolumeProbeResult {0} is not implemented", result.ToString()));
			}
			
			return scanner;
		}
	}
}
