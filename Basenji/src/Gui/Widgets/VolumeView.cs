// VolumeView.cs
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
using Platform.Common.Globalization;
using Basenji.Gui.Base;
using Basenji.Icons;
using VolumeDB;

namespace Basenji.Gui.Widgets
{
	public class VolumeView : ViewBase
	{
		private const IconSize ICON_SIZE = IconSize.Dialog;
		private IconCache iconCache;
		
		public VolumeView() {
			iconCache = new IconCache(this);
			
			// init columns
			const int MIN_COL_WIDTH = 80;
			
			TreeViewColumn col;
			
			col = new TreeViewColumn(string.Empty, new CellRendererPixbuf(), "pixbuf", 0);			
			col.MinWidth = 30; // TODO : adjust to icon size
			AppendColumn(col);
 
			col = new TreeViewColumn(string.Empty, new CellRendererText(), "markup", 1);
			//col.SortColumnId = (int)Columns.Id;
			col.MinWidth = MIN_COL_WIDTH;
			AppendColumn(col);
		}

		public void Fill(Volume[] volumes) {
			if (volumes == null)
				throw new ArgumentNullException("volumes");

			ListStore store = new Gtk.ListStore(typeof(Gdk.Pixbuf), typeof(string), /* Volume - not visible */ typeof(Volume));
			
			foreach(Volume v in volumes)
				AddVolume(store, v);

			Model = store;
		}
		
		public void Clear() {
			if (Model != null) {
				ListStore store = (ListStore)Model;
				store.Clear();
			}
		}
		
		public Volume GetVolume(TreeIter iter) {
			Volume v = (Volume)Model.GetValue(iter, 2);
			return v;
		}
		
		public void RemoveVolume(TreeIter iter) {
			Remove(iter);  
		}
		
		public void UpdateVolume(TreeIter iter, Volume volume) {
			//Model.SetValue(iter, 0, GetVolumeIcon(volume);
			Model.SetValue(iter, 1, GetVolumeDescription(volume));
		}
		
		public void AddVolume(Volume v) {
			AddVolume((ListStore)this.Model, v);
		}
				
		private void AddVolume(ListStore store, Volume v) {
			store.AppendValues(GetVolumeIcon(v), GetVolumeDescription(v), v);
		}
		
		private static string GetVolumeDescription(Volume v) {
			switch (v.GetVolumeType()) {
				case VolumeType.FileSystemVolume:
					FileSystemVolume fsv = (FileSystemVolume)v;
					// TODO: add colors. add ArchivNr. only show important info, otherwise its too gluttered, too high!)
					return string.Format(S._("<b>{0}</b>\nCategory: {1}\nAdded: {2}\nFiles: {3}\nSize: {4}"), v.Title, v.Category, v.Added.ToShortDateString(), fsv.Files.ToString(), Util.GetSizeStr(fsv.Size));
					break;
				//case VolumeType.Cdda: ...
				default:
					throw new NotImplementedException("Description not implemented for this VolumeType");
			}
		}
		
		private Gdk.Pixbuf GetVolumeIcon(Volume v) {
			Gdk.Pixbuf icon;
			
			switch (v.DriveType) {
				case VolumeDriveType.CDRom:
					icon = iconCache.GetIcon(Icons.Icon.Stock_Cdrom, ICON_SIZE);
					break;
				case VolumeDriveType.Harddisk:
					icon = iconCache.GetIcon(Icons.Icon.Stock_Harddisk, ICON_SIZE);
					break;
				case VolumeDriveType.Ram:
					icon = iconCache.GetIcon(Icons.Icon.Stock_Harddisk, ICON_SIZE); // FIXME : is there a more suitable icon?
					break;
				case VolumeDriveType.Network:
					icon = iconCache.GetIcon(Icons.Icon.Stock_Network, ICON_SIZE);
					break;
				case VolumeDriveType.Removable:
					icon = iconCache.GetIcon(Icons.Icon.DriveRemovableMedia, ICON_SIZE);
					break;
				case VolumeDriveType.Unknown:
					icon = iconCache.GetIcon(Icons.Icon.Stock_Harddisk, ICON_SIZE); // FIXME : is there a more suitable icon?
					break;
			   default:
				   throw new Exception("Invalid VolumeDriveType");
			}
			
			return icon;
		}
	}
}