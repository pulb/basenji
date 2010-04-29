// ItemEditor.cs
// 
// Copyright (C) 2010 Patrick Ulbrich
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
using Basenji;
using Basenji.Gui.Base;
using VolumeDB;

namespace Basenji.Gui.Widgets.Editors
{
	public abstract partial class ItemEditor : ObjectEditor<VolumeItem>
	{
		private string	itemType;
		
		private Label	lblItemType;
		private Label	lblName;
		
		protected ItemEditor(string itemType) : base() {
			this.itemType = itemType;
		}
		
		public static ItemEditor CreateInstance(VolumeItemType itemType) {
			switch (itemType) {
				case VolumeItemType.FileVolumeItem:
					return new FileItemEditor();
				case VolumeItemType.DirectoryVolumeItem:
					return new DirectoryItemEditor();
				case VolumeItemType.AudioTrackVolumeItem:
					return new AudioTrackItemEditor();
				default:
					throw new NotImplementedException(string.Format("ItemEditor widget for VolumeItemType {0} is not implemented", itemType.ToString()));
			}
		}
		
		public VolumeItem VolumeItem { get { return Object; } }
		
		protected override void ValidateForm() {
		}
		
		protected override void SaveToObject(VolumeDB.VolumeItem item) {
			// save form
			item.Note = tvNote.Buffer.Text;
			item.Keywords = txtKeywords.Text.Trim();
			
			item.UpdateChanges();
		}
		
		protected override void LoadFromObject(VolumeDB.VolumeItem item) {
			//
			// form
			//
			tvNote.Buffer.Text = item.Note;
			txtKeywords.Text = item.Keywords;
			
			//
			// info labels
			//
			lblItemType.LabelProp = itemType;
			lblName.LabelProp = item.Name;
		}
		
		protected override void AddInfoLabels(List<InfoLabel> infoLabels) {
			lblItemType				= WindowBase.CreateLabel();			
			lblName					= WindowBase.CreateLabel();
			lblName.Ellipsize		= Pango.EllipsizeMode.End;
			
			infoLabels.AddRange( new InfoLabel[] {
				new InfoLabel(S._("Item type") + ":", lblItemType),
				new InfoLabel(S._("Name") + ":", lblName),
			} );
		}
	}
	
	// gui initialization
	public abstract partial class ItemEditor : ObjectEditor<VolumeItem>
	{
		private TextView	tvNote;
		private Entry		txtKeywords;
		
		protected override void CreateWidgetTbl(out Table tbl) {
			tbl = WindowBase.CreateTable(2, 2);
			
			// labels
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Note") + ":", false, 0F, 0F),	0, 0);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Keywords") + ":"),			0, 1);
			
			// widgets
			ScrolledWindow swNote = WindowBase.CreateScrolledTextView(out tvNote, VolumeItem.MAX_NOTE_LENGTH);
			// set min width of the scrolled window widget
			// (translated labels may make it smaller otherwise)
			swNote.WidthRequest = 280;
			txtKeywords = new Entry(VolumeItem.MAX_KEYWORDS_LENGTH);
			
			AttachOptions xAttachOpts = AttachOptions.Expand | AttachOptions.Fill | AttachOptions.Shrink;
			
			WindowBase.TblAttach(tbl, swNote, 1, 0, xAttachOpts, AttachOptions.Fill | AttachOptions.Expand);
			WindowBase.TblAttach(tbl, txtKeywords, 1, 1, xAttachOpts, AttachOptions.Fill);
			
			// events 
			tvNote.Buffer.Changed	+= ChangedEventHandler;
			txtKeywords.Changed		+= ChangedEventHandler;
		}
	}
}
