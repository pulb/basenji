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
using System.Collections.Generic;
using Gtk;
using Platform.Common.Diagnostics;
using NDesk.Options;

namespace Basenji
{
	class MainClass
	{
		public static void Main (string[] args) {
			
			string dbPath;
			bool debug;
			
			if (!GetOptions(args, out dbPath, out debug))
				return;
			
			if (debug) {
				Basenji.Global.EnableDebugging = true;
				VolumeDB.Global.EnableDebugging = true;
			}
			
			Debug.WriteLine(string.Format("{0} {1}", App.Name, App.Version));
			Debug.WriteLine(string.Format("Used runtime: {0}",
			                              System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()));
			
			if (CurrentPlatform.IsUnix)
				Util.SetProcName(App.Name.ToLower());
				
			Application.Init();

			// see http://bugzilla.ximian.com/show_bug.cgi?id=77130
			if (!GLib.Thread.Supported)
				GLib.Thread.Init();

			using (new InstanceLock()) {
				// load custom icon theme
				string themeName = App.Settings.CustomThemeName;
				if (!string.IsNullOrEmpty(themeName)) {
					string path = System.IO.Path.Combine(App.CUSTOM_THEME_PATH, themeName);
					string fullPath = System.IO.Path.GetFullPath(path);
					Icons.CustomIconTheme.Load(fullPath);
				}
				
				Gui.MainWindow win = new Gui.MainWindow (dbPath);
				Gui.Base.WindowBase.MainWindow = win;
				
				win.Show();
				Application.Run();
			}
		}
		
		private static bool GetOptions(string[] args, out string dbPath, out bool debug) {
			
			bool show_help = false;
			string optDbPath = null;
			bool optDebug = false;
			
			dbPath = optDbPath;
			debug = optDebug;
			
			// parse options
			var p = new OptionSet() {
				{ "g|debug",
					"enable debugging output",
					v => optDebug = (v != null)
				},
				{ "d|database=",
					"path of database to open",
					(string v) => optDbPath = v
				},
				/* help */
				{ "h|help", 
					"show this message and exit",
					v => show_help = (v != null)
				}
			};
			
			try {
				List<string> extra = p.Parse(args);
				
				if (extra.Count > 0)
					throw new OptionException(string.Format("Unknown option: {0}", extra[0]), extra[0]);
					                          
			} catch (OptionException e) {
				Console.Write(string.Format("{0}: ", App.Name));
				Console.WriteLine(e.Message);
				Console.WriteLine(string.Format("Try `{0}: --help' for more information.", App.Name.ToLower()));
				return false;
			}
			
			if (show_help) {
				ShowHelp(p);
				return false;
			}
			
			dbPath = optDbPath;
			debug = optDebug;
			
			return true;
		}
		
		private static void ShowHelp(OptionSet p) {
			Console.WriteLine (string.Format("Usage: {0} [OPTIONS]", App.Name.ToLower()));
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			p.WriteOptionDescriptions (Console.Out);
		}
		
		// allows only one instance of the app
		private class InstanceLock : IDisposable
		{
			private System.Threading.Mutex mtx;
			
			public InstanceLock() {
			 	mtx = new System.Threading.Mutex(false, "715829bd-de3b-44c0-8bbc-a542eec8d8be");
				if (!mtx.WaitOne(1, true)) {
					MsgDialog.Show(null, MessageType.Error, ButtonsType.Ok, S._("Error"), string.Format(S._("{0} is already running."), App.Name));
					Environment.Exit(0);
				}
			}
			
			public void Dispose() {
				mtx.ReleaseMutex();
				mtx.Close();
			}
		}
	}
}