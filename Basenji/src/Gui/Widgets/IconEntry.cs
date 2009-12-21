// IconEntry.cs
// 
// Copyright (C) 2009 Patrick Ulbrich
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
using System.Runtime.InteropServices;
using GLib;
using Gtk;

namespace Basenji.Gui.Widgets
{
	// Temporary hack that enables icons in Gtk's 2.16 Entry widget.
	// This file can be removed when gtk# 2.16 bindings are ready.
	public class IconEntry : Entry
	{
		public void SetIconFromStock(string stockIcon, EntryIconPosition iconPos) {
			try {
				gtk_entry_set_icon_from_stock(this.Handle, iconPos, stockIcon);
				
			} catch(EntryPointNotFoundException) {
			} catch(DllNotFoundException) {
			}
		}
		
		public void SetIconActivatable(EntryIconPosition iconPos, bool activatable) {
			try {
				gtk_entry_set_icon_activatable(this.Handle, iconPos, activatable);
				
			} catch(EntryPointNotFoundException) {
			} catch(DllNotFoundException) {
			}
		}
		
		[Signal("icon_press")]
		public event IconPressReleaseEventHandler IconPress {
			add {
				Signal.Lookup(this,
				              "icon_press",
				              new SignalCallbackDelegate(IconEntry.SignalCallback)
				              ).AddDelegate(value);
			}
			remove {
				Signal.Lookup(this,
				              "icon_press",
				              new SignalCallbackDelegate(IconEntry.SignalCallback)
				              ).RemoveDelegate(value);
			}
		}
		
		[Signal("icon_release")]
		public event IconPressReleaseEventHandler IconRelease {
			add {
				Signal.Lookup(this,
				              "icon_release",
				              new SignalCallbackDelegate(IconEntry.SignalCallback)
				              ).AddDelegate(value);
			}
			remove {
				Signal.Lookup(this,
				              "icon_release",
				              new SignalCallbackDelegate(IconEntry.SignalCallback)
				              ).AddDelegate(value);
			}
		}
		
		[CDeclCallback]
		private delegate void SignalCallbackDelegate(IntPtr arg0, int arg1, IntPtr arg2, IntPtr gch);
		
		private static void SignalCallback(IntPtr arg0, int arg1, IntPtr arg2, IntPtr gch) {
			IconPressReleaseEventArgs args = new IconPressReleaseEventArgs();
			try {
				GCHandle handle = (GCHandle) gch;
				Signal target = handle.Target as Signal;
				if (target == null) {
					throw new Exception("Unknown signal GC handle received " + gch);
				}
				
				args.Args = new object[] { arg1 };
				IconPressReleaseEventHandler handler = (IconPressReleaseEventHandler)target.Handler;
				handler(GLib.Object.GetObject(arg0), args);
				
	        } catch (Exception exception) {
				ExceptionManager.RaiseUnhandledException(exception, false);
			}
		}
		
		[DllImport("libgtk-x11-2.0.so")]
		private static extern void gtk_entry_set_icon_from_stock(IntPtr gtk_entry, EntryIconPosition icon_pos, string icon_name);
		
		[DllImport("libgtk-x11-2.0.so")]
		private static extern void gtk_entry_set_icon_activatable(IntPtr gtk_entry, EntryIconPosition icon_pos, bool activatable);
	}
	
	public class IconPressReleaseEventArgs : SignalArgs {
		
		public EntryIconPosition IconPos {
			get {
				return (EntryIconPosition)(int)base.Args[0];
			}
		}
	}
	
	public delegate void IconPressReleaseEventHandler(System.Object o, IconPressReleaseEventArgs args);
	
	public enum EntryIconPosition : int
	{
		Primary = 0,
		Secondary = 1
	}
}
