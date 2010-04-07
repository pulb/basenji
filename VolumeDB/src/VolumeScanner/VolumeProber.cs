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
using PlatformIO = Platform.Common.IO;

namespace VolumeDB.VolumeScanner
{
	public static class VolumeProber
	{
		/// <summary>
		/// Values returned by VolumeProber.ProbeVolume()
		/// </summary>
		public enum VolumeProbeResult {
			Unknown = 0,
			Filesystem = 1,
			AudioCd = 2
			// ...
		}
		
		public static VolumeProbeResult ProbeVolume(PlatformIO.DriveInfo drive) {
			VolumeProbeResult result = VolumeProbeResult.Unknown;
			
			if (drive == null)
				throw new ArgumentNullException("drive");
			
			if (!drive.IsReady)
				throw new ArgumentException("Drive is not ready", "drive");
			
			// check for audio cd first - 
			// win32 also mounts audio cds as filesystems
			if (drive.HasAudioCdVolume) {
				return VolumeProbeResult.AudioCd;
			} else if (drive.IsMounted) {
				return VolumeProbeResult.Filesystem;
			}
			
			return result;
		}
				
		// <summary>
		/// Probes a volume, creates the appropriate VolumeScanner and returns a general interface to it.
		/// </summary>
		/// <param name="drive">Drive to be scanned</param>
		/// <param name="database">VolumeDatabase object</param>
		/// <param name="options">ScannerOptions for all possible scanners</param>
		/// <returns>Interface to the proper VolumeScanner</returns>
		public static IVolumeScanner GetScannerForVolume(PlatformIO.DriveInfo drive,
		                                                 VolumeDatabase database,
		                                                 ScannerOptions[] options) {
			if (drive == null)
				throw new ArgumentNullException("drive");
			
			if (!drive.IsReady)
				throw new ArgumentException("Drive is not ready", "drive");
			
			if (options == null)
				throw new ArgumentNullException("options");
			
			IVolumeScanner scanner = null;
			VolumeProbeResult result = ProbeVolume(drive);
			
			switch (result) {
				case VolumeProbeResult.Filesystem:
					scanner = new FilesystemVolumeScanner(drive,
				                                      database,
				                                      GetOptions<FilesystemScannerOptions>(options));
					break;
				case VolumeProbeResult.AudioCd:				
				 	scanner = new AudioCdVolumeScanner(drive,
				                                   database,
				                                   GetOptions<AudioCdScannerOptions>(options));
					break;
				case VolumeProbeResult.Unknown:
					throw new ArgumentException("Volume is of an unknown type");
				default:
					throw new NotImplementedException(string.Format("VolumeProbeResult {0} is not implemented", result.ToString()));
			}
			
			return scanner;
		}
		
		private static TOpts GetOptions<TOpts>(ScannerOptions[] options) 
			where TOpts : ScannerOptions {
			
			foreach (ScannerOptions opt in options) {
				if (opt is TOpts)
					return (TOpts)opt;
			}
			
			throw new ArgumentException(string.Format("Missing options for type {0}", typeof(TOpts)));
		}
	}
}