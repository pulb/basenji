// Volume.cs
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
//using System.Collections.Generic;

// TODO : overwrite ToString() reasonably
// TODO : IDiposable to free VolumeDatabase? (if yes, do so in VolumeITEM as well!)
// TODO : if this class gets a method in terms of GetFiles(), make sure that it can't be called while the class is filled by a scanner!
namespace VolumeDB
{
	public abstract class Volume : VolumeDBDataType
	{	
		// length constants used by 
		// - client programs to validate user input		
		// - this class to validate _user_ input to _public_ properties
		// - VolumeDatabase when creating tables
		public const int MAX_TITLE_LENGTH		= 64;
		public const int MAX_ARCHIVE_NO_LENGTH	= 16;
		public const int MAX_LOANED_TO_LENGTH	= 64;
		public const int MAX_CATEGORY_LENGTH	= 64;
		public const int MAX_DESCRIPTION_LENGTH = 4096;
		public const int MAX_KEYWORDS_LENGTH	= 4096;
		
		// table info required by VolumeDBDataType
		private const			string		tableName			= "Volumes";
		private static readonly string[]	primarykeyFields	= { "VolumeID" };
		
		private long			volumeID;
		private string			title;
		private DateTime		added;
		private bool			isHashed;
		
		private string			archiveNo;
		private VolumeDriveType driveType;
		private string			loanedTo;
		private DateTime		loanedDate;
		private DateTime		returnDate;
		private string			category;
		private string			description;
		private string			keywords;
		//private string		  clientAppData;

		private VolumeDatabase	database;

		private VolumeType		volumeType;
		
		internal Volume(VolumeDatabase database, VolumeType volumeType)
			: base(tableName, primarykeyFields)
		{
			this.volumeID		= 0L;
			this.title			= null;
			this.added			= DateTime.MinValue;
			this.isHashed		= false;

			this.archiveNo		= null;
			this.driveType		= VolumeDriveType.Unknown;
			this.loanedTo		= null;
			this.loanedDate		= DateTime.MinValue;
			this.returnDate		= DateTime.MinValue;
			this.category		= null;
			this.description	= null;
			this.keywords		= null;
			//this.clientAppData  = null;

			this.database		= database;

			this.volumeType		= volumeType;
		}
		
		/// <summary>
		/// <para>Required by internal factory methods like AbstractVolumeScanner.CreateVolumeObject()</para>
		/// <para>Purpose :</para>
		/// <para>
		/// - guarantee that _all_ fields of this type are initialized by the caller 
		///  (in contrast to property initialization, which easily makes you miss a property [in particular if a new one was added..])
		/// </para>
		/// <para>
		/// - seperate fields of a type from fields of its base type (e.g. AbstractVolumeScanner.CreateVolumeObject() initializes all fields of a the Volume base type. 
		/// Caller code only needs to initialize fields of the derived Volume type)
		/// </para>
		/// </summary>
		internal void SetVolumeFields(
			long volumeID,
			string title,
			DateTime added,
			bool isHashed,

			string archiveNo,
			VolumeDriveType driveType,
			string loanedTo,
			DateTime loanedDate,
			DateTime returnDate,
			string category,
			string description,
			string keywords /*,
			string clientAppData*/)
		{
			this.volumeID	   = volumeID;
			this.title		   = title;
			this.added		   = added;
			this.isHashed	   = isHashed;

			this.archiveNo	   = archiveNo;
			this.driveType	   = driveType;
			this.loanedTo	   = loanedTo;
			this.loanedDate    = loanedDate;
			this.returnDate    = returnDate;
			this.category	   = category;
			this.description   = description;
			this.keywords	   = keywords;
			//this.clientAppData  = clientAppData;
		}

		public VolumeType GetVolumeType() {
			return volumeType;
		}
		
		public IContainerItem GetRoot() {
			return Database.GetVolumeRoot<IContainerItem>(volumeID);
		}
		
		internal override void ReadFromVolumeDBRecord(IRecordData recordData) {
			volumeID	  = (long)						  	recordData["VolumeID"];
			title		  = Util.ReplaceDBNull<string>(		recordData["Title"], null);
			added		  = Util.ReplaceDBNull<DateTime>(	recordData["Added"], DateTime.MinValue);
			isHashed	  = (bool)						  	recordData["IsHashed"];

			archiveNo	  = Util.ReplaceDBNull<string>(		recordData["ArchiveNr"], null);
			driveType	  = (VolumeDriveType)(int)(long)  	recordData["DriveType"];
			loanedTo	  = Util.ReplaceDBNull<string>(		recordData["Loaned_To"], null);
			loanedDate	  = Util.ReplaceDBNull<DateTime>(	recordData["Loaned_Date"], DateTime.MinValue);
			returnDate	  = Util.ReplaceDBNull<DateTime>(	recordData["Return_Date"], DateTime.MinValue);
			category	  = Util.ReplaceDBNull<string>(		recordData["Category"], null);
			description   = Util.ReplaceDBNull<string>(		recordData["Description"], null);
			keywords	  = Util.ReplaceDBNull<string>(		recordData["Keywords"], null);
			//clientAppData   = Util.ReplaceDBNull<string>(		  recordData["ClientAppData"], null);
		}

		internal override void WriteToVolumeDBRecord(IRecordData recordData) {
			recordData.AddField("VolumeID",		volumeID);
			recordData.AddField("Title",		title);
			recordData.AddField("Added",		added);
			recordData.AddField("VolumeType",	volumeType);
			recordData.AddField("IsHashed",		isHashed);

			recordData.AddField("ArchiveNr",	archiveNo);
			recordData.AddField("DriveType",	driveType);
			recordData.AddField("Loaned_To",	loanedTo);
			recordData.AddField("Loaned_Date",	loanedDate);
			recordData.AddField("Return_Date",	returnDate);
			recordData.AddField("Category",		category);
			recordData.AddField("Description",	description);
			recordData.AddField("Keywords",		keywords);
			//recordData.AddField("ClientAppData",	  clientAppData);
		}
		
		internal override void InsertIntoDB() {
			Database.InsertVolume(this);
		}

		public override void UpdateChanges() {
			Database.UpdateVolume(this);
		}
		
		internal static Volume CreateInstance(VolumeType type, VolumeDatabase database) {
			Volume volume = null;
			switch (type) {
				case VolumeType.FileSystemVolume:
					volume = new FileSystemVolume(database);
					break;
				case VolumeType.AudioCdVolume:
					volume = new AudioCdVolume(database);
					break;
				default:
					throw new NotImplementedException(string.Format("Instanciation of type {0} is not implemented", type.ToString()));
			}
			return volume;
		}
		
		#region read-only properties

		public long VolumeID {
			get				{ return volumeID; }
			internal set	{ volumeID = value; }
		}

		public DateTime Added {
			get				{ return added; }
			internal set	{ added = value; }
		}		

		public bool IsHashed {
			get				{ return isHashed; }
			internal set	{ isHashed = value; }
		}

		#endregion
		
		#region editable properties

		/*	it's not always possible to retrieve a title from a volume,
		 *	some volumes don't even have a name (especially on linux).
		 *	allow the user to specify the title manually.
		 */
		public string Title {
			get { return title ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_TITLE_LENGTH);
				title = value;
			}
		}

		public string ArchiveNo {
			get { return archiveNo ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_ARCHIVE_NO_LENGTH);
				archiveNo = value;
			}
		}

		public VolumeDriveType DriveType {
			get { return driveType; }
			set { driveType = value; }
		}

		public string LoanedTo {
			get { return loanedTo ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_LOANED_TO_LENGTH);
				loanedTo = value;
			}
		}

		public DateTime LoanedDate {
			get { return loanedDate; }
			set { loanedDate = value; }
		}

		public DateTime ReturnDate {
			get { return returnDate; }
			set { returnDate = value; }
		}

		public string Category {
			get { return category ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_CATEGORY_LENGTH);
				category = value;
			}
		}

		public string Description {
			get { return description ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_DESCRIPTION_LENGTH);
				description = value;
			}
		}

		public string Keywords {
			get { return keywords ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_KEYWORDS_LENGTH);
				keywords = value;
			}
		}

//		///<summary>
//		///Data a client application (app that is using this library) can attach to a Volume object.
//		///</summary>
//		public string ClientAppData {
//			get { return clientAppData ?? string.Empty; }
//			set { clientAppData = value; }
//		}
		#endregion
		
		public override string ToString() {
			return Title;		 
		}
		
		protected VolumeDatabase Database {
			get {
				if (database == null)
					throw new InvalidOperationException("No database associated");
				return database;
			}
		}
		
	}
}
