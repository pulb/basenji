// ItemInfo.cs
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

// TODO : show meta info (e.g. mp3 bitrate, width/height of pictures...) 
// of files rather then generic file info
using System;
using Gtk;
using Basenji.Gui.Base;
using Basenji.Icons;
using VolumeDB;

namespace Basenji.Gui.Widgets
{
	public partial class ItemInfo : BinBase
	{
		private const IconSize ICON_SIZE = IconSize.Dialog;

		private ItemIcons itemIcons;
		
		public ItemInfo() {
			BuildGui();
			itemIcons = new ItemIcons(this);
		}
		
		public void ShowInfo(VolumeItem item) {
			if (item == null)
				throw new ArgumentNullException("item");

			Table tbl;
			
			switch(item.GetVolumeItemType()) {
				case VolumeItemType.FileVolumeItem:
				case VolumeItemType.DirectoryVolumeItem:
					tbl = CreateFSInfoTable((FileSystemVolumeItem)item);
					break;
				default:
					throw new NotImplementedException("Iteminfo has not been implemented for this itemtype yet");
			}
			
			Box box = CreateBox(item, tbl);
			ShowChild(box);
		}
		
		public void Clear() {
			if (this.Child != null)
				this.Remove(this.Child);
		}
		
		private void ShowChild(Widget w) {
			Clear();
			this.Add(w);
			this.ShowAll();			   
		}
	}
	
	// gui initialization
	public partial class ItemInfo : BinBase
	{
		protected override void BuildGui() {}
		
		private static Table CreateFSInfoTable(FileSystemVolumeItem item) {
			bool isHashed = (item is FileVolumeItem) && !string.IsNullOrEmpty(((FileVolumeItem)item).Hash);

			Table tbl = WindowBase.CreateTable(5, 3);
			
			// caption labels
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel("<b>Name:</b>", true), 0, 0);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel("<b>Location:</b>", true), 0, 1);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel("<b>Last write time:</b>", true), 0, 2);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel("<b>Size:</b>", true), 0, 3);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel("<b>Hash:</b>", true), 0, 4);
			
			// value labels
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(item.Name), 1, 0);
			
			string location;
			if (item.IsSymLink) {
				FileSystemVolumeItem targetItem = item.GetSymLinkTargetItem();
				
				string targetPath;
				if (targetItem.Location != "/" && targetItem.Name != "/")
					targetPath = string.Format("{0}/{1}", targetItem.Location, targetItem.Name);
				else
					targetPath = targetItem.Location + targetItem.Name;
				
				location = string.Format("{0} <i>(link to {1}</i>)", item.Location, targetPath);
			} else {
				location = item.Location;
			}

			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(location, true), 1, 1);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(item.LastWriteTime.ToString()), 1, 2);

			string sizeStr, hash;
			if (item is FileVolumeItem) {
				FileVolumeItem fvi = (FileVolumeItem)item;
				sizeStr = Util.GetSizeStr(fvi.Size);
				hash = fvi.Hash;
			} else {
				sizeStr = Util.GetSizeStr(0L);
				hash = null;
			}

			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(sizeStr), 1, 3);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(string.IsNullOrEmpty(hash) ? "-" : hash), 1, 4);
			
			return tbl;
		}
		
		private Box CreateBox(VolumeItem item, Table tbl) {
			HBox box = new HBox(false, 6);

			Image img = new Image();
			img.FromPixbuf = itemIcons.GetIconForItem(item, ICON_SIZE);
				
			box.PackStart(img, false, false, 0);
			box.PackStart(tbl, true, true, 0);
			return box;
		}
	}
}
