// VolumeInfo.cs
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

namespace VolumeDB.VolumeScanner
{	
	/* 
	 * Class for the VolumeInfo property of the AbstractVolumeScanner class.
	 * It provides basic readonly info about the volume being scanned.
	 * There is no need to make properties threadsafe since they are written to 
	 * on AbstractVolumeScanner construction time only.
	 * (this does not apply to derived classes which are populated during scanning! 
	 * (client may read while scanner writes))
	 */
	public abstract class VolumeInfo
	{	
		private Volume volume;
		
		internal VolumeInfo(Volume v) {
			this.volume = v;
		}
		
		internal abstract void Reset();
		
		// The VolumeID should not be exposed here to prevent users from accessing the 
		// Volume in the database before it and its items have been written to database completely.
		// The VolumeID will be available in the scanners ScanCompleted event that will be 
		// raised when scanning has been finished.
		public string			ArchiveNo	{ get { return volume.ArchiveNo;	} }
		public string			Title		{ get { return volume.Title;		} }
		public DateTime			Added		{ get { return volume.Added;		} }
		public bool				IsHashed	{ get { return volume.IsHashed;		} }
		public VolumeDriveType	DriveType	{ get { return volume.DriveType;	} }
		
		public VolumeType		GetVolumeType() { return volume.GetVolumeType(); }
	}
}
