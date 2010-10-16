// CustomIconTheme.cs
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

// TODO: faster? more mem usage? longer startup?
/*#define LOAD_PIXBUFS*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Gtk;
using Gdk;
using Platform.Common.Diagnostics;

namespace Basenji.Icons
{	 
	public static class CustomIconTheme
	{
		public static void Load(string themePath) {
			if (themePath == null)
				throw new ArgumentNullException("themePath");
			
			if (themePath.Length == 0)
				throw new ArgumentException("Argument is empty", "themePath");
			
			// gtk requires an absolute path
			if (!Path.IsPathRooted(themePath))
				throw new ArgumentException("Path must be absolute", "themePath");
				
			if (!Directory.Exists(themePath))
				throw new DirectoryNotFoundException(string.Format("Path to theme \"{0}\" not found", themePath));

			//IconSize[]				  iconSizes   = (IconSize[])Enum.GetValues(typeof(IconSize));
			
			// all icon sizes the app uses			  
			IconSize[] iconSizes =	{	IconSize.Menu,			/* 16px */
										IconSize.LargeToolbar,	/* 24px */
										IconSize.Button,		/* 24px */
										IconSize.Dialog			/* 48px */
									};
			
			Dictionary<string, string>	iconNames	= GetAllIconNames();			
			IconFactory					fac			= new IconFactory();			

			foreach (KeyValuePair<string, string> namePair in iconNames) {
				
				string	name				= namePair.Key;
				string	nameInCustomTheme	= namePair.Value;
				IconSet iconSet				= new IconSet();
				bool	setHasSources		= false;
				
				foreach (Gtk.IconSize size in iconSizes) {

					int    sz		= IconUtils.GetIconSizeVal(size);
					string fullPath = Path.Combine(Path.Combine(themePath, sz.ToString()), nameInCustomTheme);

					if (!File.Exists(fullPath)) {
						if (Global.EnableDebugging) {
							Debug.WriteLine(string.Format("IconTheme: could not find custom icon for \"{0}\" (size = {1}), using system default", name, sz));
						}
						continue;
					}
					
					IconSource source = new IconSource();
					
#if LOAD_PIXBUFS
					source.Pixbuf = new Gdk.Pixbuf(fullPath);
#else
					source.Filename = fullPath;
#endif
					
					source.Size = size;
					//source.IconName = name;
					source.SizeWildcarded = false;
					
					iconSet.AddSource(source);
					setHasSources = true;
				}
				if (setHasSources)
					fac.Add(name, iconSet);
			}

			fac.AddDefault(); // add icon factory to the apps default factories
		}
		
		private static Dictionary<string, string> GetAllIconNames() {
			Dictionary<string, string> names = new Dictionary<string, string>();
			
			Type t = typeof(Icons.Icon);
			PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach(PropertyInfo pi in propInfos) {
				if (pi.PropertyType == typeof(Icons.Icon)) {
					// get default name					   
					Icon icon = (Icon)pi.GetValue(null, null);
					// get name in a custom theme					 
					object[] attribs = pi.GetCustomAttributes(typeof(Icons.NameInCustomIconThemeAttribute), true);
					if (attribs.Length == 0)
						throw new NotImplementedException(string.Format("Property \"{0}\" does not have the NameInCustomIconThemeAttribute", pi.Name));
					Icons.NameInCustomIconThemeAttribute attr = (Icons.NameInCustomIconThemeAttribute)attribs[0];

					names.Add(icon.Name, attr.Name);
				}
			}
			return names;
		}
	}
	
//	  public static class IconLoader
//	  {
//		  private static string customThemePath;
//		  
//		  private static bool enableCaching;
//		  private static Dictionary<string, Pixbuf> iconCache;
//		  
//		  static IconLoader() {
//			  enableCaching = false;
//			  iconCache = new Dictionary<string, Pixbuf>();
//		  }
//		  
//		  public static string CustomThemePath {
//			  get { return customThemePath; }
//			  set { customThemePath = value; }
//		  }
//		  
//		  public static bool EnableCaching {
//			  get { return enableCaching; }
//			  set { enableCaching = value; }
//		  }
//		  
//		  public static void ClearCache() {
//			  iconCache.Clear();		
//		  }
//		  
//		  public static Pixbuf LoadIcon(string name, Gtk.IconSize size) {
//			  Pixbuf icon;
//			  int sz = GetSize(size);
//			  string iconKey = name + sz;
//			  
//			  if (enableCaching && iconCache.TryGetValue(iconKey, out icon))
//				  return icon;
//
//			  if (string.IsNullOrEmpty(customThemePath))
//				  icon = LoadIconFromSystemTheme(name, sz); // use system theme
//			  else
//				  icon = LoadIconFromCustomTheme(name, sz);
//			  
//			  if (enableCaching) {			  
//				  iconCache.Add(iconKey, icon);
//				  Debug.WriteLine(string.Format("IconLoader cached icon \"{0}\" (size = {1})", name, sz));
//			  }
//			  
//			  return icon;
//		  }
//		  
//		  private static Pixbuf LoadIconFromSystemTheme(string name, int size) {
//			  return Gtk.IconTheme.Default.LoadIcon(name, size, 0);		   
//		  }
//		  
//		  private static Pixbuf LoadIconFromCustomTheme(string name, int size) {
//			  if (!Directory.Exists(customThemePath))
//				  throw new DirectoryNotFoundException(string.Format("Path to custom theme \"{0}\" not found", customThemePath));
//			  
//			  string iconPath = Path.Combine(Path.Combine(customThemePath, size.ToString()), name);
//			  return new Pixbuf(iconPath);
//		  }
//		  
//		  private static int GetSize(Gtk.IconSize size) {
//			  int sz;
//			  switch (size) {
//				  case Gtk.IconSize.Button:
//					  sz = 20;
//					  break;
//				  case Gtk.IconSize.Dialog:
//					  sz = 48;
//					  break;
//				  case Gtk.IconSize.Dnd:
//					  sz = 32;
//					  break;
//				  case Gtk.IconSize.LargeToolbar:
//					  sz = 24;
//					  break;
//				  case Gtk.IconSize.Menu:
//					  sz = 16;
//					  break;
//				  case Gtk.IconSize.SmallToolbar:
//					  sz = 18;
//					  break;
//				  default:
//					  sz = 16;
//					  break;
//			  }
//			  return sz;
//		  }
//	  }
}
