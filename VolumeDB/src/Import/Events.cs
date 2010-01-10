// Events.cs
// 
// Copyright (C) 2010 Patrick Ulbrich
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
using System.ComponentModel;

namespace VolumeDB.Import
{
	public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);	
	public delegate void ImportCompletedEventHandler(object sender, ImportCompletedEventArgs e);
	public delegate void ProgressUpdateEventHandler(object sender, ProgressUpdateEventArgs e);
	
	public class ErrorEventArgs : EventArgs
	{
		private Exception ex;
		
		public ErrorEventArgs(Exception ex) : base() {
			this.ex = ex;
		}

		public Exception Exception {
			get { return ex; }
		}
	}

	public class ImportCompletedEventArgs : AsyncCompletedEventArgs
	{
		public ImportCompletedEventArgs(Exception error, bool cancelled) 
		: base(error, cancelled, null) {}
	}
	
	public class ProgressUpdateEventArgs : EventArgs
	{
		private double completed;
		
		public ProgressUpdateEventArgs(double completed) : base() {
			this.completed = completed;
		}

		public double Completed {
			get { return completed; }
		}
	}
}
