// AudioCdVolumeScanner.cs
// 
// Copyright (C) 2010 Patrick Ulbrich
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
using PlatformIO = Platform.Common.IO;

namespace VolumeDB.VolumeScanner
{
	public sealed class AudioCdVolumeScanner 
		: AbstractVolumeScanner<AudioCdVolume, AudioCdVolumeInfo, AudioCdScannerOptions>
	{
		// note:
		// do not allow to modify the constuctor parameters 
		// (i.e. database, options)
		// through public properties later, since the scanner 
		// may already use them after scanning has been started,
		// and some stuff has been initialized depending on the 
		// options in the ctor already.
		public AudioCdVolumeScanner(Platform.Common.IO.DriveInfo drive,
		                            VolumeDatabase database,
		                            AudioCdScannerOptions options)
			: base(drive, database, options)
		{
			if (!drive.VolumeIsAudioCd)
				throw new ArgumentException("No audio cd present in drive");
			
			
		}
		
		internal override void ScanningThreadMain(PlatformIO.DriveInfo drive,
		                                          AudioCdVolume volume,
		                                          BufferedVolumeItemWriter writer) {
			
			if (Options.ComputeHashs)
				SendScannerWarning(S._("Hashcode generation not implemented for audio cds yet."));
		}

		protected override void Reset() {
			base.Reset();
		}
		
		protected override void Dispose (bool disposing) {
			base.Dispose(disposing);
		}
	}
}
