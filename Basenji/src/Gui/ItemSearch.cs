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
using Basenji.Gui.Widgets;
using VolumeDB;
using VolumeDB.Searching;

namespace Basenji.Gui
{
	public partial class ItemSearch : Base.WindowBase
	{
		private VolumeDatabase database;
		private volatile bool windowDeleted;
		
		public ItemSearch(VolumeDatabase db) {
			windowDeleted = false;			  
			this.database = db;
			BuildGui();
			btnSearch.Sensitive = false;
			
			// the widget should be visible the first time
			// when the user clicks on an item
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
			
			ISearchCriteria criteria = null;
			try {
				criteria = new EUSLSearchCriteria(txtSearchString.Text);
			} catch (ArgumentException e) {				
				SetStatus(Util.FormatExceptionMsg(e));
				return;				
			}
			
			// callback called when searching has been finished
			AsyncCallback callback = delegate(IAsyncResult ar) {
				if (windowDeleted)
					return;
				
				try {
					VolumeItem[] items = database.EndSearchItem(ar);
					
					Application.Invoke(delegate {						
						// calls selection changed handler
						// that fills the searchrestult view
						tvCategory.Categorize(items);

						TimeSpan time = DateTime.Now.Subtract((DateTime)ar.AsyncState);
						SetStatus(string.Format(S._("Found {0} items in {1:F3} seconds."), items.Length, time.TotalSeconds));
					});
				} catch (TimeoutException) {
					// couldn't get connection lock
					Application.Invoke(delegate {
						tvCategory.Clear();
						navi.Clear();
						tvSearchResult.Clear();
						SetStatus(S._("Timeout: another search is probably still in progress."));
					});
				} catch (TooManyResultsException) {
					Application.Invoke(delegate {
						tvCategory.Clear();
						navi.Clear();
						tvSearchResult.Clear();
						SetStatus(S._("Too many search results. Please refine your search criteria."));
					});
				//} catch (Exception e) { SetStatus(e.Message);
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
		
		private void OnBtnSearchClicked(object o, System.EventArgs args) {
			BeginSearch();
		}
		
		[GLib.ConnectBefore()]
		private void OnTxtSearchStringKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {
			if (args.Event.Key == Gdk.Key.Return)
				BeginSearch();
		}
		
		private void OnTxtSearchStringChanged(object o, EventArgs args) {
			btnSearch.Sensitive = (txtSearchString.Text.Length >= VolumeDatabase.MIN_SEARCHSTR_LENGTH);
		}

		/*
		private void OnBtnCloseClicked(object sender, System.EventArgs e) {
			this.Destroy(); // TODO : not neccessary?		 
		}*/
		
		private void OnTvCategorySelectionChanged(object o, EventArgs args) {
			TreeIter iter;
			CategoryView.Category c = CategoryView.Category.None;
			if (tvCategory.GetSelectedIter(out iter))
				c = tvCategory.GetCategory(iter);
			
			VolumeItem[] categoryItems = tvCategory.GetCategoryItems(c);
//			tvSearchResult.Fill(categoryItems);
			
			navi.SetItems(categoryItems);
			tvSearchResult.Fill(navi.PageItems);
			
			itemInfo.Clear();
			itemInfo.Hide();
		}
		
		private void OnNaviNavigate(object o, NavigateEventArgs args) {
			tvSearchResult.Fill(navi.PageItems);
			
			itemInfo.Clear();
			itemInfo.Hide();
		}
		
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
		private CategoryView				tvCategory;
		private PageNavigation<VolumeItem>	navi; 
		private SearchResultView			tvSearchResult;
		private ItemInfo					itemInfo;
		//private Button			  btnClose;
		private Statusbar					statusbar;
		
		protected override void BuildGui() {
			base.BuildGui();

			// general window settings
			//this.BorderWidth		= 0;
			this.DefaultWidth		= 800;
			this.DefaultHeight		= 600;
			this.Modal				= true;
			this.SkipTaskbarHint	= true;
			this.Title				= S._("Search Items");
			this.Icon				= this.RenderIcon(Basenji.Icons.Icon.Stock_Find, IconSize.Menu);
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.Spacing = 0;
			
			// search box
			HBox hbSearch = new HBox();
			hbSearch.Spacing = 6;
			hbSearch.BorderWidth = 12;
			
			Image img = new Image(Gdk.Pixbuf.LoadFromResource("search.png"));
			hbSearch.PackStart(img, false, false, 0);
			
			hbSearch.PackStart(CreateLabel(S._("Search:")), false, false, 0);
			
			txtSearchString = new Entry();
			hbSearch.PackStart(txtSearchString, true, true, 0);
			
			btnSearch = CreateButton(Stock.Find, true, OnBtnSearchClicked);
			Alignment algn = new Alignment(0.5f, 0.5f, 0f, 0f);
			algn.Add(btnSearch);
			hbSearch.PackStart(algn, false, false, 0); 
			
			vbOuter.PackStart(hbSearch, false, false, 0);
			
			// hpaned
			HPaned hpaned = new HPaned();
			hpaned.BorderWidth = 6;
			hpaned.Position = 180;
			
			// category
			ScrolledWindow swCategory = CreateScrolledView<CategoryView>(out tvCategory, true);
			hpaned.Pack1(swCategory, false, false);
			
			// search results
			VBox vbRight = new VBox();
			vbRight.Spacing = 6;
			
			navi = new PageNavigation<VolumeItem>();
			navi.PageSize = App.SEARCH_RESULTPAGE_SIZE;
			
			vbRight.PackStart(navi, false, false, 0);
			
//			VBox vbSearchResult = new VBox();
//			vbSearchResult.Spacing = 6;
//			
//			vbSearchResult.PackStart(CreateLabel(S._("<b>Search results:</b>"), true), false, false, 0);
			
			ScrolledWindow swSearchResult = CreateScrolledView<SearchResultView>(out tvSearchResult, true);
//			vbSearchResult.PackStart(swSearchResult, true, true, 0);
			vbRight.PackStart(swSearchResult, true, true, 0);
			
			// item info
			itemInfo = new ItemInfo();
			vbRight.PackStart(itemInfo, false, false, 0);
			
			hpaned.Pack2(vbRight, true, true);
			
			vbOuter.PackStart(hpaned, true, true, 0);
			
			
			
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
			tvCategory.Selection.Changed		+= OnTvCategorySelectionChanged;
			navi.Navigate						+= OnNaviNavigate;
			tvSearchResult.Selection.Changed	+= OnTvSearchResultSelectionChanged;
			this.DeleteEvent					+= OnDeleteEvent;
				
			ShowAll();
		}
	}
}
