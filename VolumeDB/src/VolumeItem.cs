// VolumeItem.cs
// 
// Copyright (C) 2008, 2010, 2011 Patrick Ulbrich
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
using System.Collections.Generic;
using VolumeDB.Metadata;

// TODO : IDiposable to free VolumeDatabase? (if yes, do so in Volume class as well!)
namespace VolumeDB
{
	/// <summary>
	/// Base class of all VolumeItem types (files, directories, ...)
	/// </summary>
	public abstract class VolumeItem : VolumeDBDataType, IChildItem
	{
		// length constants used by 
		// - client programs to validate user input		
		// - this class to validate _user_ input to _public_ properties
		// - VolumeDatabase when creating tables
		public const int MAX_NAME_LENGTH		= 256;
		public const int MAX_NOTE_LENGTH		= 4096;
		public const int MAX_KEYWORDS_LENGTH	= 4096;

		internal const int MAX_MIMETYPE_LENGTH	= 64;
		internal const int MAX_METADATA_LENGTH	= 4096;
		internal const int MAX_HASH_LENGTH		= 64;

		// table info required by VolumeDBDataType
		private const			string		tableName			= "Items";
		private static readonly string[]	primarykeyFields	= { "VolumeID", "ItemID" };
		
		private long			volumeID;		 
		private long			itemID;		   
		//private long rootID;
		private long			parentID;
		
		private string			name;
		private string			mimeType; // content type
		private MetadataStore	metaData;
		private string			note;
		private string			keywords;
		
		//private Volume		  ownerVolume;
		
		private VolumeDatabase	database;
		
		private VolumeItemType	itemType;
		
		internal VolumeItem(VolumeDatabase database, VolumeItemType itemType)
			: base(tableName, primarykeyFields)
		{
			this.volumeID		= 0L;
			this.itemID			= 0L;
			//this.rootID	  = 0L;
			this.parentID		= 0L;
			
			this.name			= null;
			this.mimeType		= null;
			this.metaData		= MetadataStore.Empty;
			this.note			= null;
			this.keywords		= null;
			
			//this.ownerVolume	  = null;
			
			this.database		= database;
			this.itemType		= itemType;
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
		/// Caller code only needs to initialize fields of the derived <TMediaItem> type)
		/// </para>
		/// </summary>
		internal void SetVolumeItemFields(
			long volumeID,
			long itemID,
			//long rootID,
			long parentID,

			string name,
			string mimeType,
			MetadataStore metaData,
			string note,
			string keywords

			/*Volume ownerVolume*/
			)
		{
			this.volumeID	= volumeID;			   
			this.itemID		= itemID;			 
			//this.m_rootID = rootID;
			this.parentID	= parentID;

			this.name		= name;
			this.mimeType	= mimeType;
			this.metaData	= metaData;
			this.note		= note;
			this.keywords	= keywords;

			//this.ownerVolume = ownerVolume;
		}

		public VolumeItemType GetVolumeItemType() {
			return itemType;
		}
		
		#region IChildItem Members

		// implemented explicitely as derived VolumeItem types may not have a parent item. 
		// if they have, they implement a specific parent-getter if needed (e.g. FileVolumeItem.GetDirectory()).
		IContainerItem IChildItem.GetParent() {
			 return (IContainerItem)Database.GetVolumeItem(volumeID, parentID);
		}

		#endregion
		
		public Volume GetOwnerVolume() {
			/*
				do not cache, 
				always pull a fresh instance from the database
				to get possible changes made to the owner volume record.
			*/
			return Database.GetVolume(volumeID);
		}
		
		internal override void ReadFromVolumeDBRecord(IRecordData recordData) {
			volumeID	= (long)				  		recordData["VolumeID"];			 
			itemID		= (long)				  		recordData["ItemID"];
			//rootID	= (long)				  recordData["RootID"];
			parentID	= (long)				  		recordData["ParentID"];
			name		= Util.ReplaceDBNull<string>(	recordData["Name"],		null);
			mimeType	= Util.ReplaceDBNull<string>(	recordData["MimeType"],	null);
			metaData	= new MetadataStore(Util.ReplaceDBNull<string>(	recordData["MetaData"],	null));
			note		= Util.ReplaceDBNull<string>(	recordData["Note"],		null);
			keywords	= Util.ReplaceDBNull<string>(	recordData["Keywords"],	null);
		}

		internal override void WriteToVolumeDBRecord(IRecordData recordData) {
			recordData.AddField("VolumeID", volumeID);			  
			recordData.AddField("ItemID",	itemID);
			//recordData.AddField("RootID",  rootID);
			recordData.AddField("ParentID", parentID);
			recordData.AddField("ItemType", itemType);
			recordData.AddField("Name",		name);
			recordData.AddField("MimeType", mimeType);
			// NOTE : metadata can't be null since it is a struct
			recordData.AddField("MetaData", metaData.MetadataString);
			recordData.AddField("Note",		note);
			recordData.AddField("Keywords", keywords);			  
		}
		
		internal override void InsertIntoDB() {
			// TODO : owner volume must not be saved 
			// -- if there are many items to be updated, 
			// the ownerVolume will be updated in the database for every single item as well
			Database.InsertVolumeItem(this);
		}

		public override void UpdateChanges() {
			// TODO : owner volume must not be saved 
			// -- if there are many items to be updated, 
			// the ownerVolume will be updated in the database for every single item as well
			Database.UpdateVolumeItem(this);
		}
		
		internal static VolumeItem CreateInstance(VolumeItemType type, VolumeDatabase database) {
			VolumeItem item = null;
			switch (type) {
				case VolumeItemType.DirectoryVolumeItem:
					item = new DirectoryVolumeItem(database);
					break;
				case VolumeItemType.FileVolumeItem:
					item = new FileVolumeItem(database);
					break;
				case VolumeItemType.AudioCdRootVolumeItem: 
					item = new AudioCdRootVolumeItem(database);
					break;
				case VolumeItemType.AudioTrackVolumeItem:
					item = new AudioTrackVolumeItem(database);
					break;
				default:
					throw new NotImplementedException(string.Format("Instantiation of type {0} is not implemented", type.ToString()));
			}
			return item;
		}
		
//		  //TODO : remove this method if generic constraints allow internal and parameterized constructors.
//		  //It is used as a workaround to instanciate a VolumeItem object, 
//		  //because the internal VolumeItem constructor with the database parameter can't be used in generic code.
//		  //see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=9a8e58ee-1371-4e99-8385-c3e2a4157fd6
//		  //see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=80517ec1-2d08-43cc-bc90-9927877061a9
//		  internal static VolumeItem CreateInstance<TVolumeItem>(VolumeDB database) 
//			  where TVolumeItem : VolumeItem
//		  {
//			  Type t = typeof(TVolumeItem);
//			  
//			  if (t == typeof(FileVolumeItem))
//				  return new FileVolumeItem(database);
//			  else if(t == typeof(DirectoryVolumeItem))
//				  return new DirectoryVolumeItem(database);
//			  else
//				  throw new NotImplementedException(string.Format("Instanciation of type {0} is not implemented.", t.ToString()));
//		  }
		
		#region read-only properties

		public long VolumeID {
			get				{ return volumeID; }
			internal set	{ volumeID = value; }
		}
		
		public long ItemID {
			get				{ return itemID; }
			internal set	{ itemID = value; }
		}

		//// TODO : make internal after reorganisation if it isn't needed publicly
		//public long RootID
		//{
		//	  get { return m_rootID; }
		//	  internal set { m_rootID = value; }
		//}

		public long ParentID {
			get				{ return parentID; }
			internal set	{ parentID = value; }
		}

		public string Name {
			get				{ return name ?? string.Empty; }
			internal set	{ name = value; }
		}
		
		public string MimeType {
			get				{ return mimeType ?? string.Empty; }
			internal set	{ mimeType = value; }
		}
		
		public MetadataStore MetaData {
			// NOTE : metadata can't be null since it is a struct
			get				{ return metaData; }
			internal set	{ metaData = value;	}
		}

		#endregion
		
		#region editable properties
		public string Note {
			get { return note ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_NOTE_LENGTH);				 
				note = value;
			}
		}

		public string Keywords
		{
			get { return keywords ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_KEYWORDS_LENGTH);
				keywords = value;
			}
		}
		#endregion
		
		public override string ToString() {
			return Name;		
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