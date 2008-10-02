// IVolumeScanner.cs
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
using System.Threading;

namespace VolumeDB.VolumeScanner
{
	public interface IVolumeScanner : IDisposable
	{
		// methods

		/// <summary>
		/// Begins scanning of a volume.
		/// The scanning main loop itself is running on a new thread, 
		/// so this method does not block and returns immediately after initialization.
		/// If this VolumeScanner was instanciated with a VolumeDatabase object passed to its constructor, 
		/// the VolumeDatabase object will be filled with the acquired information.
		/// On completion, the ScanCompleted event is raised.
		/// </summary>
		WaitHandle RunAsync();

		/// <summary>
		/// Notifies the scanning thread to initiate the cancellation phase.
		/// If a VolumeDatabase object is associated with this VolumeScanner instance, 
		/// the cancellation phase will involve a roll-back, reversing all changes that have been made to the database.
		/// The cancellation phase may take some time, depending on factors like activated hashing or buffersize.
		/// </summary>
		void CancelAsync();

		// properties
		bool IsBusy				{ get; }
		//bool CancellationPending	  { get; }
		bool ScanSucceeded		{ get; }
		//Media Media		  { get ; }
		VolumeInfo VolumeInfo	{ get; }
		
		
		// events

		/// <summary>
		/// Raised when the next item is scanned.
		/// </summary>
		event BeforeScanItemEventHandler	BeforeScanItem;

		/// <summary>
		/// Raised when an non-fatal error occurs during scanning (e.g. UnauthrizedAccessExepion, IOException). 
		/// Scanning continues if the assigned eventhandler doesn't set args.CancelScanning to true.
		/// </summary>
		event ScannerWarningEventHandler	ScannerWarning;

		/// <summary>
		/// Raised when an unhandled exception occurs on the scanning thread.
		/// </summary>
		event ErrorEventHandler				Error;

		/// <summary>
		/// Raised when scanning has been completed.
		/// To verify whether scanning was successful, check the Error and Cancelled properties of the EventArgs object.
		/// </summary>
		event ScanCompletedEventHandler		ScanCompleted;	
	}
}
