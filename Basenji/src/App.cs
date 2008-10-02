// App.cs
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
using System.Reflection;

namespace Basenji
{
	/* static class providing several info about the application */
	static class App
	{
		private static string name;
		private static string version;
		private static string copyright;
		
		private static Settings settings;
		
		static App() {
			Assembly asm = Assembly.GetExecutingAssembly();
			
			name = asm.GetName().Name;
			version = asm.GetName().Version.ToString();			

			object[] attr = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			copyright = ((AssemblyCopyrightAttribute)attr[0]).Copyright;
			
			settings = null;
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
		
	}
}