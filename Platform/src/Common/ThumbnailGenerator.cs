// ThumbnailGenerator.cs
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
using System.IO;

namespace Platform.Common
{	
	internal interface IThumbnailGenerator : IDisposable
	{
		bool GenerateThumbnail(FileInfo fi, string mimeType);
		void SaveThumbnail(string filename);
	}
	
	public class ThumbnailGenerator : IThumbnailGenerator, IDisposable
	{
		private IThumbnailGenerator tg;
		private bool disposed;
	
		public ThumbnailGenerator() {
			disposed = false;
#if GNOME
			tg = new Platform.Gnome.ThumbnailGenerator();
#else
			// TODO : imlpement me
			tg = null;
#endif	

		}
		
		public bool GenerateThumbnail(FileInfo fi, string mimeType) {
			EnsureNotDisposed();
			
			if (fi == null)
				throw new ArgumentNullException("fi");
			if (mimeType == null)
				throw new ArgumentNullException("mimeType");

			if (tg != null)
				return tg.GenerateThumbnail(fi, mimeType);
			else
				return false;
		}
		
		public void SaveThumbnail(string filename) {
			EnsureNotDisposed();
			
			if (filename == null)
				throw new ArgumentNullException("filename");
			
			if (tg != null)
				tg.SaveThumbnail(filename);
		}
		
		public void Dispose() {
			Dispose(true);
		}

		private void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					if (tg != null)
						tg.Dispose();
				}
				tg = null;
			}
			disposed = true;
		}
		
		private void EnsureNotDisposed() {
			if (disposed)
				throw new ObjectDisposedException("ThumbnailGenerator");
		}
	}
}
