// VolumeDatabase.SqlBackend.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Platform.Common.DB;

namespace VolumeDB
{
	public sealed partial class VolumeDatabase : IDisposable
	{
		private class SqlBackend : IDisposable
		{			 
			// Time in ms a thread should try to acquire the lock to the database connection 
			// before throwing a timeout exception.
			// Keep in mind that e.g. search methods may take a few seconds 
			// and that other threads may already be waiting in the ready queue.
			private const int		CONN_LOCK_TIMEOUT = 10 * 1000;
			
			private bool			disposed;

			private IDbConnection	conn;

			private object			_conn_lock;						// used by Monitor.TryEnter() to lock a VolumeDatabase instance (e.g. sqlite is singlethreaded)
			
			private IDbTransaction	transaction;					// current transaction
			private Thread			currentTransactionThread;		// thread that is currently performing a transaction
			
			private VolumeDatabase	ownerDb;						// reference to the owner volumedatabe object

			public SqlBackend(string dbPath, bool create, VolumeDatabase ownerDb) {
				_conn_lock = new Object();
				
				conn = SqliteDB.Open(dbPath, create);
				
				disposed					= false;

				transaction					= null;
				currentTransactionThread	= null;
				
				this.ownerDb				= ownerDb;
			}
			
			// executes a single sql query.
			public void ExecuteNonQuery(string sqlCommand) {
			//ExecuteNonQuery(new string[] { sqlCommand });
				EnsureOpen();
				IDbCommand cmd = null;
				EnterConnectionLock();
				
				try {
					try {
						ownerDb.OnBeginWriteAccess(new EventArgs());
						cmd = conn.CreateCommand();
						cmd.Transaction = transaction;
						cmd.CommandText = sqlCommand;
						cmd.ExecuteNonQuery();
					} finally {
						if (cmd != null)
							cmd.Dispose();
						ownerDb.OnEndWriteAccess(new EventArgs());
					}
				} finally {
					ExitConnectionLock();
				}
			}
			
			// executes multiple sql queries 
			// encapsulated in a new transaction if there isn't an open transaction already.
			public void ExecuteNonQuery(string[] sqlCommandBatch) {
				EnsureOpen();				 
				/*
				IDbCommand cmd = null; // TODO : command auf klassenebene? NEIN! Ist jetzt multithreadsave!
				EnterConnectionLock();
				try {
					try {
						OnBeginWriteAccess(new EventArgs());
						cmd = conn.CreateCommand();
						cmd.Transaction = transaction;

						for (int i = 0; i < sqlCommandBatch.Length; i++) {
							cmd.CommandText = sqlCommandBatch[i];
							cmd.ExecuteNonQuery();
						}
					} finally {
						if (cmd != null)
							cmd.Dispose();
						OnEndWriteAccess(new EventArgs());
					}
				} finally {
					ExitConnectionLock();
				}
				*/
				
				IDbCommand cmd = null;
				bool localTransaction = false;
				
				if (!CurrentThreadHasStartedTransaction) {
					TransactionBegin(); // calls EnterConnectionLock()
					localTransaction = true;
				}
				
				try {
					ownerDb.OnBeginWriteAccess(new EventArgs());
					cmd = conn.CreateCommand();
					cmd.Transaction = transaction;
					
					for (int i = 0; i < sqlCommandBatch.Length; i++) {
						cmd.CommandText = sqlCommandBatch[i];
						cmd.ExecuteNonQuery();
					}
				
					if (localTransaction)
						TransactionCommit(); // calls ExitConnectionLock()
				
				} catch(Exception) {
				
					if (localTransaction)
						TransactionRollback(); // calls ExitConnectionLock()
					throw;
				
				} finally {
				
					if (cmd != null)
						cmd.Dispose();
					ownerDb.OnEndWriteAccess(new EventArgs());
				}
				
			}
			
			public object ExecuteScalar(string sqlCommand) {
				EnsureOpen();
				EnterConnectionLock();
				try {
					//try {  // TODO: in case OnBeginReadAccess() will be uncommented: remove using block and use this try block (as in ExecuteNonQuery(string))
					//OnBeginReadAccess(new EventArgs());
					using (IDbCommand cmd = conn.CreateCommand()) {
						cmd.CommandText = sqlCommand;
						cmd.Transaction = transaction;
						return cmd.ExecuteScalar();
					}
					//} finally {
					//	  OnEndReadAccess(new EventArgs());
					//}
				} finally {
					ExitConnectionLock();
				}
			}
			
			public delegate void ExecuteReaderCallback(IDataReader reader, IRecordData readerRecData);
			public void ExecuteReader(string sqlCommand, ExecuteReaderCallback callback) {
				EnsureOpen();				 
				EnterConnectionLock();
				try {
					IDbCommand	cmd		= null;
					IDataReader reader	= null;
					try {
						// OnBeginReadAccess(new EventArgs());
						cmd = conn.CreateCommand();
						cmd.CommandText = sqlCommand;
						cmd.Transaction = transaction;
						
						reader = cmd.ExecuteReader();
						IRecordData readerRecData = new RecordData_DataReader_Wrapper(reader);
						
						callback(reader, readerRecData);
					} finally {
						if (reader != null)
							reader.Dispose();
						if (cmd != null)
							cmd.Dispose();
						// OnEndReadAccess(new EventArgs());
					}
				} finally {
					ExitConnectionLock();
				}
			}
			
			/*
			* transactions
			*/
			
			///<summary>
			///Begins a transaction on the current thread. 
			///Other threads are blocked from accessing the database until this thread calls TransactionCommit() or TransactionRollback().
			///Callers of this method must make sure that TransactionRollback() is called whenever an exception occurs in succeeding mehtod calls on the same SqlBackend instance.
			///Ignoring exceptions of SqlBackend methods during an active transaction can lead to loss of/inconsistent data.
			///</summary>
			public void TransactionBegin() {
				EnsureOpen();
				
				if (CurrentThreadHasStartedTransaction) // nested transactions are not supported by most database systems
					throw new InvalidOperationException("The current thread has already started a transaction");

				// other threads cannot write to conn during an active transaction (their writes will have no effect (in case of a sqliteconnection, without notice!))
				// so lock them out during a transaction.
				EnterConnectionLock();
				
				try {
					transaction					= conn.BeginTransaction();
					currentTransactionThread	= Thread.CurrentThread;
				} catch(Exception) {
					transaction					= null;
					currentTransactionThread	= null;
					ExitConnectionLock();
					throw;
				}
			}
			
			///<summary>
			///Commits the transaction of the current thread.
			///</summary>
			public void TransactionCommit() {
				EnsureOpen();

				if (!CurrentThreadHasStartedTransaction)
					throw new InvalidOperationException("The current thread has not started a transaction");
					
				try {
					transaction.Commit();
				} finally {
					transaction					= null;
					currentTransactionThread	= null;
					ExitConnectionLock();
				}
			}
			
			///<summary>
			///Rolls back the transaction of the current thread.
			///</summary>
			public void TransactionRollback() {
				EnsureOpen();
		 
				if (!CurrentThreadHasStartedTransaction)
					throw new InvalidOperationException("The current thread has not started a transaction");
					
				try {
					transaction.Rollback();
				} finally {
					transaction					= null;
					currentTransactionThread	= null;
					ExitConnectionLock();
				}
			}
		   
			private bool CurrentThreadHasStartedTransaction {
				get { return Thread.CurrentThread == currentTransactionThread; }
			}
			
			/*
			 * mutlithreading support functions
			 */
			public void EnterConnectionLock() {
				EnsureOpen();
				if (!Monitor.TryEnter(_conn_lock, CONN_LOCK_TIMEOUT))
					throw new TimeoutException("Another thread is busy accessing the database");
			}
			
			public void ExitConnectionLock() {
				EnsureOpen();
				Monitor.Exit(_conn_lock);
			}

			public void Close() {
				Dispose(true);
			}
			
			public bool IsClosed { get { return disposed; } }

			#region IDisposable Members

			void IDisposable.Dispose() {
				Dispose(true);
			}

			#endregion

			private void Dispose(bool disposing) {
				if (!disposed) {
					if (disposing) {
						if (conn.State == ConnectionState.Open)
							conn.Close();
					}
					conn						= null;
					
					transaction					= null;
					currentTransactionThread	= null;
					_conn_lock					= null;
				}
				disposed = true;
			}
				
			private void EnsureOpen() {
				if (disposed)
					throw new ObjectDisposedException("SqlConn");
			}
		
			#region nested class RecordData_DataReader_Wrapper
			private class RecordData_DataReader_Wrapper : IRecordData
			{
				private IDataReader reader;
				private string		fieldNamePrefix;

				public RecordData_DataReader_Wrapper(IDataReader reader) : this(reader, null) { }
				public RecordData_DataReader_Wrapper(IDataReader reader, string fieldNamePrefix) {
					this.reader				= reader;
					this.fieldNamePrefix	= fieldNamePrefix;
				}

				public IDataReader Reader {
					get { return reader; }
					set { reader = value; }
				}

				public string FieldNamePrefix {
					get { return fieldNamePrefix ?? string.Empty; }
					set { fieldNamePrefix = value; }
				}

				#region IRecordData Members

				public object this[string fieldName] {
					get {
						if (fieldNamePrefix == null || fieldNamePrefix.Length == 0)
							return reader[fieldName];

						return reader[fieldNamePrefix + fieldName];
					}
				}

				public object GetValue(string fieldName) {
					if (fieldNamePrefix == null || fieldNamePrefix.Length == 0)
						return reader[fieldName];

					return reader[fieldNamePrefix + fieldName];
				}

				public void AddField(string fieldName, object value) {
					throw new NotSupportedException();
				}

				#endregion

				#region IEnumerable<FieldnameValuePair> Members

				IEnumerator<FieldnameValuePair> IEnumerable<FieldnameValuePair>.GetEnumerator() {
					for (int i = 0; i < reader.FieldCount; i++) {
						string fieldName	= reader.GetName(i);
						object value		= reader.GetValue(i);

						if (fieldNamePrefix != null && fieldNamePrefix.Length > 0)
							fieldName = fieldName.Substring(fieldNamePrefix.Length);

						FieldnameValuePair pair = new FieldnameValuePair(fieldName, value);
						yield return pair;
					}
				}

				#endregion

				#region IEnumerable Members

				IEnumerator IEnumerable.GetEnumerator() {
					return ((IEnumerable<FieldnameValuePair>)this).GetEnumerator();
				}

				#endregion
			}
			#endregion
	
		}
	}
}