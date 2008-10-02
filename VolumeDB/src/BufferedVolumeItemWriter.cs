// BufferedVolumeItemWriter.cs
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
	internal sealed class BufferedVolumeItemWriter : IDisposable
	{
		private VolumeDatabase	database;
		private bool			leaveOpen;

		private int				buffCounter; // buffer counter
		private VolumeItem[]	buffer;

		private bool			disposed;
		
		public BufferedVolumeItemWriter(VolumeDatabase database, int size) : this(database, false, size) { }
		public BufferedVolumeItemWriter(VolumeDatabase database, bool leaveOpen, int size) {
			if (database == null)
				throw new ArgumentNullException("database");

			if (size < 1)
				throw new ArgumentOutOfRangeException("size");

			this.database	= database;
			this.leaveOpen	= leaveOpen;

			this.buffer		= new VolumeItem[size];
			this.disposed	= false;

			Reset();
		}
		
		public void Write(VolumeItem item) {
			EnsureOpen();
			buffer[buffCounter++] = item;
			if (buffCounter >= buffer.Length) {
				database.InsertVolumeItems(buffer);
				buffCounter = 0;
			}
		}
		
		public void Flush() {
			EnsureOpen();
			if (buffCounter > 0) {
				VolumeItem[] remainder = new VolumeItem[buffCounter];

				Array.Copy(buffer, remainder, buffCounter);
				database.InsertVolumeItems(remainder);
				buffCounter = 0;
			}
		}
		
		public void Reset() {
			buffCounter = 0;
		}		 

		public void Close() {
			Dispose(true);
		}
		
		private void EnsureOpen() {
			if (disposed)
				throw new ObjectDisposedException(this.ToString());
		}

		private void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					this.Flush();

					if (database != null && !leaveOpen)
						database.Close();
				}
				buffer		= null;
				database	= null;

				disposed = true;
			}
		}
		
		#region IDisposable Members
		
		void IDisposable.Dispose() {
			Dispose(true);
		}

		#endregion
	}
}
