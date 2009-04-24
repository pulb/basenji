// VolumeDatabase.cs
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
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.Remoting.Messaging;
using VolumeDB.Searching;
using Platform.Common.Diagnostics;

// MediaDirectory[] Media.GetDirectories() ?
// MediaDirectory[] dir.GetDirectories() ?
namespace VolumeDB
{	 
	/*
	About threadsafety:
	  it's only save to read/write concurrently to a database file from _one single_ VolumeDatabase instance 
	  (i.e. multiple threads may access the same VolumeDatabase object simultanously).
	  it's not save to read/write to a database file from multiple VolumeDatabase instances / multiple processes.
	  To achive this functionality database-level locking must be implemented (not supported by current Sqlite ADO providers).

	  The VolumeDatabase class was designed with compatibility to most sql databases backends in mind.
	  To achive compatibility with most sql db backends, database access logic must be implemented with the least common dominator in mind, 
	  esp. when it comes to multihreading.
	  The least common dominator is libSQLite - which is single threaded. 
	
	Note to maintainers:
	  _every_ access to the conn object/a DataReader must be surrounded by the EnterConnectionLock()/ExitConnectionLock() methods.
	  Though, using the conn object/DataReaders explicitly shouldn't be necessary - the Execute*() helper methods should fullfil most needs and handle locking already.
	*/
	
	///<summary>
	///Class representing the database that holds contents/infotmation of scanned volumes.
	///</summary>
	public sealed partial class VolumeDatabase : IDisposable
	{		
		private	const	int		DB_VERSION = 1;
		private const	string	SQL_DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
		
		// prevent long wating time / massive mem consumption by limiting the searchstr length
		public	const	int		MIN_SEARCHSTR_LENGTH = 3;
		
		internal const	long	ID_NONE  = 0; // represents a "not-set" value for foreign keys to ID fields (e.g. Volumes.VolumeID, Items.ItemID)
		internal const	long	ID_FIRST = 1; // first ID of a table record		
		
		private bool			disposed;

		private SqlBackend		sql;
		
		// prevents nasty item searches with too many results 
		// from cosuming all mem (and time...). -1 = disabled.
		private int				searchItemResultsLimit;
		
		public VolumeDatabase(string dbPath) : this(dbPath, false) {}
		public VolumeDatabase(string dbPath, bool create) {
			if (dbPath == null)
				throw new ArgumentNullException("dbPath");
			
			disposed				= false;
			sql						= new SqlBackend(dbPath, create, this);			
			searchItemResultsLimit	= -1;
			
			if (create) {
				CreateTables();
			} else {
				int version = GetDBProperties().Version;
				if (version != DB_VERSION) {
					sql.Close();
					throw new UnsupportedDbVersionException(string.Format("Found version {0}, expected version is {1}", version, DB_VERSION));
				}
			}
		}
		
		#region Events
		
		public event EventHandler BeginWriteAccess;
		public event EventHandler EndWriteAccess;

		private void OnBeginWriteAccess(EventArgs e) {
			if (this.BeginWriteAccess != null)
				this.BeginWriteAccess(this, e);
		}

		private void OnEndWriteAccess(EventArgs e) {
			if (this.EndWriteAccess != null)
				this.EndWriteAccess(this, e);
		}
		
		#endregion
		
		public void Close() {
			Dispose(true);
		}

		public bool IsClosed {
			get { return disposed; }
		}
		
		public DatabaseProperties GetDBProperties() {
			EnsureOpen();
			DatabaseProperties p = new DatabaseProperties(this);
			IVolumeDBRecord record = p;
			string query = string.Format("SELECT * FROM {0}", record.TableName);
			
			sql.ExecuteReader(query, delegate(IDataReader reader, IRecordData readerRecData) {
				reader.Read();
				record.SetRecordData(readerRecData);
				record.IsNew = false;
			});
			
			return p;
		}
		
		public void UpdateDBProperties(DatabaseProperties p) {
			EnsureOpen();
			if (p == null)
				throw new ArgumentNullException("p");

			WriteRecord(p, true);
		}

		public Volume GetVolume(long volumeID) {
			EnsureOpen();
			// TODO : VolumeDatabase shouldnt hardcode table fields or table names
			Volume[] volumes = QueryVolumes(string.Format("SELECT * FROM Volumes WHERE VolumeID = {0}", volumeID));
			return volumes.Length == 0 ? null : volumes[0];
		}
		
		public IAsyncResult BeginSearchVolume(AsyncCallback callback, object state) {
			//if (callback == null)
			//	  throw new ArgumentNullException("callback");
			
			EnsureOpen();

			BeginSearchDelegate<Volume> d = new BeginSearchDelegate<Volume>(_SearchVolume);
			return d.BeginInvoke(null, callback, state);
		}
		
		public IAsyncResult BeginSearchVolume(ISearchCriteria searchCriteria, AsyncCallback callback, object state) {
			if (searchCriteria == null)
				throw new ArgumentNullException("searchCriteria");

			//if (callback == null)
			//	  throw new ArgumentNullException("callback");
			
			EnsureOpen();
			
			BeginSearchDelegate<Volume> d = new BeginSearchDelegate<Volume>(_SearchVolume);
			return d.BeginInvoke(searchCriteria, callback, state);
		}
		
		public Volume[] EndSearchVolume(IAsyncResult asyncResult) {
			//EnsureOpen(); // DON'T!
			if (asyncResult == null)
				throw new ArgumentNullException("asyncResult");

			BeginSearchDelegate<Volume> d = (BeginSearchDelegate<Volume>) ((AsyncResult)asyncResult).AsyncDelegate;
			return d.EndInvoke(asyncResult);
		}
		
		public Volume[] SearchVolume() {
			EnsureOpen();
			return _SearchVolume(null);
		 }
		
		public Volume[] SearchVolume(ISearchCriteria searchCriteria) {
			if (searchCriteria == null)
				throw new ArgumentNullException("searchCriteria");

			EnsureOpen();

			return _SearchVolume(searchCriteria);
		}
		
		private Volume[] _SearchVolume(ISearchCriteria searchCriteria) {
			// TODO : check SQL output! implemetation was rewritten from scratch!!
			// TODO : VolumeDatabase shouln't hardcode fieldnames or tablenames
			// TODO : Volume.TableName, Volume.IDField?
			//const string ORDER_FIELD = "Volumes.VolumeID"; // slows down searching 

			/*** build sql query ***/
			string sqlQuery;
			//sqlQuery = string.Format("SELECT * FROM Volumes WHERE {0} ORDER BY {1};", condition, ORDER_FIELD);
			
			if (searchCriteria == null) {
				// when searching volumes, the searchcriteria may be optional
				// since the resultset can not become as big as in in item searches.
				// apart from that there must be a way to retrieve all volumes in the database.
				sqlQuery = "SELECT * FROM Volumes;"; 
			} else {
				string condition = searchCriteria.GetSqlSearchCondition();

				if (condition.Length == 0) // e.g. empty SearchCriteriaGroup
				   throw new ArgumentException("SearchCriteria is empty", "searchCriteria");
				
				if ((searchCriteria.SearchCriteriaType & SearchCriteriaType.ItemSearchCriteria) == SearchCriteriaType.ItemSearchCriteria) {
					// searchriteria contains item searchriteria -> join items table
					sqlQuery = string.Format("SELECT DISTINCT Volumes.* FROM Volumes, Items WHERE ({0}) AND (Volumes.VolumeID = Items.VolumeID);", condition);
				} else {					
					sqlQuery = string.Format("SELECT * FROM Volumes WHERE {0};", condition);
				}
			}
			
			Debug.WriteLine(string.Format("_SearchVolume() executes query: '{0}'", sqlQuery));
			return QueryVolumes(sqlQuery);
		}
		
		/// <summary>
		/// Removes a volume and all associated items.
		/// </summary>
		/// <param name="volume">Volume object to be removed from the database.</param>
		public void RemoveVolume(Volume volume) {
			RemoveVolume(volume.VolumeID);
		}
		
		/// <summary>
		/// Removes a volume and all associated items.
		/// </summary>
		/// <param name="volumeID">ID of the volume to be removed from the database.</param>
		public void RemoveVolume(long volumeID) {
			// TODO : what if a volume has been deleted from the database physicaly, but Insert/UpdataeChanges() is called on the object representing the volume?
			// TODO : the same applies to VolumeItems..
			// TODO : it's a general problem affecting volume/volumeitem operations on the database (e.g. VolumeItem.GetOwnerVolume())
			// TODO : review whether that's ok / how to improve that
			
			EnsureOpen();
			
			// TODO : VolumeDatabase shouldnt hardcode table fields
			string[] deleteCommands = {
				"DELETE FROM Items WHERE VolumeID = " + volumeID,
				"DELETE FROM Volumes WHERE VolumeID = " + volumeID
			};
			sql.ExecuteNonQuery(deleteCommands);
		}
		
		public void UpdateVolume(Volume volume) {
			EnsureOpen();
			WriteRecord(volume, true);
		}
		
		internal void InsertVolume(Volume volume) {
			EnsureOpen();
			WriteRecord(volume, false);
		}
		
		public void UpdateVolumeItem(VolumeItem item) {
			UpdateVolumeItems( new VolumeItem[] { item } );
		}
		
		public void UpdateVolumeItems(VolumeItem[] items) {
			EnsureOpen();
			WriteRecords((IVolumeDBRecord[])items, true);
		}
		
		internal void InsertVolumeItem(VolumeItem item) {
			InsertVolumeItems(new VolumeItem[] { item });
		}
		
		internal void InsertVolumeItems(VolumeItem[] items) {
			EnsureOpen();
			WriteRecords((IVolumeDBRecord[])items, false);
		}
		
		public VolumeItem GetVolumeItem(long volumeID, long itemID) {
			EnsureOpen();

			// TODO : VolumeDatabase shouldnt hardcode table fields or table names
			VolumeItem[] items = QueryItems<VolumeItem>(
											string.Format("SELECT * FROM Items WHERE VolumeID = {0} AND ItemID = {1}", volumeID, itemID),
											-1);

			return items.Length == 0 ? null : items[0];
		}
		
		public int SearchItemResultsLimit {
			get { return searchItemResultsLimit; }
			set { searchItemResultsLimit = value; }
		}
		
		public IAsyncResult BeginSearchItem(ISearchCriteria searchCriteria, AsyncCallback callback, object state) {
		
			if (searchCriteria == null)
				throw new ArgumentNullException("searchCriteria");

			//if (callback == null)
			//	  throw new ArgumentNullException("callback");
			
			EnsureOpen();
			
			BeginSearchDelegate<VolumeItem> d = new BeginSearchDelegate<VolumeItem>(_SearchItem);
			return d.BeginInvoke(searchCriteria, callback, state);
		}
		
		public VolumeItem[] EndSearchItem(IAsyncResult asyncResult) {
			//EnsureOpen(); // DON'T!
			if (asyncResult == null)
				throw new ArgumentNullException("asyncResult");

			BeginSearchDelegate<VolumeItem> d = (BeginSearchDelegate<VolumeItem>) ((AsyncResult)asyncResult).AsyncDelegate;
			return d.EndInvoke(asyncResult);
		}
		
		public VolumeItem[] SearchItem(ISearchCriteria searchCriteria) {
		
			if (searchCriteria == null)
				throw new ArgumentNullException("searchCriteria");

			EnsureOpen();

			return _SearchItem(searchCriteria);
		}
		
		private VolumeItem[] _SearchItem(ISearchCriteria searchCriteria) {
			// TODO : check SQL output! implemetation was rewritten from scratch!!
			// TODO : VolumeDatabase shouln't hardcode fieldnames or tablenames
			//const string ORDER_FIELD = "Items.VolumeID"; // slows down searching drastically 

			/*** build sql query ***/
			string condition = searchCriteria.GetSqlSearchCondition();

			if (condition.Length == 0) // e.g. empty SearchCriteriaGroup
			   throw new ArgumentException("SearchCriteria is empty", "searchCriteria");
			
			string sqlQuery;
			if ((searchCriteria.SearchCriteriaType & SearchCriteriaType.VolumeSearchCriteria) == SearchCriteriaType.VolumeSearchCriteria) {
				// searchriteria contains volume searchriteria -> join volumes table
				sqlQuery = string.Format("SELECT Items.* FROM Items, Volumes WHERE ({0}) AND (Items.VolumeID = Volumes.VolumeID);", condition);
			} else {
				//sqlQuery = string.Format("SELECT * FROM Items WHERE {0} ORDER BY {1};", condition, ORDER_FIELD);
				sqlQuery = string.Format("SELECT * FROM Items WHERE {0};", condition);
			}
			
			Debug.WriteLine(string.Format("_SearchItem() executes query: '{0}'", sqlQuery));
			return QueryItems<VolumeItem>(sqlQuery, searchItemResultsLimit);
		}
		
		// used by Volume.GetRoot() and specific implementations of Volume.GetRoot()
		internal TRootItem GetVolumeRoot<TRootItem>(long volumeID)
			where TRootItem : IContainerItem
		{
			EnsureOpen();
			// TODO : VolumeDatabase shouldnt hardcode table fields or table names
			TRootItem[] items = QueryItems<TRootItem>(
										string.Format("SELECT * FROM Items WHERE (VolumeID = {0}) AND (ParentID = {1})", volumeID, ID_NONE),
										-1);
			
			return items.Length == 0 ? default(TRootItem) : items[0];
		}
		
		// used by IContainerItem.GetContainers() and specific implementations of IContainerItem.GetContainers()
		internal TContainerItem[] GetChildContainerItems<TContainerItem>(long volumeID, long itemID)
			where TContainerItem : IContainerItem
		{
			EnsureOpen();
			// TODO : VolumeDatabase shouldnt hardcode table fields or table names
			return QueryItems<TContainerItem>(
							string.Format("SELECT * FROM Items WHERE (VolumeID = {0}) AND (ParentID = {1}) AND (IsContainer = 1)", volumeID, itemID),
							-1);
		}
		
		// used by IContainerItem.GetItems() and specific implementations of IContainerItem.GetItems()
		internal TChildItem[] GetChildItems<TChildItem>(long volumeID, long itemID)
			where TChildItem : IChildItem
		{
			EnsureOpen();
			// TODO : VolumeDatabase shouldnt hardcode table fields or table names
			return QueryItems<TChildItem>(
							string.Format("SELECT * FROM Items WHERE (VolumeID = {0}) AND (ParentID = {1}) AND (IsContainer = 0)", volumeID, itemID),
							-1);
		}
		
		internal long GetNextVolumeID() {
			EnsureOpen();
			sql.EnterConnectionLock();
			try {
				// volumeID must be unique per database, so a id counter table is used
				sql.ExecuteNonQuery("UPDATE IdCounters SET Count = Count + 1 WHERE IdFieldname = 'Volumes.VolumeID'");
				long nextID = (long)sql.ExecuteScalar("SELECT Count FROM IdCounters WHERE IdFieldname = 'Volumes.VolumeID'");
				return nextID;
			} finally {
				sql.ExitConnectionLock();
			}
		}

		private void CreateTables() {
			List<string> commands = new List<string>();			   
			
			//
			// create tables
			// * remarks regarding fields:
			//	 some fields (e.g. Files, Dirs, Size, Location, LastWriteTime, SymLinkTargetID, ...) can be NULL
			//	 because they're not used by Non-Filesystem-Volumes (e.g. Audio CDs).
			//	 most of the others fields with the NULL keyword attached are optional.
			// * remarks regarding primary keys:
			//	 PRIMARY KEY (VolumeID, ItemID) speeds up VolumeItem::GetParent(), 
			//	 VolumeDatabase::GetVolumeItem(), removing items by volumeID (and probably resolving symlink targets)
			string sqlCreate = string.Format(
			@"
			CREATE TABLE DatabaseProperties (
				Name		VARCHAR({0}),
				Description	TEXT,	
				Created		DATE			NOT NULL,
				Version		INTEGER			NOT NULL,
				GUID		VARCHAR(36)		NOT NULL
			);

			CREATE TABLE Volumes (
				VolumeID	INTEGER			PRIMARY KEY,
				Title		VARCHAR({1}),
				Added		DATE,
				VolumeType	INTEGER			NOT NULL,
				IsHashed	BOOLEAN			NOT NULL DEFAULT 0,
				ArchiveNr	VARCHAR({2}),
				DriveType	INTEGER			NOT NULL,
				Loaned_To	VARCHAR({3}),
				Loaned_Date	DATE,
				Return_Date	DATE,
				Category	VARCHAR({4}),
				Description	TEXT,
				Keywords	TEXT,

				Files		INTEGER,
				Dirs		INTEGER,
				Size		INTEGER
			);

			CREATE TABLE Items (
				VolumeID		INTEGER,
				ItemID			INTEGER,
				ParentID		INTEGER			NOT NULL,
				ItemType		INTEGER			NOT NULL,
				Name			VARCHAR({5})	NOT NULL,
				MimeType		VARCHAR({6}),
				MetaData		TEXT,
				Note			TEXT,
				Keywords		TEXT,

				Hash			VARCHAR({7}),
				IsContainer		BOOLEAN			NOT NULL DEFAULT 0,


				Location		VARCHAR({8}),
				LastWriteTime	DATE,
				SymLinkTargetID	INTEGER,

				Size			INTEGER,

				PRIMARY KEY (VolumeID, ItemID)
			);

			CREATE TABLE IdCounters (
				IdFieldname	VARCHAR(64)	PRIMARY KEY,
				Count		INTEGER		NOT NULL
			)
			",

			DatabaseProperties.MAX_NAME_LENGTH,

			Volume.MAX_TITLE_LENGTH,
			Volume.MAX_ARCHIVE_NR_LENGTH,
			Volume.MAX_LOANED_TO_LENGTH,
			Volume.MAX_CATEGORY_LENGTH,

			VolumeItem.MAX_NAME_LENGTH,
			VolumeItem.MAX_MIMETYPE_LENGTH,
			VolumeItem.MAX_HASH_LENGTH,

			FileVolumeItem.MAX_LOCATION_LENGTH
			);
			
			commands.Add(sqlCreate);
			
			//
			// additional indices are added here:
			//
			
			// index that (together with the VolumeID index part of the primary key)
			// speeds up all GetChild*() functions (majority of all queries used in Basenji) 
			// and Volume::GetRoot()
			commands.Add("CREATE INDEX IDX_Items_ParentID ON Items (ParentID)");
			
			// improves searching performance (EDIT: no, it does not!?)
			//commands.Add("CREATE INDEX IDX_Items_Name ON Items (Name)");
			
			// TODO : create furhter indices for fields frequently involved in full text searches here

			// 
			// initialize tables
			//
			commands.Add(string.Format(
			@"
				INSERT INTO DatabaseProperties 
				(Name, Description, Created, Version, GUID) 
				Values('', '', '{0}', {1}, '{2}');
			"
			, DateTime.Now.ToString(SQL_DATETIME_FORMAT), DB_VERSION, Guid.NewGuid().ToString())
			);
			 
			commands.Add(string.Format("INSERT INTO IdCounters (IdFieldName, Count) VALUES('Volumes.VolumeID', {0})", (ID_FIRST - 1)));
			//commands.Add("INSERT INTO IdCounters (IdFieldName, Count) VALUES('Items.ItemID', 0)");

			sql.ExecuteNonQuery(commands.ToArray());
		}
		
		private void EnsureOpen() {
			if (disposed)
				throw new ObjectDisposedException("VolumeDatabase", "This VolumeDatabase has been closed");
		}
		
		private void WriteRecord(IVolumeDBRecord record, bool update) {
			WriteRecords(new IVolumeDBRecord[] { record }, update);
		}

		private void WriteRecords(IVolumeDBRecord[] records, bool update) {
			
			List<string>	sqlCommands = new List<string>();
			StringBuilder	sqlcmd		= new StringBuilder();
			
			int changed = 0;
			
			try {
			
				if (update) {

					foreach (IVolumeDBRecord record in records) {
						if (record.IsNew)
							throw new InvalidOperationException("Database record can not be updated because it has not been inserted yet");
					
						// reset stringbuilder
						sqlcmd.Length = 0;
						
						sqlcmd.Append("UPDATE ").Append(record.TableName).Append(" SET ");
						
						int n = 0;
						IRecordData recData = record.GetRecordData();
						foreach (FieldnameValuePair pair in recData) {
							if (n > 0)
								sqlcmd.Append(", ");
							string val = SqlPrepareValue(pair.Value, true);
							sqlcmd.Append(pair.Fieldname).Append(" = ").Append(val);
							n++;
						}
						
						string[] primarykeyFields = record.PrimaryKeyFields;
						if ((primarykeyFields != null) && (primarykeyFields.Length > 0)) { // single-record-tables (e.g. table DatabaseProperties) may not have a primary key
							sqlcmd.Append(" WHERE ");							 
							for (int i = 0; i < primarykeyFields.Length; i++) {
								if (i > 0)
									sqlcmd.Append(" AND ");
								string pk = primarykeyFields[i];
								sqlcmd.Append(pk).Append(" = ").Append(recData[pk]);
							}							 
						}

//						  string idField = record.TableIDField;
//						  if (!string.IsNullOrEmpty(idField)) // single-record-tables (e.g. table DatabaseProperties) may not have a primary key
//							  sqlcmd.Append(" WHERE ").Append(idField).Append(" = ").Append(recData[idField]);
						
						sqlCommands.Add(sqlcmd.ToString());
					}
					
			
				} else { // insert
					
					StringBuilder fields = new StringBuilder();
					StringBuilder values = new StringBuilder();
			
					foreach (IVolumeDBRecord record in records) {
						if (!record.IsNew)
							throw new InvalidOperationException("Database record has already been inserted");
					
						// reset stringbuilders
						sqlcmd.Length	   = 0;
						fields.Length	= 0;
						values.Length	= 0;
						
						sqlcmd.Append("INSERT INTO ").Append(record.TableName).Append(' ');
						
						int n = 0;
						IRecordData recData = record.GetRecordData();
						foreach (FieldnameValuePair pair in recData) {
							if (n > 0) {
								fields.Append(", ");
								values.Append(", ");
							}
							string val = SqlPrepareValue(pair.Value, true);
							fields.Append(pair.Fieldname);
							values.Append(val);
							n++;
						}
						
						sqlcmd.Append('(').Append(fields.ToString()).Append(") VALUES (").Append(values.ToString()).Append(')');
						record.IsNew = false; // mark the record object as not-new
						changed++; 
						
						sqlCommands.Add(sqlcmd.ToString());
					}

				}

				if (sqlCommands.Count > 1)
					sql.ExecuteNonQuery(sqlCommands.ToArray());
				else
					sql.ExecuteNonQuery(sqlCommands[0]);
			
			} catch(Exception) {
				// undo changes
				if (!update) {
					for (int i = 0; i < changed; records[i++].IsNew = true);
				}
				throw;
			}
						
		}

		private static string SqlPrepareValue(object value, bool replaceEmptyByNull) { return SqlPrepareValue(value, replaceEmptyByNull, null); }
		private static string SqlPrepareValue(object value, bool replaceEmptyByNull, string formatStringValue) {

			// TODO : or simply put _all_ values in apostrophes, so i dont have to care about the type?
			//		  would that be valid standard-sql?
			//		  Do querys still work (e.g. "WHERE fieldname = 1" must be the same as "WHERE fieldname = '1'") ?
			
			string retVal = null;

			if (formatStringValue == null || formatStringValue.Length == 0)
				formatStringValue = "'{0}'";

			if (value == null) {
			
				retVal = "NULL";
			
			} else if (value is string || value is char) {
			
				string s = value.ToString(); // cast to string would be bad here -- type of value could be char
				retVal = (replaceEmptyByNull && s.Length == 0) ? "NULL" : string.Format(formatStringValue, s.Replace("'","''"));
			
			} else if (value is DateTime) {
			
				// DateTime.MinValue is interpreted as "empty"
				DateTime d = (DateTime)value;
				if (replaceEmptyByNull && d.Ticks == 0L)
					retVal = "NULL";
				else
					retVal = string.Format(formatStringValue, ((DateTime)value).ToString(SQL_DATETIME_FORMAT));
			
			} else if (value is Enum) {
			
				// TODO : is there a better way to convert enum types to string?
				retVal = ((Enum)value).ToString("D");
			
			} else if (value is bool) {
				
				retVal = ((bool)value) ? "1" : "0";
			
			} else {
			
				// numeric types like int, long, double, byte etc
				// and unknown types (unknown types that return an alphanumeric value won't work though)
				retVal = value.ToString();
			}

			return retVal;
		}

		private Volume[] QueryVolumes(string sqlQuery) {
			
			List<Volume> list = new List<Volume>();

			sql.ExecuteReader(sqlQuery, delegate(IDataReader reader, IRecordData readerRecData) {
				while(reader.Read()) {
					// TODO : if possible, don't hardcode 'reader["VolumeType"]' here, 
					// use something like reader[AnInterface.TypeField]. VolumeDatabase shouldnt include table-/fieldnames if possible. 
					// the classes representing the data should hold the names.
					Volume volume = Volume.CreateInstance((VolumeType)(int)(long)reader["VolumeType"], this);
					
					IVolumeDBRecord record = volume;
					record.SetRecordData(readerRecData);
					record.IsNew = false;
					
					list.Add(volume);
				}
			});
			
			return list.ToArray();
		}
		
		private TItem[] QueryItems<TItem>(string sqlQuery, int limit)
			where TItem : IChildItem // IChildItem is the least common denominator of all volumeitems and related interfaces (e.g. DirectoryVolumeItem, FileVolumeItem, ..., IContainerItem, IChildItem) 
		{
		
			List<TItem> list = new List<TItem>();
			int n = 0;
			
			sql.ExecuteReader(sqlQuery, delegate(IDataReader reader, IRecordData readerRecData) {
				while(reader.Read()) {
					if ((limit > -1) && (++n > limit)) {
						list.Clear();
						throw new TooManyResultsException(
							string.Format("The result limit of {0} items has been reached", limit));
					}
					
					// TODO : if possible, don't hardcode 'reader["ItemType"]' here,
					// use something like reader[AnInterface.TypeField]. VolumeDatabase shouldnt include table-/fieldnames if possible. 
					// the classes representing the data should hold the names.
					// TODO : why do i have to cast to (IChildItem) first?
					TItem item = (TItem)(IChildItem)VolumeItem.CreateInstance((VolumeItemType)(int)(long)reader["ItemType"], this);

					IVolumeDBRecord record = (IVolumeDBRecord)item;
					record.SetRecordData(readerRecData);
					record.IsNew = false;
						
					list.Add(item);
				}
			});
			
			return list.ToArray();
		}
		
		///<summary>
		///Begins a transaction on the current thread. 
		///Other threads are blocked from accessing the database until this thread calls TransactionCommit() or TransactionRollback().
		///Callers of this method must make sure that TransactionRollback() is called whenever an exception occurs in succeeding mehtod calls on the same VolumeDatabase instance.
		///Ignoring exceptions of VolumeDatabase methods during an active transaction can lead to loss of data.
		///</summary>
		public void TransactionBegin() {
			EnsureOpen();			 
			sql.TransactionBegin();
		}
		
		///<summary>
		///Commits the transaction of the current thread.
		///</summary>
		public void TransactionCommit() {
			EnsureOpen();
			sql.TransactionCommit();
		}
		
		///<summary>
		///Rolls back the transaction of the current thread.
		///</summary>
		public void TransactionRollback() {
			EnsureOpen();
			sql.TransactionRollback();
		}
		
		#region IDisposable Members

		void IDisposable.Dispose() {
			Dispose(true);
		}

		#endregion

		private void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					if (!sql.IsClosed)
						sql.Close();					
				}
				sql = null;
			}
			disposed = true;
		}

		private delegate T[] BeginSearchDelegate<T>(ISearchCriteria searchCriteria);
	}
}