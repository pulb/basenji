// Events.cs
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
	public delegate void BeforeScanItemEventHandler(object sender, BeforeScanItemEventArgs e);
	public delegate void ScannerWarningEventHandler(object sender, ScannerWarningEventArgs e);
	public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);	
	public delegate void ScanCompletedEventHandler(object sender, ScanCompletedEventArgs e);

	public class BeforeScanItemEventArgs : EventArgs
	{
		private string itemName;

		public BeforeScanItemEventArgs(string itemName) : base() {
			this.itemName = itemName;
		}

		public string ItemName {
			get { return itemName ?? string.Empty; }
		}
	}

	public class ScannerWarningEventArgs : EventArgs
	{
		private bool		cancel;
		private string		message;
		private Exception	ex;

		public ScannerWarningEventArgs(string message, Exception ex) : base() {
			this.cancel		= false;
			this.message	= message;
			this.ex			= ex;
		}

		public ScannerWarningEventArgs(string message) : this(message, null) { }

		//public ScannerWarningEventArgs() : this(null, null) { }

		public bool CancelScanning {
			get { return cancel; }
			set { cancel = value; }
		}

		public string Message {
			get { return message ?? string.Empty; }
		}

		public Exception Exception {
			get { return ex; }
		}
	}

	public class ErrorEventArgs : EventArgs
	{
		private Exception ex;
		
		public ErrorEventArgs(Exception ex) : base() {
			this.ex = ex;
		}

		//public ErrorEventArgs() : this(null) {}

		public Exception Exception {
			get { return ex; }
		}
	}

	public class ScanCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{
		//private ScanningResult  m_result;
		//private long volumeID;
		private Volume volume;
		//private Exception		  m_fatalError;

		//public ScanCompletedEventArgs(ScanningResult result, long mediaID, Exception fatalError) : base()
		public ScanCompletedEventArgs(Volume volume, Exception error, bool cancelled) : base(error, cancelled, null) {
			//m_result		  = result;
			//this.volumeID = volumeID;
			this.volume = volume;
			//m_fatalError	  = fatalError;
		}

		//public ScanCompletedEventArgs(ScanningResult result, long mediaID) : this(result, mediaID, null) {}

		//public ScanningResult Result
		//{
		//	  get { return m_result; }
		//}

//		  public long VolumeID {
//			  get {
//				  RaiseExceptionIfNecessary();
//				  return volumeID;
//			  }
//		  }

		  public Volume Volume {
			get {return volume; }
		  }

		//public Exception FatalError
		//{
		//	  get { return m_fatalError; }
		//}
	}
}
