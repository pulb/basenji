// ItemSearch.cs
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
using VolumeDB;
using VolumeDB.Searching;

namespace Basenji.Gui
{
	public partial class ItemSearch : Base.WindowBase
	{
		// prevent long wating time / massive mem consumption by limiting the searchstr length
		private const int MIN_SEARCHSTR_LENGTH = 3;
		
		private VolumeDatabase database;
		private volatile bool windowDeleted;
		
		public ItemSearch(VolumeDatabase db) {
			windowDeleted = false;			  
			this.database = db;
			BuildGui();
			btnSearch.Sensitive = false;
			// the widget should be visible the first time when the user clicks on an item
			itemInfo.Hide();
		}
		
		private void SetStatus(string message) {
			statusbar.Pop(1);
			statusbar.Push(1, message);
		}
		
		private void BeginSearch() {
			// make sure searching is enabled 
			// (another search may be in progress or searchstring too short)			
			if (!btnSearch.Sensitive)
				return;
			
			// TODO : implement smarter searchstring parsing (like banshee), see tomboy note
			FreeTextSearchCriteria criteria = new FreeTextSearchCriteria(
																		 txtSearchString.Text,
																		 FreeTextSearchField.AnyName,
																		 TextCompareOperator.Contains
																		 );
			
			// callback called when searching has been finished
			AsyncCallback callback = delegate(IAsyncResult ar) {
				if (windowDeleted)
					return;
				
				try {
					VolumeItem[] items = database.EndSearchItem(ar);
					Application.Invoke(delegate {
						tvSearchResult.Fill(items);
						TimeSpan time = DateTime.Now.Subtract((DateTime)ar.AsyncState);
						SetStatus(string.Format(S._("Found {0} items in {1:F3} seconds."), items.Length, time.TotalSeconds));
					});
				} catch (TimeoutException) {
					// couldn't get connection lock
					Application.Invoke(delegate {
						tvSearchResult.Clear();
						SetStatus(S._("Timeout: another search is probably still in progress."));
					});
				} finally {
					Application.Invoke(delegate {
						btnSearch.Sensitive = true;						   
						itemInfo.Clear();
						itemInfo.Hide();
					});
				}
			};
			
			try {
				btnSearch.Sensitive = false;
				SetStatus(S._("Searching..."));
				database.BeginSearchItem(criteria, callback, DateTime.Now);
			} catch(Exception) {
				btnSearch.Sensitive = true;
				SetStatus(string.Empty);
				throw;			  
			}
		}
		
		private void OnBtnSearchClicked(object sender, System.EventArgs e) {
			BeginSearch();
		}
		
		[GLib.ConnectBefore()]
		private void OnTxtSearchStringKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {
			if (args.Event.Key == Gdk.Key.Return)
				BeginSearch();
		}
		
		private void OnTxtSearchStringChanged(object o, EventArgs args) {
			btnSearch.Sensitive = (txtSearchString.Text.Length >= MIN_SEARCHSTR_LENGTH);
		}
		
		/*
		private void OnBtnCloseClicked(object sender, System.EventArgs e) {
			this.Destroy(); // TODO : not neccessary?		 
		}*/
		
		private void OnTvSearchResultSelectionChanged(object o, EventArgs args) {
			TreeIter iter;
			if (!tvSearchResult.GetSelectedIter(out iter))
				return;
			
			VolumeItem item = tvSearchResult.GetItem(iter);
			if (item == null)
				return;
			
			itemInfo.ShowInfo(item, database);
		}
		
		private void OnDeleteEvent(object sender, DeleteEventArgs args) {
			windowDeleted = true;		 
		}
		
	}
	
	// gui initialization
	public partial class ItemSearch : Base.WindowBase {
		
		private Entry						txtSearchString;
		private Button						btnSearch;
		private Widgets.SearchResultView	tvSearchResult;
		private Widgets.ItemInfo			itemInfo;
		//private Button			  btnClose;
		private Statusbar					statusbar;
		
		protected override void BuildGui() {
			base.BuildGui();

			// general window settings
			this.BorderWidth		= 2;
			this.DefaultWidth		= 800;
			this.DefaultHeight		= 600;
			this.Modal				= true;
			this.SkipTaskbarHint	= true;
			this.Title				= S._("Search Items");
			this.Icon				= this.RenderIcon(Basenji.Icons.Icon.Stock_Find, IconSize.Menu);
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.Spacing = 24;
			
			// search box
			HBox hbSearch = new HBox();
			hbSearch.Spacing = 6;
			
			txtSearchString = new Entry();
			hbSearch.PackStart(txtSearchString, true, true, 0);
			
			btnSearch = CreateButton(Stock.Find, true, OnBtnSearchClicked);
			hbSearch.PackStart(btnSearch, false, false, 0); 
			
			vbOuter.PackStart(hbSearch, false, false, 0);
			
			// search result box
			VBox vbSearchResult = new VBox();
			vbSearchResult.Spacing = 6;
			
			vbSearchResult.PackStart(CreateLabel(S._("<b>Search results:</b>"), true), false, false, 0);
			
			ScrolledWindow swSearchResult = CreateScrolledView<Widgets.SearchResultView>(out tvSearchResult, true);
			vbSearchResult.PackStart(swSearchResult, true, true, 0);
			vbOuter.PackStart(vbSearchResult, true, true, 0);
			
			// item info
			itemInfo = new Widgets.ItemInfo();
			vbOuter.PackStart(itemInfo, false, false, 0);
			
			/*
			// hbuttonbox
			HButtonBox bbox = new HButtonBox();
			//bbox.Spacing = 6;
			bbox.LayoutStyle = ButtonBoxStyle.End;
			
			btnClose = CreateButton(Stock.Close, true, OnBtnCloseClicked);
			
			bbox.PackStart(btnClose, false, false, 0);
			vbOuter.PackStart(bbox, false, false, 0); 
			*/
			
			// statusbar
			statusbar = new Statusbar();
			statusbar.Spacing = 6;
			vbOuter.PackStart(statusbar, false, false, 0);
			
			this.Add(vbOuter);
			
			// event handlers
			txtSearchString.KeyPressEvent		+= OnTxtSearchStringKeyPressEvent;
			txtSearchString.Changed				+= OnTxtSearchStringChanged;
			tvSearchResult.Selection.Changed	+= OnTvSearchResultSelectionChanged;
			this.DeleteEvent					+= OnDeleteEvent;
				
			ShowAll();
		}
	}
}
