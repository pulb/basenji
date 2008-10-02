using System;

namespace VolumeDB.VolumeScanner
{	
	/* 
	 * Class for the VolumeInfo property of the VolumeScannerBase class.
	 * It provides basic readonly info about the volume being scanned.
	 * There is no need to make properties threadsafe since they are written to 
	 * on VolumeScanner construction time only.
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
		
		public string			ArchiveNr	{ get { return volume.ArchiveNr;	} }
		public string			Title		{ get { return volume.Title;		} }
		public DateTime			Added		{ get { return volume.Added;		} }
		public bool				IsHashed	{ get { return volume.IsHashed;		} }
		public VolumeDriveType	DriveType	{ get { return volume.DriveType;	} }
		
		public VolumeType		GetVolumeType() { return volume.GetVolumeType(); }
	}
}
