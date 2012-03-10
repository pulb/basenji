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

#if GNOME
using System;
using System.IO;
using Gnome;
using Gdk;

namespace Platform.Gnome
{
	internal class ThumbnailGenerator : Platform.Common.IThumbnailGenerator
	{	
		private const DesktopThumbnailSize THUMB_SIZE = DesktopThumbnailSize.Normal; // 100 pix
		
		private DesktopThumbnailFactory tf;
		private Pixbuf thumbnail;
		private bool disposed;
		
		public ThumbnailGenerator() {
			disposed = false;
			tf = new DesktopThumbnailFactory(THUMB_SIZE);
		}
		
		public bool GenerateThumbnail(FileInfo fi, string mimeType) {
			if (thumbnail != null) {
				thumbnail.Dispose();
				thumbnail = null;
			}
			
			string uri = new Uri(fi.FullName).ToString();
			if (tf.CanThumbnail(uri, mimeType, fi.LastWriteTime)) {
				thumbnail = tf.GenerateThumbnail(uri, mimeType);
				if (thumbnail != null)
					return true;
			}
			return false;
		}
		
		public void SaveThumbnail(string filename) {
			if (thumbnail == null)
				throw new InvalidOperationException("no thumbnail generated");
			
			thumbnail.Save(filename, "png");
		}
		
		public void Dispose() {
			Dispose(true);
		}

		private void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					tf.Dispose();
				}
				tf = null;
			}
			disposed = true;
		}
	}
}
#endif