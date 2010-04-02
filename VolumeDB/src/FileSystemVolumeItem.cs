// FileSystemVolumeItem.cs
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

namespace VolumeDB
{	
	public abstract class FileSystemVolumeItem : VolumeItem
	{
		/*
		 * Support for CreationTime was dropped for the following reasons:
		 * - Unix Filesystems don't have a CreationTime.
		 * - It seems that the CreationTime of files burned onto ISO9660/Joliet disks 
		 *	 becomes the LastWriteTime.
		 *	 Since Basenji/VolumeDB is mainly designed for CD-ROM indexing, 
		 *	 there is not much sense in supporting a timestamp that returns false 
		 *	 information on such volumes.
		 */

		internal const int MAX_LOCATION_LENGTH = 4096;
		
		private string		location;
		//private DateTime	  createdDate;
		private DateTime	lastWriteTime;
		private long		symLinkTargetID;
		
		internal FileSystemVolumeItem(VolumeDatabase database, VolumeItemType volumeItemType)
			: base(database, volumeItemType)
		{
			this.location			= null;
			//this.createdDate	 = DateTime.MinValue;
			this.lastWriteTime		= DateTime.MinValue;
			this.symLinkTargetID	= VolumeDatabase.ID_NONE;
		}
		
		/// <summary>
		/// <para>Required by internal factory methods like AbstractVolumeScanner.GetNewVolumeItem<TVolumeItem>()</para>
		/// <para>Purpose :</para>
		/// <para>
		/// - guarantee that _all_ fields of this type are initialized by the caller 
		///  (in contrast to property initialization, which easily makes you miss a property [in particular if a new one was added..])
		/// </para>
		/// <para>
		/// - seperate fields of a type from fields of its base type (e.g. GetNewVolumeItem<TVolumeItem>() initializes all fields of a the VolumeItem base type. 
		/// Caller code only needs to initialize fields of the derived <TVolumeItem> type)
		/// </para>
		/// </summary>
		internal void SetFileSystemVolumeItemFields(string location, DateTime lastWriteTime, long symLinkTargetID) {
			//ValidatePath(path);

			this.location			= location;
			//this.createdDate = createdDate;
			this.lastWriteTime		= lastWriteTime;
			this.symLinkTargetID	= symLinkTargetID;
		}
		
		//private static void ValidatePath(string path)
		//{
		//	  if (path == null)
		//		  throw new ArgumentNullException("path");

		//	  if (path.Length == 0)
		//		  throw new ArgumentException("path is emtpy");
		//}
		
		internal override void ReadFromVolumeDBRecord(IRecordData recordData) {
			base.ReadFromVolumeDBRecord(recordData);

			location		= Util.ReplaceDBNull<string>(	recordData["Location"],		null); /* root item doesnt have a location */
			//createdDate	= Util.ReplaceDBNull<DateTime>(	recordData["CreatedDate"],	DateTime.MinValue);
			lastWriteTime	= Util.ReplaceDBNull<DateTime>(	recordData["LastWriteTime"], DateTime.MinValue);
			symLinkTargetID = (long)						recordData["SymLinkTargetID"];
		}

		internal override void WriteToVolumeDBRecord(IRecordData recordData) {
			base.WriteToVolumeDBRecord(recordData);

			recordData.AddField("Location",			location);
			//recordData.AddField("CreatedDate",  m_createdDate);
			recordData.AddField("LastWriteTime",	lastWriteTime);
			//recordData.AddField("SymLinkTargetID",  symLinkTargetID < -1 ? -1 : symLinkTargetID);
			recordData.AddField("SymLinkTargetID",	symLinkTargetID);
		}
		
		#region read-only properties
		public string Location {
			get { return location ?? string.Empty; }
			internal set { location = value; }
		}

		//public DateTime CreatedDate
		//{
		//	  get { return m_createdDate; }
		//	  internal set { m_createdDate = value; }
		//}

		public DateTime LastWriteTime {
			get { return lastWriteTime; }
			internal set { lastWriteTime = value; }
		}
		
		public bool IsSymLink {
			//get { return symLinkTargetID > -1; }
			get { return symLinkTargetID != VolumeDatabase.ID_NONE; }
		}
		#endregion
		
		#region internal properties
		internal long SymLinkTargetID {
			get { return symLinkTargetID; }
			set { symLinkTargetID = value; }
		}
		#endregion
		
		public FileSystemVolumeItem GetSymLinkTargetItem() {
			if (!IsSymLink)
				throw new InvalidOperationException("This item is not a symlink");
			
			return (FileSystemVolumeItem)Database.GetVolumeItem(VolumeID, symLinkTargetID);
		}
		
		/*
		public new FileSystemVolume OwnerVolume {
			get { return ((FilesystemVolume)base.OwnerVolume); }
		}
		*/
		
	}
}
