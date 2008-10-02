// RegistryMime.cs
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

#if WIN32
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using Platform.Common.Mime;

namespace Platform.Win32.Mime
{
	public static class RegistryMime
	{
		private static Dictionary<string, string> mimeTypes;
		
		static RegistryMime() {
			mimeTypes = new Dictionary<string, string>();		 
		}
		
		public static string GetMimeTypeForExtension(string filename) {
			if (filename == null)
				throw new ArgumentNullException("filename");
			
			if (filename.Length == 0)
				throw new ArgumentException("Argument is emtpy", "filename");
			
			string mimeType;			
			string extension = Path.GetExtension(filename);
			
			if (extension.Length == 0)
				return MimeType.MIME_TYPE_UNKNOWN;			  
			
			if (!extension.StartsWith("."))
				extension = "." + extension;
			
			if (mimeTypes.TryGetValue(extension, out mimeType))
				return mimeType;

			RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension);
			
			if (key == null)
				mimeType = MimeType.MIME_TYPE_UNKNOWN;
			else
				mimeType = (string)key.GetValue("Content Type");
			
			mimeTypes.Add(extension, mimeType);
			
			return mimeType;
		}		 
	}
}
#endif