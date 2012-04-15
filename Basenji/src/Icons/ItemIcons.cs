// ItemIcons.cs
// 
// Copyright (C) 2008 - 2012 Patrick Ulbrich
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
using Gtk;
using VolumeDB;

namespace Basenji.Icons
{	 
	/* class that looks up icons for 
	 * VolumeDatabase items (e.g. files, folders, ...) */
	public class ItemIcons
	{
		private readonly Icons.Icon DEFAULT_ICON = Icon.Stock_File;
		
		private MimeIconCache	mimeIconCache;
		private IconCache		iconCache;
		
		public ItemIcons(Widget w) {
			// create a cache for item icons
			iconCache = new IconCache(w);
			
			bool useCustomMimeIcons = !string.IsNullOrEmpty(App.Settings.CustomThemeName);
			// create a cache for mime icons and add default and fallback icons
			// for platforms where mime icons are not implemented
			mimeIconCache = new MimeIconCache(w, useCustomMimeIcons, DEFAULT_ICON, new Dictionary<string, Icons.Icon>() {
				{ "x-directory/normal", Icon.Stock_Directory }
			});
		}
		
		public Gdk.Pixbuf GetIconForItem(VolumeItem item, Gtk.IconSize iconSize) {
			Gdk.Pixbuf pb;
			
			if ((item is FileSystemVolumeItem) && (((FileSystemVolumeItem)item).IsSymLink)) {
				return iconCache.GetIcon(Icon.SymLink, iconSize);
			}
			
			string mimeType = item.MimeType;
			if (string.IsNullOrEmpty(mimeType))
				pb = iconCache.GetIcon(DEFAULT_ICON, iconSize);
			else				
				pb = mimeIconCache.GetIcon(mimeType, iconSize);
			
			return pb;
		}
	}
}
