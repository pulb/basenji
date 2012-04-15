// MimeIconLookup.cs
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
using System.Collections.Generic;
//#if GNOME
//using Platform.Gnome;
//#endif

namespace Platform.Common.Mime
{	
	public class MimeIconLookup
	{		 
		private string defaultIcon;
		private Dictionary<string, string> mimeIcons;
		
		public MimeIconLookup() {
			mimeIcons = new Dictionary<string, string>();
		}
				
		public string GetIconNameForMimeType(string mimeType) {
			string iconName;

			if (mimeType == null)
				throw new ArgumentNullException("mimeType");
			
			if (mimeType.Length == 0)
				throw new ArgumentException("Argument is emtpy", "mimeType");
			
			if (mimeIcons.TryGetValue(mimeType, out iconName))
				return iconName;
#if GNOME
			GLib.ThemedIcon icon = (GLib.ThemedIcon) GLib.Content.TypeGetIcon(mimeType);
			foreach (string name in icon.Names) {
				if (Gtk.IconTheme.Default.HasIcon(name)) {
					iconName = name;
					break;
				}
			}
#else
			// TODO : find a portable implementation
			iconName = null;
#endif
			
			if (!string.IsNullOrEmpty(iconName)) {
				mimeIcons.Add(mimeType, iconName);
			}
			
			return iconName;
		}
	}
}
