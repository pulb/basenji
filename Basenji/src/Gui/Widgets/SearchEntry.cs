/// SearchEntry.cs
// 
// Copyright (C) 2009 - 2012 Patrick Ulbrich
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
using GLib;
using Basenji.Icons;

namespace Basenji.Gui.Widgets
{
	public class SearchEntryPreset
	{
		public string Caption { get; set; }
		public string Value { get; set; }		
		public string Suggestion { get; set; }
		
		public SearchEntryPreset() {
			Caption = Value = Suggestion = null;
		}
		
		public SearchEntryPreset(string caption, 
		                             string value, 
		                             string suggestion) {
			Caption = caption;
			Value = value;
			Suggestion = suggestion;
		}
	}
	
	public class SearchEntry : IconEntry
	{	
		private string placeholderText;
		private SearchEntryPreset[] presets;
		private bool presetsChanged;
		private Gtk.Menu popup;
		
		public SearchEntry () {
			this.placeholderText = null;
			this.presets = null;
			this.presetsChanged = false;
			this.popup = null;
			this.ShowClearIcon = true;
			
			this.SetIconFromStock(Icon.Stock_Find.Name,
			                      EntryIconPosition.Primary);
		
			this.KeyPressEvent	+= OnKeyPressEvent;
			this.Changed		+= OnChanged;
			this.IconPress		+= OnIconPressEvent;
			this.Shown			+= OnShown;
			this.FocusInEvent	+= OnFocusInEvent;
			this.FocusOutEvent	+= OnFocusOutEvent;
		}
		
		// TODO: remove the method if gkt supports entry icons
		// on all platforms
		public Gtk.Widget GetFallbackWrapper()
		{
			if (IconsSupported)
				return this;
			
			// if the entry does not support embedded icons (old gtk version, e.g. on MS Windows), 
			// return a wrapper with the icon left to the entry
			Gtk.HBox hbox = new Gtk.HBox(false, 3);
			
			Gdk.Pixbuf pb = Basenji.Icons.Icon.Stock_Find.Render(this, Gtk.IconSize.Menu);
			Gtk.Image img = new Gtk.Image(pb);
			
			Gtk.Button btn = new Gtk.Button();
			btn.Relief = Gtk.ReliefStyle.None;
			btn.Clicked += delegate { ShowPopup(); };
			btn.Image = img;
			
			// also disable the button if the Search entry is disabled
			this.StateChanged += delegate { btn.Sensitive = this.Sensitive; };
			
			hbox.PackStart(btn, false, false, 0);
			hbox.PackStart(this, true, true, 0);
			
			return hbox;
		}
		
		public string PlaceholderText {
			get { return placeholderText; }
			set {
				placeholderText = value;
				if (Parent != null) {
					// Only applies if the widget has a parent yet.
					// if not, OnShown() calls this method anyways.
					SetPlaceholderText(true);
				}
			}
		}
		
		public void SetPresets(SearchEntryPreset[] presets) {
			this.presets = presets;
			this.presetsChanged = true;
		}
		
		public bool ShowClearIcon {
			get; set;
		}
		
		private void ShowPopup() {
			if ((presets == null) || (presets.Length == 0))
				return;
			
			EventHandler onActivated = delegate(object sender, EventArgs e) {
				Gtk.MenuItem item = (Gtk.MenuItem)sender;
				
				SetPlaceholderText(false);
				
				if (Text.Length > 0) {
				    if ((!Text.TrimEnd().EndsWith(" or")) && (!Text.TrimEnd().EndsWith(" OR")) &&
						(!Text.TrimEnd().EndsWith(" and")) && (!Text.TrimEnd().EndsWith(" AND"))) {
						
						if (!Text.EndsWith(" "))
							Text += " ";
						
						Text += "and ";
					} else {
						if (!Text.EndsWith(" "))
							Text += " ";
					}
				}
				
				var p = (SearchEntryPreset)item.Data["preset"];
				
				if (!string.IsNullOrEmpty(p.Value)) {
					Text += p.Value;
					
					if (!string.IsNullOrEmpty(p.Suggestion)) {
						Text += " " + p.Suggestion;
						
						GrabFocus();
						SelectRegion(Text.Length - p.Suggestion.Length, Text.Length);
					} else {
						GrabFocus();
						SelectRegion(Text.Length, Text.Length);
					}
				}				
			};
			
			if (presetsChanged) {
				
				if (popup != null)
					popup.Dispose();
				
				popup = new Gtk.Menu();
				
				foreach (var p in presets) {
					Gtk.MenuItem item = new Gtk.MenuItem(p.Caption);
					item.Activated += onActivated;
					item.Data["preset"] = p;
					
					popup.Append(item);
				}
				
				popup.ShowAll();
				presetsChanged = false;
			}
			
			popup.Popup();
		}
		
		protected virtual void OnSearch() {
			if (Search != null)
				Search(this, new SearchEventArgs(Text));
		}
		
		public event SearchEventHandler Search;
		
		private void SetPlaceholderText(bool set) {
			Gdk.Color a = Parent.Style.Base(Gtk.StateType.Normal);
			Gdk.Color b = Parent.Style.Text(Gtk.StateType.Normal);
			
			if (set) {
				if ((Text.Length == 0) && !string.IsNullOrEmpty(placeholderText)) {
					ModifyText(Gtk.StateType.Normal, Util.ColorBlend(a, b));
					Text = placeholderText;
				}
			} else {
				ModifyText(Gtk.StateType.Normal, b);
				if (IsPlaceholderTextActive())
					Text = string.Empty;
			}
		}
		
		private bool IsPlaceholderTextActive() {
			return (Text.Length > 0) && (Text == placeholderText);
		}
		
		[GLib.ConnectBefore()]
		private void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {                    
			if (args.Event.Key != Gdk.Key.Return)
				return;
			
			// update search results
			OnSearch();
		}
		
		private void OnIconPressEvent(object o, IconPressReleaseEventArgs args) {
			if (args.IconPos == EntryIconPosition.Primary) {
				ShowPopup();
			} else { // clear-button pressed
				Text = String.Empty;
			
				// update search results
				OnSearch();
			}
		}
		
		private void OnChanged(object o, EventArgs args) {
			if (ShowClearIcon && ((Text.Length > 0) && !IsPlaceholderTextActive()))
				SetIconFromStock(Icon.Stock_Clear.Name,
				                 EntryIconPosition.Secondary);
			else
				SetIconFromStock(null,
				                 EntryIconPosition.Secondary);
		}
		
		private void OnShown(object o, EventArgs e) {
			SetPlaceholderText(true);
		}
		
		void OnFocusInEvent (object o, Gtk.FocusInEventArgs args) {
			SetPlaceholderText(false);
		}
		
		void OnFocusOutEvent (object o, Gtk.FocusOutEventArgs args) {
			SetPlaceholderText(true);
		}
	}
	
	public delegate void SearchEventHandler(object o, SearchEventArgs args);
	
	public class SearchEventArgs : EventArgs
	{	
		private string searchString;
		
		public SearchEventArgs(string searchString) : base() {
			this.searchString = searchString;
		}
		
		public string SearchString { get {return searchString; } }
	}
}
