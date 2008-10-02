// Icon.cs
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
using Gdk;
using Gtk;
using Platform.Common.Diagnostics;

namespace Basenji.Icons
{
	// all icons used in this app (except icons loaded by mime-type dynamically)
	public struct Icon
	{	
		// fields
		private string name;
		
		private Icon(string name) {
			this.name = name;
		}
		
		public string Name { get { return name; } }
		
		[NameInCustomIconTheme("properties.png")]
		public static Icon Stock_Properties			{ get { return new Icon(Stock.Properties);		  } }
		[NameInCustomIconTheme("find.png")]
		public static Icon Stock_Find				{ get { return new Icon(Stock.Find);			  } }
		[NameInCustomIconTheme("add.png")]
		public static Icon Stock_Add				{ get { return new Icon(Stock.Add);				  } }
		[NameInCustomIconTheme("remove.png")]
		public static Icon Stock_Remove				{ get { return new Icon(Stock.Remove);			  } }
		[NameInCustomIconTheme("new.png")]
		public static Icon Stock_New				{ get { return new Icon(Stock.New);				  } }
		[NameInCustomIconTheme("open.png")]
		public static Icon Stock_Open				{ get { return new Icon(Stock.Open);			  } }
		[NameInCustomIconTheme("preferences.png")]
		public static Icon Stock_Preferences		{ get { return new Icon(Stock.Preferences);		  } }
		[NameInCustomIconTheme("quit.png")]
		public static Icon Stock_Quit				{ get { return new Icon(Stock.Quit);			  } }
		[NameInCustomIconTheme("about.png")]
		public static Icon Stock_About				{ get { return new Icon(Stock.About);			  } }
		[NameInCustomIconTheme("drive-cdrom.png")]
		public static Icon Stock_Cdrom				{ get { return new Icon(Stock.Cdrom);			  } }
		[NameInCustomIconTheme("drive-harddisk.png")]
		public static Icon Stock_Harddisk			{ get { return new Icon(Stock.Harddisk);		  } }
		[NameInCustomIconTheme("drive-removable-media.png")]
		public static Icon DriveRemovableMedia		{ get { return new Icon("drive-removable-media"); } }
		[NameInCustomIconTheme("drive-network.png")]
		public static Icon Stock_Network			{ get { return new Icon(Stock.Network);			  } }
		[NameInCustomIconTheme("file.png")]
		public static Icon Stock_File				{ get { return new Icon(Stock.File);			  } }
		[NameInCustomIconTheme("directory.png")]
		public static Icon Stock_Directory			{ get { return new Icon(Stock.Directory);		} }
		[NameInCustomIconTheme("symbolic-link.png")]
		public static Icon SymLink					{ get { return new Icon("emblem-symbolic-link"); } }
		[NameInCustomIconTheme("clear.png")]
		public static Icon Stock_Clear				{ get { return new Icon(Stock.Clear);			  } }
		[NameInCustomIconTheme("cancel.png")]
		public static Icon Stock_Cancel				{ get { return new Icon(Stock.Cancel);			  } }
		[NameInCustomIconTheme("close.png")]
		public static Icon Stock_Close				{ get { return new Icon(Stock.Close);			 } }
		[NameInCustomIconTheme("dialog-info.png")]
		public static Icon Stock_DialogInfo			{ get { return new Icon(Stock.DialogInfo);		  } }
		[NameInCustomIconTheme("dialog-warning.png")]
		public static Icon Stock_DialogWarning		{ get { return new Icon(Stock.DialogWarning);	  } }
		[NameInCustomIconTheme("dialog-error.png")]
		public static Icon Stock_DialogError		{ get { return new Icon(Stock.DialogError);		  } }
		[NameInCustomIconTheme("dialog-question.png")]
		public static Icon Stock_DialogQuestion		{ get { return new Icon(Stock.DialogQuestion);	  } }
		
		public Pixbuf Render(Widget w, Gtk.IconSize size) {
			Pixbuf pb = w.RenderIcon(this.name, size, string.Empty);			
			
			if (pb == null) {
				try {				 
					pb = Gtk.IconTheme.Default.LoadIcon(this.name, IconUtils.GetIconSizeVal(size), 0);
				} catch (Exception) {
					Debug.WriteLine(string.Format("Icon.Render(): Gtk.IconTheme.Default.LoadIcon() threw a exception while trying to load icon \"{0}\"", this.name));
				}
			}
			return pb;
		}
	}	 
}