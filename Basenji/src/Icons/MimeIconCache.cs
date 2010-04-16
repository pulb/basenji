// MimeIconCache.cs
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
using System.Collections.Generic;
using Platform.Common.Mime;
using Platform.Common.Diagnostics;
using Gdk;
using Gtk;

namespace Basenji.Icons
{
	public class MimeIconCache
	{
		private bool useCustomMimeIcons;
		
		private MimeIconLookup mimeIconLookup;
		private CustomIconThemeMimeMapping customMimeMapping;
		
		private Dictionary<string, Gdk.Pixbuf> mimeIconCache;
		private Widget widget;
		
		public MimeIconCache(Widget w, bool useCustomMimeIcons, Icon defaultIcon) {
			this.widget = w;
			this.useCustomMimeIcons = useCustomMimeIcons;
			
			if (useCustomMimeIcons) {
				customMimeMapping = new CustomIconThemeMimeMapping() {
					DefaultIcon = defaultIcon
				};
			} else {
				mimeIconLookup = new MimeIconLookup() {
					DefaultIcon = defaultIcon.Name
				};
			}
			
			mimeIconCache = new Dictionary<string, Gdk.Pixbuf>();
		}
		
		public void AddLookupFallbackIcon(string mimeType, Icon icon) {
			if (mimeIconLookup != null)
				mimeIconLookup.AddFallbackIcon(mimeType, icon.Name);
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
				pb = customMimeMapping
					.GetIconForMimeType(mimeType)
						.Render(widget, size);
				
				if (pb == null)
					return null;
				
				mimeIconCache.Add(iconKey, pb);
				
				return pb;				
			} else {
				// render system mime icons dynamically
				string iconName = mimeIconLookup.GetIconNameForMimeType(mimeType);
				
				try {
					pb = Gtk.IconTheme.Default.LoadIcon(iconName, IconUtils.GetIconSizeVal(size), 0);
					mimeIconCache.Add(iconKey, pb);
					return pb;
				} catch (Exception) {
					Debug.WriteLine(string.Format("MimeIconCache: IconTheme.Default.LoadIcon() failed to render icon \"{0}\"", iconName));	  
				}
				
				return null;
			}
		}
	}
}
