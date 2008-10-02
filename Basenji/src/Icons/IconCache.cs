// IconCache.cs
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
using Gdk;
using Gtk;
//using Platform.Common.Diagnostics;

namespace Basenji.Icons
{
	// caches already rendered pixbufs
	public class IconCache
	{
		private Dictionary<string, Pixbuf> iconCache;
		private Widget widget;
		
		public IconCache(Widget w) {
			widget = w;
			iconCache = new Dictionary<string, Pixbuf>();
		}
		
		public void Clear() {
			iconCache.Clear();		  
		}
		
		public Pixbuf GetIcon(Icons.Icon icon, IconSize size) {
			Pixbuf pb;
			string iconKey = icon.Name + (int)size;
			
			if (iconCache.TryGetValue(iconKey, out pb))
				return pb;
			
			pb = icon.Render(widget, size);
			if (pb == null)
				return null;
			
			iconCache.Add(iconKey, pb);
			//Debug.WriteLine(string.Format("IconCache: cached icon \"{0}\" (size = {1})", icon.Name, IconUtils.GetIconSizeVal(size)));
			
			return pb;
		}
		
	}
}
