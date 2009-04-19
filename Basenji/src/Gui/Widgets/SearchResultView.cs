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
		private const IconSize ICON_SIZE = IconSize.Dialog;
		
		private ItemIcons itemIcons;
		private Dictionary<long, Volume> volumeCache;
		
		public SearchResultView() {
			itemIcons = new ItemIcons(this);
			volumeCache = new Dictionary<long,Volume>();
			
			//
			// setup columns
			//
			const int MIN_COL_WIDTH = 400;
			
			TreeViewColumn col;
			
			col = new TreeViewColumn(string.Empty, new CellRendererPixbuf(), "pixbuf", 0);			
			col.MinWidth = 48; // TODO : adjust to icon size
			AppendColumn(col);
 
			col = new TreeViewColumn(string.Empty, new CellRendererText(), "markup", 1);
			//col.SortColumnId = (int)Columns.Id;
			col.MinWidth = MIN_COL_WIDTH;
			AppendColumn(col);
		}
		
		public void Fill(VolumeItem[] items) {
			if (items == null)
				throw new ArgumentNullException("items");
				
			ListStore store = new Gtk.ListStore(	typeof(Gdk.Pixbuf),
													typeof(string),
													typeof(VolumeItem)); /* VolumeItem - not visible */
			
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
						string description = string.Format(S._("<b>{0}</b>\n<span size=\"smaller\"><i>Location:</i> {1}\n<i>Volume:</i> {2}, <i>Archive No.:</i> {3}</span>"),
																fsvi.Name,
																fsvi.Location,
																vol.Title, 
																vol.ArchiveNr);
																
						store.AppendValues(	itemIcons.GetIconForItem(fsvi, ICON_SIZE),
											description,
											fsvi);
						
						break;
					//case VolumeItemType.CDDAVolumeItem
					//	  ...
					//	  break;
					default:
						throw new NotImplementedException("Search result view has not been implemented for this volumetype");
				}
			}
			
			this.Model = store;
			ColumnsAutosize();			
		}
		
		public void Clear() {
			if (Model != null) {
				ListStore store = (ListStore)Model;
				store.Clear();
			}
		}
		
		public VolumeItem GetItem(TreeIter iter) {
			VolumeItem item = (VolumeItem)Model.GetValue(iter, 2);
			return item;
		}
	}
}