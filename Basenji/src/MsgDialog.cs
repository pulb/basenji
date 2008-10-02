// MsgDialog.cs
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

namespace Basenji
{
	
	public static class MsgDialog
	{		
		public static ResponseType Show(Window parentWindow, DialogFlags flags, MessageType type, ButtonsType bt, bool useMarkup, string title, string format, params object[] args) {
			if (args == null)
				args = new object[0];
			
			MessageDialog md = new MessageDialog(parentWindow, flags, type, bt, useMarkup, format, args);
			md.Title = title; 
			int result = md.Run();
			md.Destroy();
			return (ResponseType)result;
		}
		
		public static ResponseType Show(Window parentWindow, MessageType type, ButtonsType bt, string title, string format, params object[] args) {
			return Show(parentWindow, DialogFlags.Modal | DialogFlags.DestroyWithParent, type, bt, false, title, format, args);
		}
		
		public static ResponseType Show(Window parentWindow, MessageType type, ButtonsType bt, string title, string format) {
			return Show(parentWindow, type, bt, title, format, null);
		}
		
		public static void ShowError(Window parentWindow, string title, string format, params object[] args) {
			Show(parentWindow, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok, false, title, format, args);
		}
		
		public static void ShowError(Window parentWindow, string title, string format) {
			ShowError(parentWindow, title, format, null);
		}
	}
}
