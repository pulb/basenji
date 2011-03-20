// AbstractVolumeScanner.cs
// 
// Copyright (C) 2008 - 2011 Patrick Ulbrich
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
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using VolumeDB.Metadata;
using Platform.Common.Diagnostics;
using PlatformIO = Platform.Common.IO;

namespace VolumeDB.VolumeScanner
{	 
	public abstract class AbstractVolumeScanner<TVolume, TVolumeInfo, TOpts> : IVolumeScanner
		where TVolume		: Volume
		where TVolumeInfo	: VolumeInfo
		where TOpts			: ScannerOptions, new()
	{
 
		private PlatformIO.DriveInfo	drive;
		private VolumeDatabase			database;
		
		private long					itemID; // item id counter
		//private VolumeDatabase.IdCounter	  itemIdCounter;
		
		private TVolume					volume; // TODO : media is defined on memberlevel now (not passed from outside anymore).. dispose() anything?
		private TVolumeInfo				volumeInfo; // basic readonly info about the volume being scanned
		private TOpts					options;
		
		private volatile bool			isRunning;
		private volatile bool			cancellationRequested;
		private AsyncOperation			asyncOperation;
		
		private volatile bool			scanSucceeded;
		
		private bool					disposed;
		
		// note:
		// do not allow to modify the constuctor parameters 
		// (i.e. database, options)
		// through public properties later, since the scanner 
		// may already use them after scanning has been started,
		// and some stuff has been initialized depending on the 
		// options in the ctor already.
		internal AbstractVolumeScanner(PlatformIO.DriveInfo drive,
		                       VolumeDatabase database,
		                       TOpts options) {
			
			if (drive == null)
				throw new ArgumentNullException("drive");
			
			if (!drive.IsReady)
				throw new ArgumentException("Drive is not ready", "drive");
			
			if (options == null)
				throw new ArgumentNullException("options");
			
			/* don't test database for null -- database is optional */

			if ((options.BufferSize < 1) && (database != null))
				throw new ArgumentOutOfRangeException("BufferSize");
			
			this.isRunning		= false;
			//m_cancellationRequested = false;

			this.scanSucceeded	= false;
			this.disposed		= false;

			this.drive			= drive;
			this.database		= database;
			
			// copy options reference so that they can't be modified 
			// while the scanner is running already.
			this.options		= new TOpts();
			options.CopyTo(this.options);
			
			this.itemID = VolumeDatabase.ID_NONE;
			this.volume			= CreateVolumeObject(drive, database, options.ComputeHashs);
			this.volumeInfo		= CreateInstance<TVolumeInfo>(volume); 
		}
		
		#region IVolumeScanner Members

		public WaitHandle RunAsync() {
			if (isRunning)
				throw new InvalidOperationException("Scanner is already running");

			if (scanSucceeded)
				throw new InvalidOperationException("Scanning has been completed successfully. Create a new scanner to scan another volume");

			try {
				/* must be set (as soon as possible) in a function that is _not_ called asynchronously 
				 * (i.e. dont call it in ScanningThread()) */
				isRunning = true;
				cancellationRequested = false;
				asyncOperation = AsyncOperationManager.CreateOperation(null);

				Reset();

				BufferedVolumeItemWriter writer = null;
				if (this.HasDB)
					writer = new BufferedVolumeItemWriter(database, true, Options.BufferSize);

				/* invoke the scanning function on a new thread and return a waithandle */
				Action<PlatformIO.DriveInfo, TVolume, BufferedVolumeItemWriter> st = ScanningThread;
				IAsyncResult ar = st.BeginInvoke(drive, volume, writer, null, null);
				return ar.AsyncWaitHandle;
			} catch (Exception) {
				isRunning = false;

				if (asyncOperation != null)
					asyncOperation.OperationCompleted();

				throw;
			}
		}

		public void CancelAsync() {
			cancellationRequested = true;
		}

		public bool IsBusy {
			get { return isRunning; }
		}

		public bool ScanSucceeded {
			get { return scanSucceeded; }
		}

		//Media IMediaScanner.Media
		//{
		//	  get { return m_media; }
		//}

		/*
		 * implemented explicitely since AbstractVolumeScanner
		 * also implements a scanner specific VolumeInfo property.
		 */ 
		VolumeInfo IVolumeScanner.VolumeInfo {
			get { return volumeInfo; }
		}
		
		public event BeforeScanItemEventHandler BeforeScanItem;
		public event ScannerWarningEventHandler ScannerWarning;
		public event ErrorEventHandler			Error;
		public event ScanCompletedEventHandler	ScanCompleted;
		
		#endregion
		
		public TVolumeInfo VolumeInfo {
			get { return volumeInfo; }
		}
		
		protected void CheckForCancellationRequest() {
			if (cancellationRequested)
				throw new ScanCancelledException();
		}
		
		#region IDisposable Members

		public void Dispose() {
			Dispose(true);
		}

		#endregion

		protected virtual void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					if (this.IsBusy)
						throw new InvalidOperationException("Scan in progress");

					/*
					if (m_db != null)
						m_db.Close(); // TODO ? leavOpen in Scanworker.ctor after database parameter? PASS THIS leavOpen PARAMETER AT THE BufferedItemWriter INSTANCIATION AS WELL!!!
					m_media.Dispose(); // TODO ? (in case m_media implements IDisposable because of the MediaDB reference anytime later)
					*/
				}
				
				drive			= null;
				database		= null;
				volume			= null;
				volumeInfo		= null;
				options			= null;
				asyncOperation	= null;

				disposed  = true;
			}
		}

		protected virtual void Reset() {
			volumeInfo.Reset();
					
			if (this.HasDB) {
				//// TODO : this is neither threadsave nor multi-instance save in general! maybe the db (physical db, MediaDB object? this scanner?) should be locked during scanning? USE MONITOR thread locker? INTERLOCKED class?
				//itemID = database.GetNextItemID();
				itemID = VolumeDatabase.ID_FIRST;
			}

			// TODO :
			// reset values of Volumebase like CreatedDate here? 
			// (or not? at last it has been created by the first try)
		}

		protected bool HasDB {
			get { return (database != null); }
		}
		
		protected VolumeDatabase Database {
			get {
				if (database == null)
					throw new InvalidOperationException("No database associated");
				return database;
			}
		}
		
		protected TOpts Options {
			get { return options; }
		}
		
		// TODO : remove parameter itemType if generic constraints allow internal and parameterized constructors.
		// Parameter itemType is used as a workaround to instanciate a VolumeItem object, 
		// because the internal VolumeItem constructor with the database parameter can't be used in generic code.
		// see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=9a8e58ee-1371-4e99-8385-c3e2a4157fd6
		// see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=80517ec1-2d08-43cc-bc90-9927877061a9
		
		/// <summary>
		/// Returns a specific VolumeItem object, but preassigns properties of the VolumeItem baseclass only.
		/// Filling properties of the specific, derived object, is job of the specific VolumeScanner implementation.
		/// </summary>
		/// <typeparam name="TVolumeItem">Type of the specific volume item.</typeparam>
		/// <returns>
		/// A new specific VolumeItem derived from base class VolumeItem 
		/// with all base class properties preassigned.
		/// </returns>
		protected TVolumeItem GetNewVolumeItem<TVolumeItem>(long parentID,
		                                                    string name,
		                                                    string mimeType,
		                                                    MetadataStore metaData,
		                                                    VolumeItemType itemType)
			where TVolumeItem : VolumeItem
		{
			// TODO: check here if TMediaItem applies to TMedia?
			
			/* TVolumeItem item = new TVolumeItem(database); */
			TVolumeItem item = (TVolumeItem)VolumeItem.CreateInstance(itemType, database);

			// initialize fields of the VolumeItem base class.
			// don't initialize via properties. initializing via properties is error-prone
			// as the compiler won't error if a new field is added to the base class
			// and forgotten to be initialized here.
			item.SetVolumeItemFields(volume.VolumeID,
			                         itemID,
			                         parentID,
			                         name,
			                         mimeType,
			                         metaData,
			                         null,
			                         null);

			itemID++;

			return item;
		}
		
		/// <summary>
		// Returns a specific Volume object, but preassigns properties of the Volume baseclass only.
		// Filling properties of the specific, derived object, is job of the specific VolumeScanner implementation.
		/// </summary>		  
		private static TVolume CreateVolumeObject(PlatformIO.DriveInfo d,
		                                          VolumeDatabase database,
		                                          bool isHashed) {
			// TODO : check here whether everything is still filled correctly after media class reorganisation

			//Util.DriveInfo di = new Util.DriveInfo(rootDir);
			VolumeDriveType driveType;

			switch (d.DriveType) {
				case PlatformIO.DriveType.CDRom:
					driveType = VolumeDriveType.CDRom;
					break;
				case PlatformIO.DriveType.Fixed:
					driveType = VolumeDriveType.Harddisk;
					break;
				case PlatformIO.DriveType.Ram:
					driveType = VolumeDriveType.Ram;
					break;
				case PlatformIO.DriveType.Network:
					driveType = VolumeDriveType.Network;
					break;
				case PlatformIO.DriveType.Removable:
					driveType = VolumeDriveType.Removable;
					break;
				case PlatformIO.DriveType.Unknown:
					driveType = VolumeDriveType.Unknown;
					break;
				default:
					throw new Exception("Invalid DriveType");
			}
			
			long volumeID = VolumeDatabase.ID_NONE;
			if (database != null) {
				// TODO : this is neither threadsave nor multi-instance save in general! maybe the db (physical db, VolumeDatabase object? this scanner?) should be locked during scanning?
				volumeID = database.GetNextVolumeID();
			}
			
			TVolume v = CreateInstance<TVolume>(database);
//			  /* v = new TVolume(database); */
//			  v = (TVolume)VolumeDB.Volume.CreateInstance(volumeType, database);
//
			// initialize fields of the Volume base class.
			// don't initialize via properties. initializing via properties is error-prone
			// as the compiler won't error if a new field is added to the base class
			// and forgotten to be initialized here.
			v.SetVolumeFields(
				volumeID,
				d.IsMounted ? d.VolumeLabel : string.Empty,
				DateTime.Now,
				/*di.VolumeSerialNumber,*/
				isHashed,
				volumeID.ToString(),
				driveType,
				null,
				DateTime.MinValue,
				DateTime.MinValue,
				null,
				null,
				null
				);

			return v;
		}
		
		/// <summary>
		/// Overriden by VolumeScanner implementations to implement specific scanning logic.
		/// The purpose of this function is to scan a volume 
		/// and populate the passed objects Volume and BufferedVolumeItemWriter with the information acquired.
		/// If an error occurs, ScannintThreadMain() should throw an exception, 
		/// for non-fatal errors it should call SendScannerWarning()
		/// Note: ScanningThreadMain() is running on a new thread.
		/// </summary>
		/// <param name="drive">
		/// driveInfo object of the volume to be scanned.
		/// </param>
		/// <param name="volume">
		/// Specific Volume object that has to be populated with the information acquired. 
		/// Fields of the base class are already preassigned.
		/// This parameter is null if the VolumeScanner is not connected to a VolumeDatabase object.
		/// </param>
		/// <param name="writer">
		/// BufferedVolumeItemWriter that receives VolumeItem objects populated with the information acquired.
		/// This parameter is null if the VolumeScanner is not connected to a VolumeDatabase object.
		/// </param>

		// TODO : make this member internally protected in case this language feature has become real
		// see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=33c53cf6-2709-4cc9-a408-6cafee4313ef
		//protected
		internal
		abstract void ScanningThreadMain(PlatformIO.DriveInfo drive,
			                                 TVolume volume,
			                                 BufferedVolumeItemWriter writer);

		private void ScanningThread(PlatformIO.DriveInfo drive,
		                            TVolume volume,
		                            BufferedVolumeItemWriter writer) {

			TVolume		returnVolume = null;
			Exception	fatalError = null;
			bool		cancelled = false;

			try {

				if (this.HasDB)
					Database.TransactionBegin();  // locks VolumeDatabase

				ScanningThreadMain(drive, volume, writer);
				if (this.HasDB) {
					writer.Close();
					if (!volume.IsInserted)
						volume.InsertIntoDB();

					returnVolume = volume;
					
					database.TransactionCommit(); // unlocks VolumeDatabase
				}
				
				//result = ScanningResult.Success;
				scanSucceeded = true;
			} catch (Exception ex) {

				Exception rollbackException = null;
				try {
					// rollback all database changes
					if (this.HasDB)						   
						database.TransactionRollback(); // unlocks VolumeDatabase
				} catch (Exception e) {
					rollbackException = e;
				}
				
				
				if (ex is ScanCancelledException) {
					//result = ScanningResult.Cancelled;
					cancelled = true;
				} else {
					//result = ScanningResult.FatalError;
					
					/* save the error that caused the scanner to stop (scanning failure) */
					fatalError = ex;
					//OnError(new ErrorEventArgs(ex));
					PostError(ex);
					
					Debug.WriteLine("Details for exception in ScanningThread():\n" + ex.ToString());
				}

				// in case an error occured while rollig back, 
				// post the error here, _after_ the initial error that made the scan fail.
				if (rollbackException != null) {
					//OnError(new ErrorEventArgs(rollbackException));
					PostError(rollbackException);
				}

//#if THROW_EXCEPTIONS_ON_ALL_THREADS
//				  if (!(ex is ScanCancelledException))
//					  throw;
//#endif
			} finally {
				/*
				 * TODO : unlock db / thread // (in try / catch / PostError !!) */

				//if (result == ScanningResult.Success)
				//	  m_scanSucceeded = true;

				//m_cancellationRequested = false;
				isRunning = false;

				//try { OnScanCompleted(new ScanCompletedEventArgs(result, mediaID, fatalError)); }
				//catch (Exception e) { OnError(new ErrorEventArgs(e)); }

				PostCompleted(returnVolume, fatalError, cancelled);
			}
		}
		
		/// <summary>
		/// Called right before the next item is scanned.
		/// </summary>
		protected void PostBeforeScanItem(string itemName) {
			SendOrPostCallback cb = delegate(object args) {
				OnBeforeScanItem((BeforeScanItemEventArgs)args);
			};

			BeforeScanItemEventArgs e = new BeforeScanItemEventArgs(itemName);
			asyncOperation.Post(cb, e);
		}

		/// <summary>
		/// Called if a non-critical error occurs while scanning.
		/// Throws a ScanCancelledException if an eventhandler sets e.CancelScanning to true.
		/// This methods blocks until the called eventhandler returns.
		/// </summary>
		protected void SendScannerWarning(string message, Exception ex) {
			SendOrPostCallback cb = delegate(object args) {
				OnScannerWarning((ScannerWarningEventArgs)args);
			};

			ScannerWarningEventArgs e = new ScannerWarningEventArgs(message, ex);
			asyncOperation.SynchronizationContext.Send(cb, e);

			if (e.CancelScanning)
				throw new ScanCancelledException();
		}
		
		/// <summary>
		/// Called if a non-critical error occurs while scanning.
		/// Throws a ScanCancelledException if an eventhandler sets e.CancelScanning to true.
		/// This methods blocks until the called eventhandler returns.
		/// </summary>
		protected void SendScannerWarning(string message) { SendScannerWarning(message, null); }

		/// <summary>
		/// Called when an unhandled exception occurs.
		/// </summary>
		private void PostError(Exception ex) {
			SendOrPostCallback cb = delegate(object args) {
				OnError((ErrorEventArgs)args);
			};

			ErrorEventArgs e = new ErrorEventArgs(ex);
			asyncOperation.Post(cb, e);
		}
		
		/// <summary>
		/// Called when scanning has been completed.
		/// </summary>
		private void PostCompleted(Volume volume, Exception error, bool cancelled) {
			SendOrPostCallback cb = delegate(object args) {
				OnScanCompleted((ScanCompletedEventArgs)args);
			};

			ScanCompletedEventArgs e = new ScanCompletedEventArgs(volume, error, cancelled);
			asyncOperation.PostOperationCompleted(cb, e);
		}


		#region Events

		private void OnBeforeScanItem(BeforeScanItemEventArgs e) {
			if (this.BeforeScanItem != null)
				this.BeforeScanItem(this, e);
		}
		
		private void OnScannerWarning(ScannerWarningEventArgs e) {
			if (this.ScannerWarning != null)
				this.ScannerWarning(this, e);

			//if (e.Cancel)
			//	  throw new ScanCancelledException();
		}

		private void OnError(ErrorEventArgs e) {
			//try
			//{
				if (this.Error != null)
					this.Error(this, e);
			//}
			//catch (Exception ex)
			//{
			//	  System.Diagnostics.Debug.WriteLine(ex);
			//}
		}

		private void OnScanCompleted(ScanCompletedEventArgs e) {
			if (this.ScanCompleted != null)
				this.ScanCompleted(this, e);
		}

		#endregion

		#region Exceptions

		/// <summary>
		/// Signals that scanning has been cancelled.
		/// </summary>
		protected class ScanCancelledException : Exception {
			public ScanCancelledException() : base() { }
		}
	
		#endregion

		//TODO : remove this method if generic constraints allow internal and parameterized constructors.
		//see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=9a8e58ee-1371-4e99-8385-c3e2a4157fd6
		//see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=80517ec1-2d08-43cc-bc90-9927877061a9
		// don't use this method to create objects frequently, it uses reflection and thus is slow!
		private static T CreateInstance<T>(params object[] args) {
			Type[] argTypes = new Type[args.Length];
			for(int i = 0; i < argTypes.Length; i++)
				argTypes[i] = args[i].GetType();
				
			ConstructorInfo ci = typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
			                                              null,
			                                              argTypes,
			                                              null);
			return (T)ci.Invoke(args);
		}
		
	}	
}
