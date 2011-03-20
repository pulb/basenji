// MetadataProvider.cs
//
// Copyright (C) 2011 Patrick Ulbrich
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

namespace VolumeDB.Metadata
{

	public abstract class MetadataProvider : IDisposable
	{
		private bool disposed;
		
		internal MetadataProvider () {
			disposed = false;
		}
		
		public abstract IEnumerable<MetadataItem> GetMetadata(string filename);
		
//		public void CopyTo(MetaDataProvider p) {
//			CopyProvider(p);
//		}
//		
//		protected abstract void CopyProvider(MetaDataProvider p);
		
		#region IDisposable Members

		public void Dispose() {
			Dispose(true);
		}

		#endregion
		
		protected virtual void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					// nothing to do for now
				}
				
				disposed  = true;
			}
		}
		
		protected void EnsureNotDisposed() {
			if (disposed)
				throw new ObjectDisposedException("MetadataProvider");
		}	
	}
}
