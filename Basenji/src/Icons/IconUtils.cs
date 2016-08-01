// IconUtils.cs
// 
// Copyright (C) 2008, 2016 Patrick Ulbrich
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
using Platform.Common.IO;
//using Platform.Common.Diagnostics;

namespace Basenji.Icons
{
	public static class IconUtils
	{
		public static int GetIconSizeVal(IconSize size) {
			int w, h;
			Gtk.Icon.SizeLookup(size, out w, out h);
			return w;
		}
		
		// keep in sync with VolumeView.GetVolumeIcon()
		public static Icons.Icon GetDriveIcon(DriveInfo d) { 
			
			//// DriveInfo.DriveType is not supported on linux
			//if (CurrentPlatform.IsUnix)
			//	  return Stock.Harddisk;
			
			Icons.Icon icon;
			switch(d.DriveType) {
				case DriveType.CDRom:
					//name = Gtk.Stock.Cdrom;
					icon = Icons.Icon.Stock_Cdrom;
					break;
				case DriveType.Fixed:
					//name = Gtk.Stock.Harddisk;
					icon = Icons.Icon.Stock_Harddisk;
					break;
				case DriveType.Ram:
					//name = Gtk.Stock.Harddisk; // FIXME : is there a more suitable icon?
					icon = Icons.Icon.Stock_Harddisk; // FIXME : is there a more suitable icon?
					break;
				case DriveType.Network:
					//name = Gtk.Stock.Network;
					icon = Icons.Icon.Stock_Network;
					break;
				case DriveType.Removable:
					//name = "drive-removable-media";
					icon = Icons.Icon.DriveRemovableMedia;
					break;
				case DriveType.Unknown:
					//name = Gtk.Stock.Harddisk; // FIXME : is there a more suitable icon?
					icon = Icons.Icon.Stock_Harddisk; // FIXME : is there a more suitable icon?
					break;
				default:
					throw new Exception("Invalid DriveType.");
			}
			return icon;
		}
	}
}
