// CategoryView.cs
// 
// Copyright (C) 2009, 2010 Patrick Ulbrich
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
using Basenji.Gui.Base;
using Basenji.Icons;
using VolumeDB;

namespace Basenji.Gui.Widgets
{
	public class CategoryView : ViewBase
	{
		private const	IconSize							ICON_SIZE = IconSize.Button;
		
		private readonly Gdk.Pixbuf							PIXBUF_ALL_ITEMS;		
		private readonly CategoryInfo[]						CATEGORIES;		
		private readonly Dictionary<string, CategoryInfo>	MIME_MAPPING;
		
		public enum Category : int {			
			Directories		= 0,
			Text			= 1,
			Documents		= 2,
			Music			= 3,
			Movies			= 4,
			Images			= 5,
			Applications	= 6,
			Archives		= 7,
			Development		= 8,
			AllItems		= 9,
			None			= 10
		}
		
		private VolumeItem[] allItems;
		
		public CategoryView() {
		
			PIXBUF_ALL_ITEMS = RenderIcon(Icons.Icon.Stock_File, ICON_SIZE);
			
			// all categories
			CATEGORIES = new CategoryInfo[] {
				_ci(Icons.Icon.Stock_Directory,			S._("Directories")),
				_ci(Icons.Icon.Category_Texts,			S._("Text")),
				_ci(Icons.Icon.Category_Documents,		S._("Documents")),
				_ci(Icons.Icon.Category_Music,			S._("Music")),
				_ci(Icons.Icon.Category_Movies,			S._("Movies")),
				_ci(Icons.Icon.Category_Images	,		S._("Images")),
				_ci(Icons.Icon.Category_Applications,	S._("Applications")),
				_ci(Icons.Icon.Category_Archives,		S._("Archives")),
				_ci(Icons.Icon.Category_Development,	S._("Development"))			
			};
		
			// mimetype -> category mapping
			MIME_MAPPING = MimeCategoryMapping
				.GetMapping<CategoryInfo>(new MimeCategoryData<CategoryInfo>() {
					DirectoryCategory	= CATEGORIES[0],
					TextCategory		= CATEGORIES[1],
					DocumentCategory	= CATEGORIES[2],
					MusicCategory		= CATEGORIES[3],
					MovieCategory		= CATEGORIES[4],
					ImageCategory		= CATEGORIES[5],
					ApplicationCategory	= CATEGORIES[6],
					ArchiveCategory		= CATEGORIES[7],
					DevelopmentCategory	= CATEGORIES[8]
				});
			
			allItems = new VolumeItem[0];
			
			//
			// set up columns
			//
			HeadersVisible = true;

			TreeViewColumn col;
				
			// column icon/category			
			CellRendererPixbuf pix = new CellRendererPixbuf();
			CellRendererText txt = new CellRendererText();
			col = new TreeViewColumn();
			col.SortColumnId = 1;
			col.Resizable = true;
			col.Title = S._("Category");
			col.PackStart(pix, false);
			col.PackStart(txt, false);
			col.SetAttributes(pix, "pixbuf", 0);
			col.SetAttributes(txt, "text", 1);
			
			AppendColumn(col);
			
			Model = GetNewStore();
		}
		
		public void Categorize(VolumeItem[] items) {
			if (items == null)
				throw new ArgumentNullException("items");
				
			ListStore store	= GetNewStore();
			TreeIter iter	= TreeIter.Zero;
			
			this.allItems = items;
			
			ClearCategories();
			
			if (items.Length > 0) {
				// categorize items
				foreach(VolumeItem item in items) {
					CategoryInfo ci;
					if (MIME_MAPPING.TryGetValue(item.MimeType, out ci))
						ci.items.Add(item);
				}
				
				// fill listore
				iter = store.AppendValues(	PIXBUF_ALL_ITEMS,
											string.Format("{0} ({1})", S._("All items"), items.Length),
											Category.AllItems);
				
				for (int i = 0; i < CATEGORIES.Length; i++) {
					CategoryInfo ci = CATEGORIES[i];
					
					if (ci.items.Count > 0) {
						store.AppendValues(	ci.pixbuf,
											string.Format("{0} ({1})", ci.caption, ci.items.Count),
											(Category)i);
					}
				}
			}
			
			Model = store;
			ColumnsAutosize();
			
			// select "all items"
			if (!iter.Equals(TreeIter.Zero))
				Selection.SelectIter(iter);
		}
		
		public Category GetCategory(TreeIter iter) {
			return (Category)Model.GetValue(iter, 2);
		}
		
		public VolumeItem[] GetCategoryItems(Category c) {
			switch(c) {
				case Category.None:
					return new VolumeItem[0];
				case Category.AllItems:
					return allItems;
				default:
					return CATEGORIES[(int)c].items.ToArray();
			}
		}
		
		public void Clear() {			
			this.allItems = new VolumeItem[0];
			
			ClearCategories();
			
			if (Model != null) {
				ListStore store = (ListStore)Model;
				store.Clear();
			}
		}
		
		private void ClearCategories() {
			foreach(CategoryInfo ci in CATEGORIES)
				ci.items.Clear();
		}
		
		private static ListStore GetNewStore() {
			return new ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(Category));
		}
		
		private CategoryInfo _ci(Icons.Icon icon, string caption) {
			return new CategoryInfo(RenderIcon(icon, ICON_SIZE), caption);
		}
		
		private class CategoryInfo
		{
			public CategoryInfo(Gdk.Pixbuf pb, string caption) {
				this.items		= new List<VolumeItem>();
				this.pixbuf		= pb;
				this.caption	= caption;
			}
			
			public List<VolumeItem>	items;
			public Gdk.Pixbuf		pixbuf;
			public string			caption;			
		}
	}
}
