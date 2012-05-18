// InfoBar.cs
// 
// Copyright (C) 2012 Patrick Ulbrich
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
using Basenji.Gui.Base;

namespace Basenji.Gui.Widgets
{
	// TODO : remove this class and 
	// use the native GtkInfoBar when it's available in Gtk#
	public partial class InfoBar : BinBase
	{
		private string headline;
		private string text;
		
		public InfoBar () {
			headline = string.Empty;
			text = string.Empty;
			
			BuildGui();
		}
		
		public string Headline {
			get {
				return headline;
			}
			
			set {
				headline = value;
				lblHeadline.Markup = string.Format("<b>{0}</b>", headline); 
			}
		}
		
		public string Text {
			get {
				return text;
			}
			
			set {
				text = value;
				lblText.Markup = text; 
			}
		}
	}
	
	public partial class InfoBar : BinBase
	{
		private Label lblHeadline;
		private Label lblText;
		
		protected override void BuildGui ()	{
			Gdk.Pixbuf pb = Basenji.Icons.Icon.Stock_DialogInfo.Render(this, IconSize.Dialog);
			Image img = new Image(pb);
			
			lblHeadline = WindowBase.CreateLabel(string.Empty, true);
			lblText = WindowBase.CreateLabel(string.Empty, true);
			
			VBox vbMsg = new VBox(false, 3);
			vbMsg.PackStart(lblHeadline, false, false, 0);
			vbMsg.PackStart(lblText, false, false, 0);
			
			HBox outerBox = new HBox(false, 12);
			outerBox.BorderWidth = 6;
			outerBox.PackStart(img, false, false, 0);
			outerBox.PackStart(vbMsg, true, true, 0);
			
			// (Gtk.Frame derives from Gtk.Bin which has no window)
			Frame frame = new Frame();
			frame.Add(outerBox);
				
			// the Eventbox is needed for the modified background color
			EventBox eb = new EventBox();
			eb.ModifyBg(StateType.Normal, new Gdk.Color(0xFC, 0xFC, 0xBD));
			eb.Add(frame);			
			
			this.Add(eb);
		}
	}
}

