// Main.cs
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
using Gtk;
using Platform.Common.Diagnostics;

namespace Basenji
{
	class MainClass
	{
		public static void Main (string[] args) {
			Debug.WriteLine(string.Format("{0} {1}", App.Name, App.Version));
			Debug.WriteLine(string.Format("Used runtime: {0}", System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()));
						
			Application.Init ();
			
			// GLib.Timeout / GLib.Application.Invoke() won't work without this
			// see http://bugzilla.ximian.com/show_bug.cgi?id=77130
			// TODO : remove this if this situation has changed
			if (CurrentPlatform.IsWin32)
				GLib.Thread.Init();			   

			// TODO : test _ON WINDOWS AND LINUX_ with: (the recommended way! no need to check for WIN32 then)
			// see http://bugzilla.ximian.com/show_bug.cgi?id=77130
			/*if (!GLib.Thread.Supported)
				GLib.Thread.Init();
			*/			  

			// load custom icon theme
			string themeLocation = App.Settings.CustomThemeLocation;
			string themeName = App.Settings.CustomThemeName;
			if (!string.IsNullOrEmpty(themeLocation) && !string.IsNullOrEmpty(themeName))
				Icons.CustomIconTheme.Load(System.IO.Path.Combine(themeLocation, themeName));			 
			
			Gui.MainWindow win = new Gui.MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}