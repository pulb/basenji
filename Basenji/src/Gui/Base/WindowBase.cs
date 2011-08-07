// WindowBase.cs
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
using Gdk;
	
namespace Basenji.Gui.Base
{	
	public abstract class WindowBase : Gtk.Window
	{	
		public WindowBase() : base(Gtk.WindowType.Toplevel) {}

		public static Gtk.Window MainWindow {
			get; set;
		}
		
		protected virtual void BuildGui() {
			if ((MainWindow != null) && (this != MainWindow))
				this.TransientFor = MainWindow;
			
			this.BorderWidth	= 0;
			this.WindowPosition = WindowPosition.Center;
			this.Icon			= App.DefaultWindowIcon;
		}
		
		protected void SetModal() {
			this.Modal = true;
			/* TODO : somehow disable the minimize button on modal windows 
			   when Gtk supports it. */
			// this.SkipTaskbarHint	= true;
		}
		
		/*
		protected void ShowAll() {
			if (this.Child != null)
				this.Child.ShowAll();
			
			this.Show();		
		}*/
		
		internal static Gtk.Action CreateAction(string name,
		                                        string label,
		                                        string tooltip,
		                                        string stockid,
		                                        EventHandler activated) {
			
			Gtk.Action a = new Gtk.Action(name, label, tooltip, stockid);
			if (activated != null)			  
				a.Activated += activated;

			return a;
		}
		
		internal static Gtk.RadioAction CreateRadioAction(string name,
		                                                  string label,
		                                                  string tooltip,
		                                                  string stockid,
		                                                  int value,
		                                                  GLib.SList grp,
		                                                  EventHandler activated) {
			
			Gtk.RadioAction a = new Gtk.RadioAction(name, label, tooltip, stockid, value);
			
			if (grp != null)
				a.Group = grp;
			
			if (activated != null)
				a.Activated += activated;

			return a;
		}
		
		internal static Gtk.ToggleAction CreateToggleAction(string name,
		                                                    string label,
		                                                    string tooltip,
		                                                    string stockid,
		                                                    EventHandler activated) {
			
			Gtk.ToggleAction a = new Gtk.ToggleAction(name, label, tooltip, stockid);
			if (activated != null)			  
				a.Activated += activated;

			return a;
		}
		
		internal static Label CreateLabel() { return CreateLabel(string.Empty); }
		internal static Label CreateLabel(string caption) { return CreateLabel(caption, false); }
		internal static Label CreateLabel(string caption, bool useMarkup) { return CreateLabel(caption, useMarkup, 0.0f, 0.5f); }
		internal static Label CreateLabel(string caption, bool useMarkup, float xalign, float yalign) {
			Label lbl = new Label();
			lbl.UseMarkup = useMarkup;
			lbl.Xalign = xalign;
			lbl.Yalign = yalign;
			lbl.LabelProp = caption;
			
			return lbl;
		}
		
		internal static Button CreateButton(string label, bool useStock, EventHandler clicked) {
			Button btn = new Button();
			//btn.CanFocus = true;
			btn.UseUnderline = true;
			btn.UseStock = useStock;
			btn.Label = label;
			if (clicked != null)
				btn.Clicked += clicked;
			
			return btn;
		}
		
		internal static Button CreateCustomButton(Pixbuf pixbuf, string label, EventHandler clicked) {
			Button btn = new Button();
			btn.UseUnderline = true; 
			if (clicked != null)			
				btn.Clicked += clicked;
			
			Alignment algn = new Alignment(0.5f, 0.5f, 0f, 0f);
			
			HBox box = new HBox();
			box.Spacing = 6;
			
			Gtk.Image img = new Gtk.Image();
			img.Pixbuf = pixbuf;
			
			box.PackStart(img, false, false, 0);
			
			if (!string.IsNullOrEmpty(label)) {			   
				Label lbl = new Label();
				lbl.UseUnderline = true;
				lbl.LabelProp = label;
				
				box.PackStart(lbl, false, false, 0);
			}
			
			algn.Add(box);
			btn.Add(algn);
			
			return btn;
		}
		
		internal static ScrolledWindow CreateScrolledView<V>(out V tv, bool rulesHint) 
		where V : TreeView, new() {
			// scrolled window
			ScrolledWindow sw = new ScrolledWindow();
			//sw.CanFocus = true;
			sw.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.ShadowType = Gtk.ShadowType.In;
			
			// treeview
			tv = new V();
			//tv.CanFocus = true;
			tv.RulesHint = rulesHint;
			tv.HeadersClickable = true;
			
			sw.Add(tv);
			
			return sw;
		}
		
		internal static ScrolledWindow CreateScrolledTextView(out TextView txt, int maxChars) {
			// scrolled window
			ScrolledWindow sw = new ScrolledWindow();
			//sw.CanFocus = true;
			sw.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.ShadowType = Gtk.ShadowType.In;
			
			// textview
			txt = new TextView();
			//tv.CanFocus = true;
			if (maxChars > 0) {
				txt.Buffer.InsertText += delegate(object o, InsertTextArgs e) {
					// TODO : is there a smarter way to cancel the last typed char?
					TextBuffer buff = (TextBuffer)o;					 
					if (buff.CharCount > maxChars)
						buff.Text = buff.Text.Substring(0, buff.Text.Length - e.Length);
				};
			}
			
			sw.Add(txt);
			
			return sw;
		}
		
		// default to 12 pix columnspacing (between label and widget, as proposed in the HIG)
		internal static Table CreateTable(int rows, int cols) { return CreateTable(rows, cols, 12); }
		internal static Table CreateTable(int rows, int cols, int colSpacing) {
			Table tbl = new Table((uint)rows, (uint)cols, false);
			tbl.RowSpacing = 6;
			tbl.ColumnSpacing = (uint)colSpacing;			   
			return tbl;
		}
		
		internal static void TblAttach(Table tbl, Widget w, int x, int y) { TblAttach(tbl, w, x, y, 1, 1, AttachOptions.Fill, AttachOptions.Fill); }
		internal static void TblAttach(Table tbl, Widget w, int x, int y, AttachOptions xoptions, AttachOptions yoptions) { TblAttach(tbl, w, x, y, 1, 1, xoptions, yoptions); }	   
		internal static void TblAttach(Table tbl, Widget w, int x, int y, int width, int height) { TblAttach(tbl, w, x, y, width, height, AttachOptions.Fill, AttachOptions.Fill); }		
		internal static void TblAttach(Table tbl, Widget w, int x, int y, int width, int height, AttachOptions xoptions, AttachOptions yoptions) {
			tbl.Attach(w, (uint)x, (uint)(x + width), (uint)y, (uint)(y + height), xoptions, yoptions, (uint)0, (uint)0);
		}
		
		internal static Widget LeftAlign(Widget w) { return LeftAlign(w, 24); }
		internal static Widget LeftAlign(Widget w, int leftPadding) {
			Alignment algn = new Alignment(0.5F, 0.5F, 1F, 1F);
			algn.LeftPadding = (uint)leftPadding;
			algn.Add(w);
			return algn;
		}
		
		protected Gdk.Pixbuf RenderIcon(Basenji.Icons.Icon icon, Gtk.IconSize size) {
			return icon.Render(this, size);
		}
	}
}
