// ItemInfo.cs
// 
// Copyright (C) 2008, 2009 Patrick Ulbrich
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
using System.Text;
using Gtk;
using Basenji.Gui.Base;
using Basenji.Icons;
using VolumeDB;

namespace Basenji.Gui.Widgets
{
	public partial class ItemInfo : BinBase
	{
		private const int MAX_THUMB_WIDTH		= 100;
		private const int MAX_THUMB_HEIGHT		= 100;
		private const IconSize ICON_SIZE		= IconSize.Dialog;
		
		private static readonly string STR_BY	= S._("by");
		private static readonly string STR_FROM	= S._("from");
		
		private ItemIcons itemIcons;
		private Dictionary<string, Gdk.Pixbuf> thumbnailCache;
		
		public ItemInfo() {
			BuildGui();
			itemIcons		= new ItemIcons(this);
			thumbnailCache	= new Dictionary<string, Gdk.Pixbuf>();
		}
		
		public void ShowInfo(VolumeItem item, VolumeDatabase db) {
			if (item == null)
				throw new ArgumentNullException("item");
			if (db == null)
				throw new ArgumentNullException("db");
			
			UpdateImage(item, db);
			FillPropertyBox(item);
			
			this.Show();
		}

		public void Clear() {
			img.Clear();
			propertyBox.Clear();
		}

		public bool Minimized {
			get {
				return !img.Visible;
			}
			set {
				img.Visible = !value;
				if (propertyBox.Minimized != value)
					propertyBox.Minimized = value;
			}
		}
		
		private void UpdateImage(VolumeItem item, VolumeDatabase db) {
			Gdk.Pixbuf pb;

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
		
		private void FillPropertyBox(VolumeItem item) {
			switch(item.GetVolumeItemType()) {
				case VolumeItemType.FileVolumeItem:
				case VolumeItemType.DirectoryVolumeItem:
					ItemProperty[] properties;
					Dictionary<string, string> nameProperty;
					
					ItemProperty.GetFSItemProperties(
				                                 (FileSystemVolumeItem)item, 
				                                 out properties, 
				                                 out nameProperty);
					
					propertyBox.SetProperties(properties);

					string tmp;
					if (nameProperty.TryGetValue("title", out tmp)) {
						// title found (e.g. html or doc file)
						// may be followed optionally by artist and/or album (audio file)
						StringBuilder sbName = new StringBuilder();
						StringBuilder sbTooltip = new StringBuilder();
				
						sbName.AppendFormat("<b>{0}</b>", Util.Escape(tmp));
						sbTooltip.Append(tmp);

						if (nameProperty.TryGetValue("artist", out tmp)) {								
							sbName.AppendFormat(" <i>{0}</i> {1}", STR_BY, Util.Escape(tmp));
							sbTooltip.AppendFormat(" {0} {1}", STR_BY, tmp);
						}

						if (nameProperty.TryGetValue("album", out tmp)) {
							sbName.AppendFormat(" <i>{0}</i> {1}", STR_FROM, Util.Escape(tmp));
							sbTooltip.AppendFormat(" {0} {1}", STR_FROM, tmp);
						}
							
						propertyBox.SetNameProperty(sbName.ToString(), sbTooltip.ToString());
					} else {
						// name expected
						if (!nameProperty.TryGetValue("name", out tmp))
							throw new ArgumentException("Name expected");
					
						propertyBox.SetNameProperty(string.Format("<b>{0}</b>", Util.Escape(tmp)), tmp);
					}
					
					break;
				default:
					throw new NotImplementedException("Iteminfo has not been implemented for this itemtype yet");
			}
		}

#region ItemProperty class
		// helper class that returns properties sorted by priority
		private class ItemProperty : IComparable<ItemProperty>
		{
			public string	name;
			public string	value;
			public int		priority;

			private static readonly string[] KEYWORD_SEPARATORS = { "; " };

			private ItemProperty(string name, string value, int priority) {
				this.name		= name;
				this.value		= value;
				this.priority	= priority;
			}
	
			public int CompareTo(ItemProperty p) {
				return (this.priority - p.priority);
			}
			
			public static void GetFSItemProperties(
			                                       FileSystemVolumeItem item, 
			                                       out ItemProperty[] properties, 
			                                       out Dictionary<string, string> nameProperty) {
				
				List<ItemProperty> tmp = new List<ItemProperty>();
				nameProperty = new Dictionary<string, string>();
				
				//
				// add metadata properties first (higher priority)
				//
				
				// check if metadata is present before parsing
				// (prevents unnecessary DllNotFoundExceptions 
				// if libextractor is not installed and there is no metadata anyway).
				if (!string.IsNullOrEmpty(item.MetaData)) {
					try {
					 	Dictionary<string, string> metadata = item.ParseMetaData();
						
						foreach (KeyValuePair<string, string> pair in metadata) {
					 		// cherry-pick interesting properties
							/* audio properties*/
							if (pair.Key == "genre") {
								tmp.Add(new ItemProperty(S._("Genre"), RemoveSimilarIDTags(pair.Value), 105));
							} else if (pair.Key == "artist") {
								//tmp2.Add(new ItemProperty(S._("Artist"), pair.Value, 101));
								nameProperty.Add("artist", RemoveSimilarIDTags(pair.Value));
							} else if (pair.Key == "title") {
								// NOTE: title keyword is used in e.g. html or doc files as well
								//tmp2.Add(new ItemProperty(S._("Title"), pair.Value, 101));
								nameProperty.Add("title", RemoveSimilarIDTags(pair.Value));
							} else if (pair.Key == "album") {
								//tmp2.Add(new ItemProperty(S._("Album"), pair.Value, 103));
								nameProperty.Add("album", RemoveSimilarIDTags(pair.Value));
							} else if (pair.Key == "year") {
								tmp.Add(new ItemProperty(S._("Year"), RemoveSimilarIDTags(pair.Value), 104));
							} else if (pair.Key == "description") {
								// NOTE: description keyword is used in e.g. html files as well
								tmp.Add(new ItemProperty(S._("Description"), RemoveSimilarIDTags(pair.Value), 110));
							/* audio / picture / video properties */
							} else if (pair.Key == "duration") {
								tmp.Add(new ItemProperty(S._("Duration"), pair.Value, 106));
							} else if (pair.Key == "size") {
								tmp.Add(new ItemProperty(S._("Dimensions"), pair.Value, 107));
							/* other properties*/
							} else if (pair.Key == "format") {
								tmp.Add(new ItemProperty(S._("Format"), pair.Value, 108));
							} else if (pair.Key == "author") {
								tmp.Add(new ItemProperty(S._("Author"), pair.Value, 111));
							} else if (pair.Key == "copyright") {
								tmp.Add(new ItemProperty(S._("Copyright"), pair.Value, 112));
							} else if (pair.Key == "producer") {
								tmp.Add(new ItemProperty(S._("Producer"), pair.Value, 115));
							} else if (pair.Key == "creator") {
								tmp.Add(new ItemProperty(S._("Creator"), pair.Value, 114));
							} else if (pair.Key == "software") {
								tmp.Add(new ItemProperty(S._("Software"), pair.Value, 116));
							} else if (pair.Key == "language") {
								tmp.Add(new ItemProperty(S._("Language"), pair.Value, 113));
							} else if (pair.Key == "page count") {
								tmp.Add(new ItemProperty(S._("Page count"), pair.Value, 109));
							} else if (pair.Key == "filename") {
								// count files in archives
								string[] filenames = pair.Value.Split(KEYWORD_SEPARATORS, StringSplitOptions.None);
								tmp.Add(new ItemProperty(S._("File count"), filenames.Length.ToString(), 117));
							}
	
#if DEBUG
						 	Platform.Common.Diagnostics.Debug.WriteLine(
								String.Format("{0}: {1}", pair.Key, pair.Value));
#endif
					 	}
					} catch (DllNotFoundException) { /* libextractor package not installed */}
				}

				//
				// add common item properties (low priority, shown only if there's room left)
				//
				
				if (!nameProperty.ContainsKey("title")) {
					// no title metadata
					// or artist and/or album metadata only
					nameProperty.Clear();
					// use name instead
					nameProperty.Add("name", item.Name);
				}
				
				tmp.Add(new ItemProperty(S._("Location"), item.Location, 202));
				tmp.Add(new ItemProperty(S._("Last write time"), item.LastWriteTime.ToString(), 205));
				
				if (item.IsSymLink) {
					FileSystemVolumeItem targetItem = item.GetSymLinkTargetItem();
					string symlinkTargetPath = null;
					
					if (targetItem.Location != "/" && targetItem.Name != "/")
						symlinkTargetPath = string.Format("{0}/{1}", targetItem.Location, targetItem.Name);
					else
						symlinkTargetPath = targetItem.Location + targetItem.Name;
				
					tmp.Add(new ItemProperty(S._("Symlink target"), symlinkTargetPath, 203));
				}
				
				if (item is FileVolumeItem) {
					FileVolumeItem fvi = (FileVolumeItem)item;
					string sizeStr = Util.GetSizeStr(fvi.Size);
					string hash = fvi.Hash;
	
					tmp.Add(new ItemProperty(S._("Size"), sizeStr, 204));
					if (!string.IsNullOrEmpty(hash))
						tmp.Add(new ItemProperty(S._("Hash"), hash, 207));
				}

				if (!string.IsNullOrEmpty(item.MimeType))
					tmp.Add(new ItemProperty(S._("Filetype"), item.MimeType, 206));

				tmp.Sort(); // sort by priority
				properties = tmp.ToArray();
			}

			private static string RemoveSimilarIDTags(string separatedTags) {
				// Dirty hack, tries to remove similar IDv2/IDv3 tag keywords,
				// i.e. tags that were equal in the IDv2 and the IDv3 header up to a certain length.
				// (e.g "MP3 Title Number 1" (IDv3 tag) and "MP3 Title Numb" (IDv2 tag)
				// In this case the shorter one is removed.				
				// Fully equal keywords can not occur, they have been filtered out in the scanning process.
				// NOTE: This function may fail if the keyword itself contains the separator string ("; ").
				
				if (separatedTags.IndexOf(KEYWORD_SEPARATORS[0]) > -1) {
					string[] tags = separatedTags.Split(KEYWORD_SEPARATORS, StringSplitOptions.None);
					if (tags.Length == 2) {
						if (tags[0].StartsWith(tags[1]))
							return tags[0];
						else if (tags[1].StartsWith(tags[0]))
							return tags[1];
					}
				}
				return separatedTags;
			}
		}
#endregion
	}
	
	// gui initialization
	public partial class ItemInfo : BinBase
	{
		private EventBox	eventBox;
		private HBox		outerBox;
		private Image		img;
		private PropertyBox	propertyBox;
		
		protected override void BuildGui() {
			eventBox = new EventBox();
			eventBox.ModifyBg(Gtk.StateType.Normal, new Gdk.Color(255, 255, 255));
			
			img = new Image();
			img.WidthRequest	= MAX_THUMB_WIDTH;
			img.HeightRequest	= MAX_THUMB_HEIGHT;

			propertyBox = new PropertyBox(this);
	
			outerBox = new HBox(false, 12);
			outerBox.BorderWidth = 6;
			outerBox.PackStart(img, false, false, 0);
			outerBox.PackStart(propertyBox, true, true, 0);
			
			eventBox.Add(outerBox);
			
			Frame frame = new Frame();
			frame.Add(eventBox);
			
			this.Add(frame);
		}
		
#region PropertyBox class
		private class PropertyBox : VBox
		{
			private Label		lblName;
			private Arrow		btnArrow;
			private Table[]		tbls;
			private HBox		hbox;
			private Label[] 	captionLbls;
			private Label[]		valueLbls;
			private ItemInfo	owner;
			
			private const int MAX_ITEM_PROPERTIES	= 8; // must be a multiple of 2
			private const int WIDTH					= 2;
			private const int HEIGHT				= MAX_ITEM_PROPERTIES / WIDTH;
			
			public PropertyBox(ItemInfo owner) : base(false, 12) {
				this.owner = owner;
				
				this.captionLbls	= new Label[MAX_ITEM_PROPERTIES];
				this.valueLbls		= new Label[MAX_ITEM_PROPERTIES];
				
				this.lblName = WindowBase.CreateLabel(string.Empty, true);
				this.lblName.Ellipsize = Pango.EllipsizeMode.End;

				this.tbls = new Table[WIDTH];
				this.hbox = new HBox();
				
				for (int i = 0; i < WIDTH; i++) {
					this.tbls[i] = WindowBase.CreateTable(HEIGHT, 2); // 2 = caption + value
					this.hbox.PackStart(tbls[i], true, true, 0);
				}
	
				int tbl = 0, y = 0;
				for (int i = 0; i < MAX_ITEM_PROPERTIES; i++, y++) {
	
					if (i == HEIGHT) {
						y = 0;
						tbl++;
					}
					
					this.captionLbls[i] = WindowBase.CreateLabel(string.Empty, true);
					this.captionLbls[i].Ellipsize = Pango.EllipsizeMode.End;
						
					this.valueLbls[i] = WindowBase.CreateLabel(string.Empty, false);
					this.valueLbls[i].Ellipsize = Pango.EllipsizeMode.End;
	
					this.tbls[tbl].Attach(captionLbls[i], 0, 1, (uint)y, (uint)(y + 1));
					this.tbls[tbl].Attach(valueLbls[i], 1, 2, (uint)y, (uint)(y + 1));
				}

				// button
				HBox hb = new HBox(false, 6);
				this.btnArrow = new Arrow(ArrowType.Down, ShadowType.None);
				hb.PackStart(btnArrow, false, false, 0);
				hb.PackStart(lblName, true, true, 0);
				
				Button btn = new Button(hb);
				btn.Clicked += OnBtnClicked;
				
				this.PackStart(btn, true, false, 0);
				this.PackStart(hbox, true, true, 0);
			}
	
			public void SetNameProperty(string name, string tooltip) {
				lblName.LabelProp = name;
				lblName.TooltipText = tooltip;
			}
	
			public void SetProperties(ItemProperty[] properties) {
				int itemCount = (properties.Length < MAX_ITEM_PROPERTIES) ? properties.Length : MAX_ITEM_PROPERTIES;
				
				// hide second table when it won't contain labels
				tbls[1].Visible = (itemCount > HEIGHT);
				
				for (int i = 0; i < itemCount; i++) {
					ItemProperty p = properties[i];
					
					captionLbls[i].LabelProp = string.Format("<b>{0}:</b>", p.name);
					captionLbls[i].TooltipText = p.name;
					
					// remove linebreaks
					valueLbls[i].LabelProp = p.value.Replace('\n', ' ').Replace('\r', ' ');
					valueLbls[i].TooltipText = p.value;
				}
				
				// clear remaining labels
				for (int i = itemCount; i < MAX_ITEM_PROPERTIES; i++) {
					captionLbls[i].LabelProp = string.Empty;
					captionLbls[i].TooltipText = string.Empty;
					valueLbls[i].LabelProp = string.Empty;
					valueLbls[i].TooltipText = string.Empty;
				}
			}

			public void Clear() {
				SetNameProperty(string.Empty, string.Empty);
				for (int i = 0; i < MAX_ITEM_PROPERTIES; i++) {
					captionLbls[i].LabelProp = captionLbls[i].TooltipText = String.Empty;
					valueLbls[i].LabelProp = valueLbls[i].TooltipText = String.Empty;
				}
			}

			public bool Minimized {
				get {
					return !hbox.Visible;
				}
				set {					
					if (value) {
						btnArrow.ArrowType = ArrowType.Right;
						hbox.Visible = false;
					} else {
						btnArrow.ArrowType = ArrowType.Down;
						hbox.Visible = true;
					}
					
					if (owner.Minimized != value)
						owner.Minimized = value;
				}
			}
			
			private void OnBtnClicked(object sender, System.EventArgs args) {
				Minimized = !Minimized;
			}
		}
#endregion

	}
}
