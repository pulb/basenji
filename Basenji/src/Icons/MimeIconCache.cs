// MimeIconCache.cs
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
using System.Collections.Generic;
using Platform.Common.Mime;
using Platform.Common.Diagnostics;
using Gdk;

namespace Basenji.Icons
{
	public class MimeIconCache
	{
		private MimeIconLookup mimeIconLookup;
		private Dictionary<string, Gdk.Pixbuf> mimeIconCache;
		
		public MimeIconCache() {
			mimeIconLookup = new MimeIconLookup();	 
			mimeIconCache = new Dictionary<string, Gdk.Pixbuf>();
		}
		
		public MimeIconLookup MimeIconLookup { get { return mimeIconLookup; } }
		
		public void Clear() {
			mimeIconCache.Clear();
		}
		
		public Pixbuf GetIcon(string mimeType, Gtk.IconSize size) {
			if (mimeType == null)
				throw new ArgumentNullException("mimeType");
			
			if (mimeType.Length == 0)
				throw new ArgumentException("Argument is emtpy", "mimeType");			 
			
			Pixbuf pb;
			string iconKey = mimeType + (int)size;
			
			if (mimeIconCache.TryGetValue(iconKey, out pb))
				return pb;
			
			string iconName = mimeIconLookup.GetIconNameForMimeType(mimeType);

			try {
				pb = Gtk.IconTheme.Default.LoadIcon(iconName, Icons.IconUtils.GetIconSizeVal(size), 0);					   
				mimeIconCache.Add(iconKey, pb);
				return pb;
			} catch (Exception) {
				Debug.WriteLine(string.Format("MimeIconCache: IconTheme.Default.LoadIcon() failed to render icon \"{0}\"", iconName));	  
			}
			
			return null;				
		}
	}
}
