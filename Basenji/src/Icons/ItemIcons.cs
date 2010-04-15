// ItemIcons.cs
// 
// Copyright (C) 2008, 2010 Patrick Ulbrich
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
using Gtk;
using VolumeDB;

namespace Basenji.Icons
{	 
	/* class that looks up icons for 
	 * VolumeDatabase items (e.g. files, folders, ...) */
	public class ItemIcons
	{
		private readonly Icons.Icon DEFAULT_ICON = Icon.Stock_File;
		
		private bool			useMimeIcons;		 
		private MimeIconCache	mimeIconCache;
		private IconCache		iconCache;
		
		public ItemIcons(Widget w) {
			iconCache = new IconCache(w);
			
			// only use the systems mime icons if no custom theme is set			
			useMimeIcons = string.IsNullOrEmpty(App.Settings.CustomThemeName);
			if (useMimeIcons) {		
				mimeIconCache = new MimeIconCache();
				
				// default icons for platforms where mime icons are not implemented
				mimeIconCache.MimeIconLookup.DefaultIcon = DEFAULT_ICON.Name;
				mimeIconCache.MimeIconLookup.AddFallbackIcon("x-directory/normal", Icon.Stock_Directory.Name);
			}
			
		}
		
		public Gdk.Pixbuf GetIconForItem(VolumeItem item, Gtk.IconSize iconSize) {
			Gdk.Pixbuf pb;
			
			if ((item is FileSystemVolumeItem) && (((FileSystemVolumeItem)item).IsSymLink)) {
				return iconCache.GetIcon(Icon.SymLink, iconSize);
			}
			
			if (useMimeIcons) {
				string mimeType = item.MimeType;
				if (string.IsNullOrEmpty(mimeType))
					pb = iconCache.GetIcon(DEFAULT_ICON, iconSize);
				else				
					pb = mimeIconCache.GetIcon(mimeType, iconSize);
			} else {
				Icons.Icon icon;
				switch (item.GetVolumeItemType()) {
					case VolumeItemType.DirectoryVolumeItem:
						icon = Icon.Stock_Directory;
						break;
					case VolumeItemType.FileVolumeItem:
						icon = Icon.Stock_File;
						break;
					case VolumeItemType.AudioTrackVolumeItem:
						icon = Icon.Category_Music;
						break;
					default:
						throw new NotImplementedException(string.Format("GetIconForItem() is not implemented for VolumeItemType {0}", item.GetVolumeItemType()));
				}				 
				pb = iconCache.GetIcon(icon, iconSize);
			}
			
			return pb;
		}
	}
}
