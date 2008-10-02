// GnomeNative.cs
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

#if GNOME
using System;
using System.Runtime.InteropServices;

namespace Platform.Gnome
{
	internal static class GnomeNative
	{		 
		public enum GnomeIconLookupFlags
		{
			GNOME_ICON_LOOKUP_FLAGS_NONE							= 0,
			GNOME_ICON_LOOKUP_FLAGS_EMBEDDING_TEXT					= 1 << 0,
			GNOME_ICON_LOOKUP_FLAGS_SHOW_SMALL_IMAGES_AS_THEMSELVES = 1 << 1,
			GNOME_ICON_LOOKUP_FLAGS_ALLOW_SVG_AS_THEMSELVES			= 1 << 2
		}
		
		[DllImport("libgnomeui-2")]
		public static extern string gnome_icon_lookup(IntPtr icon_theme, IntPtr thumbnail_factory, string file_uri, string custom_icon, IntPtr file_info, string mime_type, GnomeIconLookupFlags flags, IntPtr result);
		
		[DllImport("libgtk-x11-2.0")]
		public static extern IntPtr gtk_icon_theme_get_default();
		
		// gnome vfs functions
		[DllImport ("libgnomevfs-2")]
		public static extern bool gnome_vfs_init();
		
		[DllImport ("libgnomevfs-2")]
		public static extern bool gnome_vfs_initialized();	
		
		[DllImport("libgnomevfs-2")]
		public static extern string gnome_vfs_get_mime_type(string uri);
	}
}
#endif