// SearchResultView.cs
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
using Basenji.Gui.Base;
using Basenji.Icons;
using VolumeDB;

namespace Basenji.Gui.Widgets
{
	public class SearchResultView : ViewBase
	{	
		private const IconSize ICON_SIZE = IconSize.Button;
		
		private ItemIcons itemIcons;
		private Dictionary<long, Volume> volumeCache;
		
		public SearchResultView() {
			HeadersVisible = true;
			itemIcons = new ItemIcons(this);
			volumeCache = new Dictionary<long,Volume>();
			
			//
			// setup columns
			//
			TreeViewColumn col;
				
			// icon/name			
			CellRendererPixbuf pix = new CellRendererPixbuf();
			CellRendererText txt = new CellRendererText();
			col = new TreeViewColumn();
			col.SortColumnId = 1;
			col.Resizable = true;
			col.Title = S._("Name");
			col.MaxWidth = 300;
			col.PackStart(pix, false);
			col.PackStart(txt, false);
			col.SetAttributes(pix, "pixbuf", 0);
			col.SetAttributes(txt, "text", 1);
			
			AppendColumn(col);
			
			// location
			col = new TreeViewColumn(S._("Location"), new CellRendererText(), "text", 2);
			col.SortColumnId = 2;
			col.Resizable = true;			 
			col.MaxWidth = 320;
			AppendColumn(col);
			
			// volumename
			col = new TreeViewColumn(S._("Volume"), new CellRendererText(), "text", 3);
			col.SortColumnId = 3;
			col.Resizable = true;			 
			col.MaxWidth = 100;				
			AppendColumn(col);
			
			// archive nr
			col = new TreeViewColumn(S._("Archive Nr."), new CellRendererText(), "text", 4);
			col.SortColumnId = 4;
			col.Resizable = true;			 
			col.MaxWidth = 100;
			AppendColumn(col);
		}
		
		public void Fill(VolumeItem[] items) {
			ListStore store = new ListStore(
											typeof(Gdk.Pixbuf),
											typeof(string),
											typeof(string),
											typeof(string),
											typeof(string),
											/* VolumeItem - not visible */ typeof(VolumeItem)
											);
			
			foreach(VolumeItem item in items) {
				Volume vol;
						
				if (!volumeCache.TryGetValue(item.VolumeID, out vol)) {
					vol = item.GetOwnerVolume();
					volumeCache.Add(vol.VolumeID, vol);						   
				}
				
				switch (item.GetVolumeItemType()) {
					case VolumeItemType.FileVolumeItem:					   
					case VolumeItemType.DirectoryVolumeItem:
						
						FileSystemVolumeItem fsvi = (FileSystemVolumeItem)item;
						store.AppendValues(itemIcons.GetIconForItem(fsvi, ICON_SIZE), fsvi.Name, fsvi.Location, vol.Title, vol.ArchiveNr, fsvi);
						break;
					//case VolumeItemType.CDDAVolumeItem
					//	  ...
					//	  break;
					default:
						throw new NotImplementedException("Search result view has not been implemented for this volumetype");
				}
			}
			
			this.Model = store;
		}
		
		public void Clear() {
			if (Model != null) {
				ListStore store = (ListStore)Model;
				store.Clear();
			}
		}
		
		public VolumeItem GetItem(TreeIter iter) {
			VolumeItem item = (VolumeItem)Model.GetValue(iter, 5);
			return item;
		}
	}
}