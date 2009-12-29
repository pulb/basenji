// ThumbnailGenerator.cs
// 
// Copyright (C) 2009 Patrick Ulbrich
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

#if WIN32
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using vbAccelerator.Components.Shell;

namespace Platform.Win32
{
	internal class ThumbnailGenerator : Platform.Common.IThumbnailGenerator
	{
		private const int THUMB_SIZE = 100;
		
		private bool disposed;
		private ThumbnailCreator tg;
		private Bitmap thumbnail;
		
		public ThumbnailGenerator () {
			disposed = false;
			tg = new ThumbnailCreator();
			tg.DesiredSize = new Size(THUMB_SIZE, THUMB_SIZE);
			thumbnail = null;
		}
		
		public bool GenerateThumbnail(FileInfo fi, string mimeType) {
			if (thumbnail != null) {
				thumbnail.Dispose();
				thumbnail = null;
			}
			
			try {
				// FIXME : 
				// tg.GetThumbNail() seems to throw a FileNotFoundException
				// for every single file it can't create a thumbnail for.
				// Only generate thumbnails for cherry-picked mimetypes?
				thumbnail = tg.GetThumbNail(fi.FullName);
			} catch (Exception) { }
			
			return (thumbnail != null);
		}
		
		public void SaveThumbnail(string filename) {
			if (thumbnail == null)
				throw new InvalidOperationException("no thumbnail generated");
			
			thumbnail.Save(filename, ImageFormat.Png);
		}
		
		public void Dispose() {
			Dispose(true);
		}

		private void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					tg.Dispose();
				}
				tg = null;
			}
			disposed = true;
		}
	}
}
#endif