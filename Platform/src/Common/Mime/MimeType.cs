// MimeType.cs
// 
// Copyright (C) 2008, 2012 Patrick Ulbrich
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
	
namespace Platform.Common.Mime
{
	public static class MimeType
	{
		public const string MIME_TYPE_UNKNOWN = "application/octet-stream";
		
		public static string GetMimeTypeForFile(string filename) {
			string mimeType = null;
#if GNOME
			GLib.File file = GLib.FileFactory.NewForPath(filename);
			if (file.Exists) {
				// GLib backend
				// (null if the file does not exist)
				GLib.FileInfo info = file.QueryInfo ("standard::content-type", GLib.FileQueryInfoFlags.None, null);
				mimeType = info.ContentType;
			} else {
				// use mono winforms backend as fallack for non-existing files
				// (also takes filename extension into account, 
				// always returns a mimetype, even if the file does not exist)
				mimeType = Platform.Unix.Mime.Mime.GetMimeTypeForFile(filename);
			}

#elif UNIX
			// mono winforms backend
			// (also takes filename extension into account, 
			// always returns a mimetype, even if the file does not exist)
			mimeType = Platform.Unix.Mime.Mime.GetMimeTypeForFile(filename);
#elif WIN32
			// win32 registry backend
			// (uses filename extension only, always returns a mimetype)
			mimeType = Platform.Win32.Mime.RegistryMime.GetMimeTypeForExtension(filename);
#endif
			if (mimeType == null)
				return MIME_TYPE_UNKNOWN;
			else
				return mimeType;
		}
	}
}
