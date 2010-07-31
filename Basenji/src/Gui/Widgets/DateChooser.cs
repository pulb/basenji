// DateChooser.cs
// 
// Copyright (C) 2008, 2010 Patrick Ulbrich
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
using System.Globalization;
using Gtk;
using Gdk;
using Basenji.Gui.Base;

namespace Basenji.Gui.Widgets
{	
	public class DateChooser : BinBase
	{
		private IconEntry		entry;
		private ToggleButton	btn;
		private Gtk.Window		win;
		private Gtk.Calendar	cal;
		
		private DateTime		date;
		private bool			validDate;
		
		private string			datePattern;
		
		public DateChooser(DateTime date) {
			BuildGui();
			this.datePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
			this.date = date;
			//validDate = true;
			SetDate(date);
		}
		
		public DateChooser() {
			BuildGui();
			this.datePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
			validDate = false;
		}
		
		public bool IsValid { get { return validDate; } }
		public bool IsEmpty { get { return (entry.Text.Trim().Length == 0); } }
		
		public DateTime Date {
			get {
				if (!IsValid)
					throw new FormatException("The date entered is not valid");
				if (IsEmpty)
					throw new FormatException("The date is empty");
				return date;
			}
			set {
				SetDate(value);			   
			}
		}
		
		public string DatePattern {
			get { return datePattern; }
			set { 
				if (value == null)
					throw new ArgumentNullException();
				if (value.Length == 0)
					throw new ArgumentException("The string must not be empty");
				
				datePattern = value;
			}
		}
		
		public void Clear() {
			entry.Text = string.Empty;
		}
		
		public event EventHandler Changed;
		
		protected virtual void OnChanged() {
			if (Changed != null)
				Changed(this, new EventArgs());		   
		}
		
		protected override void BuildGui() {
			entry = new IconEntry();
			entry.Changed += OnEntryChanged;
			
			if (entry.IconsSupported) {
				Pixbuf pb = Gdk.Pixbuf.LoadFromResource("Basenji.images.calendar.png");
				entry.SetIconFromPixbuf(pb, EntryIconPosition.Secondary);
				
				entry.IconPress	+= OnIconPressEvent;
				
				this.Add(entry);
			} else {
				// no icons inside entries supported ->
				// add a fallback button to the right of the entry
				HBox hbox = new HBox();
				hbox.Spacing = 2;
				
				hbox.PackStart(entry, true, true, 0);
				
				btn = new ToggleButton();
				btn.Add(new Arrow(ArrowType.Down, ShadowType.None));
				hbox.PackStart(btn, false, false, 0);
				
				this.Add(hbox);
				
				btn.Toggled += OnBtnToggled;
			}
		}
		
		private void ShowPopup() {
			win = new Gtk.Window(Gtk.WindowType.Popup);
			win.Screen = this.Screen;
			win.WidthRequest = this.Allocation.Width;

			cal = new Gtk.Calendar();
			win.Add(cal);
			
			if (validDate)
				cal.Date = date;
			
			// events
			win.ButtonPressEvent		+= OnWinButtonPressEvent; 
			cal.DaySelectedDoubleClick	+= OnCalDaySelectedDoubleClick;
			cal.KeyPressEvent			+= OnCalKeyPressEvent;
			cal.ButtonPressEvent		+= OnCalButtonPressEvent;
			
			int x, y;
			GetWidgetPos(this, out x, out y);
			win.Move(x, y + Allocation.Height + 2);
			win.ShowAll();
			win.GrabFocus();
			
			Grab.Add(win);
			
			Gdk.GrabStatus grabStatus;
			
			grabStatus = Gdk.Pointer.Grab(win.GdkWindow, true, EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.PointerMotionMask, null, null, Gtk.Global.CurrentEventTime);
			if (grabStatus == Gdk.GrabStatus.Success) {
				grabStatus = Gdk.Keyboard.Grab(win.GdkWindow, true, Gtk.Global.CurrentEventTime);
				if (grabStatus != Gdk.GrabStatus.Success) {
					Grab.Remove(win);
					win.Destroy();
					win = null;				   
				}
			} else {
				Grab.Remove(win);
				win.Destroy();
				win = null;			   
			}
			
		}
		
		private void ClosePopup(bool setDate) {
			if (win != null) {				  
				if (setDate) {	  
					//date = cal.Date;
					SetDate(cal.Date);
					//validDate = true;
				}
				
				Grab.Remove(win);
				Gdk.Pointer.Ungrab(Gtk.Global.CurrentEventTime);
				Gdk.Keyboard.Ungrab(Gtk.Global.CurrentEventTime);
				
				win.Destroy();
				win = null;
			}
			
			if (btn != null)
				btn.Active = false;		   
		}
		
		private static void GetWidgetPos(Gtk.Widget w, out int x, out int y) {
			w.ParentWindow.GetPosition(out x, out y);			 
			w.GdkWindow.GetOrigin(out x, out y);
			
			x += w.Allocation.X;
			y += w.Allocation.Y;
		}
		
		private void SetDate(DateTime d) {
			entry.Text = d.ToString(datePattern);		 
		}
		
		private void OnBtnToggled(object o, EventArgs args) {
			if (btn.Active)
				ShowPopup();
			else
				ClosePopup(false);
		}
		
		private void OnIconPressEvent(object o, IconPressReleaseEventArgs args) {
			ShowPopup();
		}
		
		private void OnEntryChanged(object o, EventArgs args) {
			validDate = DateTime.TryParseExact(entry.Text, datePattern, null, DateTimeStyles.None, out date);
			OnChanged();
		}
		
		private void OnWinButtonPressEvent(object o, ButtonPressEventArgs args) {
			// caught button press from outside the popup -> close popup			
			ClosePopup(false);		  
		}
		
		private void OnCalButtonPressEvent(object o, ButtonPressEventArgs args) {
			// cancel event, so the OnWinButtonPressEvent won't be triggered	
			args.RetVal = true;		   
		}
		
		private void OnCalDaySelectedDoubleClick(object o, EventArgs args) {
			ClosePopup(true);
		}
		
		private void OnCalKeyPressEvent(object o, KeyPressEventArgs args) {
			if (args.Event.Key == Gdk.Key.Return)
				ClosePopup(true);
			if (args.Event.Key == Gdk.Key.Escape)
				ClosePopup(false);
		}
	}
}
