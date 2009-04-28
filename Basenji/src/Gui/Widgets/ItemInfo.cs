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
// of files rather than/additional to generic file info
using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using Basenji.Gui.Base;
using Basenji.Icons;
using VolumeDB;

namespace Basenji.Gui.Widgets
{
	public partial class ItemInfo : BinBase
	{
		private const int MAX_THUMB_WIDTH	= 100;
		private const int MAX_THUMB_HEIGHT	= 100;
		private const IconSize ICON_SIZE	= IconSize.Dialog;

		private ItemIcons itemIcons;
		private Dictionary<string, Gdk.Pixbuf> thumbnailCache;
		
		public ItemInfo() {
			BuildGui();
			itemIcons = new ItemIcons(this);
			thumbnailCache = new Dictionary<string, Gdk.Pixbuf>();
		}
		
		public void ShowInfo(VolumeItem item, VolumeDatabase db) {
			if (item == null)
				throw new ArgumentNullException("item");
			if (db == null)
				throw new ArgumentNullException("db");
			
			Table tbl;
			
			switch(item.GetVolumeItemType()) {
				case VolumeItemType.FileVolumeItem:
				case VolumeItemType.DirectoryVolumeItem:
					tbl = CreateFSInfoTable((FileSystemVolumeItem)item);
					break;
				default:
					throw new NotImplementedException("Iteminfo has not been implemented for this itemtype yet");
			}
			
			Box box = CreateBox(item, db, tbl);
			ShowChild(box);
		}
		
		public void Clear() {
			//if (this.Child != null)
			//	this.Remove(this.Child);
			Widget child = this.eventBox.Child; 
			if (child != null)
				this.eventBox.Remove(child);
		}
		
		private void ShowChild(Widget w) {
			Clear();
			//this.Add(w);
			this.eventBox.Add(w);
			this.ShowAll();			   
		}
	}
	
	// gui initialization
	public partial class ItemInfo : BinBase
	{
		private EventBox eventBox;
		
		protected override void BuildGui() {
			eventBox = new EventBox();
			eventBox.ModifyBg(Gtk.StateType.Normal, new Gdk.Color(255, 255, 255));
			Frame frame = new Frame();
			frame.Add(eventBox);
			this.Add(frame);
		}
		
		private static Table CreateFSInfoTable(FileSystemVolumeItem item) {
			bool isHashed = (item is FileVolumeItem) && !string.IsNullOrEmpty(((FileVolumeItem)item).Hash);

			Table tbl = WindowBase.CreateTable(5, 3);
			
			// caption labels
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("<b>Name:</b>"), true), 0, 0);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("<b>Location:</b>"), true), 0, 1);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("<b>Last write time:</b>"), true), 0, 2);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("<b>Size:</b>"), true), 0, 3);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("<b>Hash:</b>"), true), 0, 4);
			
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
				
				location = string.Format(S._("{0} <i>(link to {1}</i>)"), item.Location, targetPath);
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
		
		private Box CreateBox(VolumeItem item, VolumeDatabase db, Table tbl) {
			HBox box = new HBox(false, 12);
			box.BorderWidth = 6;
			
			Image img = new Image();
			Gdk.Pixbuf pb;
			
			img.WidthRequest	= MAX_THUMB_WIDTH;
			img.HeightRequest	= MAX_THUMB_HEIGHT;
			
			string thumbName = System.IO.Path.Combine(
				DbData.GetVolumeDataThumbsPath(db, item.VolumeID), 
				string.Format("{0}.png", item.ItemID));
				
			if (!thumbnailCache.TryGetValue(thumbName, out pb)) {
				if (File.Exists(thumbName)) {
					Gdk.Pixbuf newThumb = new Gdk.Pixbuf(thumbName); 
					pb = Resize(newThumb, MAX_THUMB_WIDTH, MAX_THUMB_HEIGHT);
					if (pb != newThumb)
						newThumb.Dispose();
					thumbnailCache.Add(thumbName, pb);
				} else {
					pb = itemIcons.GetIconForItem(item, ICON_SIZE);
				}
			}
			
			img.FromPixbuf = pb;
				
			box.PackStart(img, false, false, 0);
			box.PackStart(tbl, true, true, 0);
			return box;
		}
		
		private static Gdk.Pixbuf Resize(Gdk.Pixbuf original, int maxWidth, int maxHeight) {
			if (original.Width > maxWidth || original.Height > maxHeight) {
				// width or height is bigger than max
				int width, height;
				float resizeFactor;
				if (original.Width > original.Height) {
					// width > height => width is bigger than max width
					resizeFactor = (float)maxWidth / original.Width;
					width = maxWidth;
					height = (int)(original.Height * resizeFactor);					
				} else {
					// height >= width => height is bigger than max height
					resizeFactor = (float)maxHeight / original.Height;
					height = maxHeight;
					width = (int)(original.Width * resizeFactor);
				}
				return original.ScaleSimple(width, height, Gdk.InterpType.Bilinear);
			} else {
				return original;
			}
		}
	}
}
