// PageNavigation.cs
// 
// Copyright (C) 2009 Patrick Ulbrich
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
	public partial class PageNavigation<ItemType> : BinBase
	{
		private readonly string DEFAULT_EMPTY_CAPTION = S._("No items.");
		
		private ItemType[] items;
		
		private int currentPage;
		private int totalPages;
		private int pageSize;
		
		private string emptyCaption;
		
		public PageNavigation() {			
			pageSize = 10;
			emptyCaption = DEFAULT_EMPTY_CAPTION;
			
			BuildGui();
			Clear();
		}
		
		public void SetItems(ItemType[] items) {
			this.items = items;
			
			currentPage = 0;
			totalPages = (int)Math.Ceiling(items.Length / ((float)pageSize));
			
			UpdateCaption();
			UpdateButtons();
		}
		
		public ItemType[] PageItems {
			get {
				int start, length;
				GetRange(out start, out length);
				
				ItemType[] arr = new ItemType[length];
				Array.Copy(items, start, arr, 0, length);
				return arr;
			}
		}
		
		public bool PrevPage() {
			if (currentPage == 0)
				return false;
				
			currentPage--;
			
			UpdateCaption();
			UpdateButtons();
			
			OnNavigate(NavigationDirection.Previous);
			
			return true;
		}
		
		public bool NextPage() {
			if (currentPage == (totalPages - 1))
				return false;
				
			currentPage++;
			
			UpdateCaption();
			UpdateButtons();
			
			OnNavigate(NavigationDirection.Next);
			
			return true;
		}
		
		public int CurrentPage {
			get {
				return currentPage;
			}
		}
		
		public int TotalPages {
			get {
				return totalPages;
			}
		}
		
		public int PageSize {
			get {
				return pageSize;
			}
			set {
				if (pageSize < 1)
					throw new ArgumentException("Pagesize must be greater than 0");
				pageSize = value;
				
				// reset view
				if (totalPages > 0)
					SetItems(items);
			}
		}
		
		public string EmptyCaption {
			get {
				return emptyCaption;
			}
			set {
				emptyCaption = value;
				UpdateCaption();
			}
		}
		
		public void Clear() {
			items = new ItemType[0];
			currentPage = 0;
			totalPages = 0;
			
			UpdateCaption();
			UpdateButtons();
		}
		
		private void UpdateCaption() {
			if (totalPages > 0) {
				int start, length;
				GetRange(out start, out length);
				
				lbl.Markup = string.Format(	S._("<b>Page {0}/{1}</b>  ({2} - {3} of {4} items)"),
											currentPage + 1,
											totalPages,
											start + 1,
											start + length,
											items.Length);
			} else {
				lbl.Markup = string.IsNullOrEmpty(emptyCaption) ? string.Empty : emptyCaption;
			}
			
			lbl.Sensitive = (totalPages > 0);
		}
		
		private void UpdateButtons() {
			if (totalPages == 0) {
				prev.Sensitive = false;
				next.Sensitive = false;
			} else {
				prev.Sensitive = (currentPage > 0);
				next.Sensitive = (currentPage < (totalPages - 1));
			}
		}
		
		private void GetRange(out int start, out int length) {
			start	= currentPage * pageSize;
			length	= pageSize;
			
			if ((start + length) > items.Length)
				length = items.Length - start;
		}
		
		public event NavigateEventHandler Navigate;
		protected void OnNavigate(NavigationDirection d) {
			if (Navigate != null)
				Navigate(this, new NavigateEventArgs(d));
		}
		
		private void OnBtnPrevClicked(object sender, System.EventArgs args) {
			PrevPage();
		}
		
		private void OnBtnNextClicked(object sender, System.EventArgs args) {
			NextPage();
		}
	}
	
	// gui initialization
	public partial class PageNavigation<ItemType> : BinBase
	{
		private Label lbl;
		private Button prev;
		private Button next;
		
		protected override void BuildGui() {
			// hbox			   
			HBox hbox = new HBox();
			hbox.Spacing = 6;
			
			lbl = WindowBase.CreateLabel(string.Empty, true, false, 0.5F, 0.5F);
			hbox.PackStart(lbl, true, true, 0);
			
			prev = WindowBase.CreateCustomButton(Icons.Icon.Stock_GoBack.Render(this, IconSize.Button), null, OnBtnPrevClicked); 
			prev.Relief = ReliefStyle.None;
			
			next = WindowBase.CreateCustomButton(Icons.Icon.Stock_GoForward.Render(this, IconSize.Button), null, OnBtnNextClicked);
			next.Relief = ReliefStyle.None;
			
			hbox.PackStart(prev, false, false, 0);
			hbox.PackStart(next, false, false, 0);
			
			this.Add(hbox);
		}
		
	}
}
