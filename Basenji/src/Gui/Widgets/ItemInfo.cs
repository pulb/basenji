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
		private const int MAX_ITEM_PROPERTIES	= 12; // must be a multiple of 2
		private const int MAX_THUMB_WIDTH		= 100;
		private const int MAX_THUMB_HEIGHT		= 100;
		private const IconSize ICON_SIZE		= IconSize.Dialog;

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
			ItemProperty[] properties = ItemProperty.GetFSItemProperties(item);

			int itemCount = (properties.Length < MAX_ITEM_PROPERTIES) ? properties.Length : MAX_ITEM_PROPERTIES;
			int hCount = 2;
			int vCount = MAX_ITEM_PROPERTIES / hCount;
			if (itemCount <= vCount)
				hCount = 1;

			Table tbl = WindowBase.CreateTable(vCount, hCount * 2);

			int x = 0, y = 0;
			for(int i = 0; i < itemCount; i++, y++) {
				ItemProperty p = properties[i];

				if (i == vCount) {
					y = 0;
					x+= 2;
				}
				
				AttachTooltipLabel(String.Format("{0}:", p.name), "b", tbl, x, y);
				AttachTooltipLabel(p.value, null, tbl, x + 1, y);
			}
			return tbl;
		}
		
		private static void AttachTooltipLabel(string caption, string tag, Table tbl, int x, int y) {
			Label lbl;

			if (!string.IsNullOrEmpty(tag))
				lbl = WindowBase.CreateLabel(String.Format("<{0}>{1}</{0}>", tag, caption), true);
			else
				lbl = WindowBase.CreateLabel(caption, false);
			
			lbl.Ellipsize = Pango.EllipsizeMode.End;
			lbl.TooltipText = caption;
			
			tbl.Attach(lbl, (uint)x, (uint)(x + 1), (uint)y, (uint)(y + 1));
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

		private class ItemProperty : IComparable<ItemProperty>
		{
			public string	name;
			public string	value;
			public int		priority;
			
			private ItemProperty(string name, string value, int priority) {
				this.name		= name;
				this.value		= value;
				this.priority	= priority;
			}

			public int CompareTo(ItemProperty p) {
				return (this.priority - p.priority);
			}
			
			public static ItemProperty[] GetFSItemProperties(FileSystemVolumeItem item) {
				List<ItemProperty> properties = new List<ItemProperty>();
				
				// add metadata properties first (higher priority)
				try {
				 	Dictionary<string, string> metadata = item.ParseMetaData();
					
					foreach (KeyValuePair<string, string> pair in metadata) {
				 		// cherry-pick interesting properties
						/* audio properties*/
						if (pair.Key == "genre") {
							properties.Add(new ItemProperty(S._("Genre"), pair.Value, 105));
						} else if (pair.Key == "artist") {
							properties.Add(new ItemProperty(S._("Artist"), pair.Value, 101));
						} else if (pair.Key == "title") {
							properties.Add(new ItemProperty(S._("Title"), pair.Value, 102));
						} else if (pair.Key == "album") {
							properties.Add(new ItemProperty(S._("Album"), pair.Value, 103));
						} else if (pair.Key == "year") {
							properties.Add(new ItemProperty(S._("Year"), pair.Value, 104));
						/* audio / picture / video properties */
						} else if (pair.Key == "duration") {
							properties.Add(new ItemProperty(S._("Duration"), pair.Value, 106));
						} else if (pair.Key == "size") {
							properties.Add(new ItemProperty(S._("Dimensions"), pair.Value, 107));
						/* html properties */
						} else if (pair.Key == "description") {
							properties.Add(new ItemProperty(S._("Description"), pair.Value, 110));
						} else if (pair.Key == "author") {
							properties.Add(new ItemProperty(S._("Author"), pair.Value, 111));
						/* other properties*/
						} else if (pair.Key == "format") {
							properties.Add(new ItemProperty(S._("Format"), pair.Value, 108));
						} else if (pair.Key == "copyright") {
							properties.Add(new ItemProperty(S._("Copyright"), pair.Value, 112));
						} else if (pair.Key == "producer") {
							properties.Add(new ItemProperty(S._("Producer"), pair.Value, 115));
						} else if (pair.Key == "creator") {
							properties.Add(new ItemProperty(S._("Creator"), pair.Value, 114));
						} else if (pair.Key == "software") {
							properties.Add(new ItemProperty(S._("Software"), pair.Value, 116));
						} else if (pair.Key == "language") {
							properties.Add(new ItemProperty(S._("Language"), pair.Value, 113));
						} else if (pair.Key == "page count") {
							properties.Add(new ItemProperty(S._("Page count"), pair.Value, 109));
						} else if (pair.Key == "filename") {
							// count files in archives
							string[] filenames = pair.Value.Split(new char[] { ',' });
							properties.Add(new ItemProperty(S._("File count"), filenames.Length.ToString(), 117));
						}

#if DEBUG
					 Platform.Common.Diagnostics.Debug.WriteLine(
						String.Format("{0}: {1}", pair.Key, pair.Value));
#endif
				 	}
				} catch(DllNotFoundException) { /* libextractor package not installed */}
	
				// add common item properties (shown only if there's room left)
				properties.Add(new ItemProperty(S._("Name"), item.Name, 201));
				properties.Add(new ItemProperty(S._("Location"), item.Location, 202));
				properties.Add(new ItemProperty(S._("Last write time"), item.LastWriteTime.ToString(), 205));
				
				if (item.IsSymLink) {
					FileSystemVolumeItem targetItem = item.GetSymLinkTargetItem();
					string symlinkTargetPath = null;
					
					if (targetItem.Location != "/" && targetItem.Name != "/")
						symlinkTargetPath = string.Format("{0}/{1}", targetItem.Location, targetItem.Name);
					else
						symlinkTargetPath = targetItem.Location + targetItem.Name;
				
					properties.Add(new ItemProperty(S._("Symlink target"), symlinkTargetPath, 203));
				}
				
				if (item is FileVolumeItem) {
					FileVolumeItem fvi = (FileVolumeItem)item;
					string sizeStr = Util.GetSizeStr(fvi.Size);
					string hash = fvi.Hash;

					properties.Add(new ItemProperty(S._("Size"), sizeStr, 204));
					if (!string.IsNullOrEmpty(hash))
						properties.Add(new ItemProperty(S._("Hash"), hash, 206));
				}
				
				properties.Sort(); // sort by priority
				return properties.ToArray();
			}
		}
	}
}
