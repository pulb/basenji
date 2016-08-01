// MimeIconCache.cs
// 
// Copyright (C) 2008 - 2016 Patrick Ulbrich
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
using Gdk;
using Gtk;

namespace Basenji.Icons
{
	// caches already rendered mime icon pixbufs
	public class MimeIconCache
	{
		private bool useCustomMimeIcons;

		private CustomIconThemeMimeMapping customMimeMapping;
		
		private Dictionary<string, Gdk.Pixbuf> mimeIconCache;
		private Dictionary<string, Icons.Icon> fallbackIcons;
		private Icon defaultIcon;
		private Widget widget;
		
		public MimeIconCache(Widget w, bool useCustomMimeIcons, 
		                     Icon defaultIcon, Dictionary<string, Icons.Icon> fallbackIcons) {
			
			this.widget = w;
			this.useCustomMimeIcons = useCustomMimeIcons;
			
			if (useCustomMimeIcons)
				customMimeMapping = new CustomIconThemeMimeMapping();
			
			mimeIconCache = new Dictionary<string, Gdk.Pixbuf>();
			
			if (fallbackIcons == null)
				this.fallbackIcons = new Dictionary<string, Icon>();
			else
				this.fallbackIcons = fallbackIcons;
			
			this.defaultIcon = defaultIcon;
		}

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
			
			if (useCustomMimeIcons) {
				// render icons which are available in the custom theme
				Icon icon;
				if (customMimeMapping.TryGetIconForMimeType(mimeType, out icon))
					pb = icon.Render(widget, size);
				else
					pb = defaultIcon.Render(widget, size);
			} else {
				// render system mime icons dynamically
				Gtk.IconTheme iconTheme = Gtk.IconTheme.Default;
				string iconName = null;

				foreach (string name in ((GLib.ThemedIcon)GLib.Content.TypeGetIcon(mimeType)).Names) {
					if (iconTheme.HasIcon(name)) {
						iconName = name;
						break;
					}
				}

				if (!string.IsNullOrEmpty(iconName)) {
					pb = iconTheme.LoadIcon(iconName, IconUtils.GetIconSizeVal(size), 0);
				} else {
					Icon fbIcon;
					if (fallbackIcons.TryGetValue(mimeType, out fbIcon)) {
						iconName = fbIcon.Name;
						pb = fbIcon.Render(widget, size);			
					} else {						
						pb = defaultIcon.Render(widget, size);
						iconName = defaultIcon.Name;
					}
				}
			}
			
			if (pb != null)
				mimeIconCache.Add(iconKey, pb);
					
			return pb;
		}
	}
}
