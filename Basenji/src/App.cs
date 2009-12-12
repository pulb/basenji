// App.cs
// 
// Copyright (C) 2008, 2009 Patrick Ulbrich
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
using System.IO;
using System.Reflection;

namespace Basenji
{
	/* static class providing several info about the application */
	static class App
	{
		// search result limit,
		// limits the result size in the item search window
		public const int SEARCH_RESULTS_LIMIT	= 10000;
		
//		public const string DEFAULT_DB			= "volumes.vdb";
		public const string WINDOW_DEFAULT_ICON	= "data/basenji.svg";
		
		private static string name;
		private static string version;
		private static string copyright;
		
		private static Settings settings;
		private static string defaultDB;
		
		static App() {
			Assembly asm = Assembly.GetExecutingAssembly();
			
			name = asm.GetName().Name;
			version = asm.GetName().Version.ToString();			

			object[] attr = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			copyright = ((AssemblyCopyrightAttribute)attr[0]).Copyright;
			
			settings = null; // lazy initialized
			defaultDB = null; // lazy initialied as well (depends on lazy initialized Settings)
		}
		
		public static string Name {
			get { return name; }
		}
		
		public static string Version {
			get { return version; }
		}
		
		public static string Copyright {
			get { return copyright; }
		}
		
		public static Settings Settings {
			get {
				if (settings == null)
					settings = new Settings();
				return settings;
			}
		}
		
		public static string DefaultDB {
			get {
				if (defaultDB == null) {
					defaultDB = Path.Combine(
					                         Settings.GetSettingsDirectory().FullName,
					                         "volumes.vdb"
					                         );
				}
				return defaultDB;
			}
		}
		
	}
}