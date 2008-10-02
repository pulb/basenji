// ItemView.cs
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
using Basenji.Gui.Base;
using Basenji.Icons;

namespace Basenji.Gui.Widgets
{
	public class ItemView : ViewBase
	{
		private const		string		STR_LOADING		= "Loading...";
		private const		string		STR_EMPTY		= "(empty)";
		
		private const		IconSize	ICON_SIZE		= IconSize.Button;
		
		private ItemIcons itemIcons;
		private Gdk.Pixbuf loadingIcon;
		
		private VolumeType currentVolumeType;
		
		public ItemView() {
			itemIcons = new ItemIcons(this);
			loadingIcon = this.RenderIcon(Icons.Icon.Stock_Find, ICON_SIZE);
			
			// event handlers
			RowExpanded			+= OnRowExpanded;
			ButtonPressEvent	+= OnButtonPressEvent;
		}
		
		public void FillRoot(Volume volume) {
			TreeStore store;
			ResetView();
			
			switch (volume.GetVolumeType()) {
				case VolumeType.FileSystemVolume:
					InitFileSystemView(out store);
					
					// load volume root
					FileSystemVolume fsv = (FileSystemVolume)volume;
					DirectoryVolumeItem item = fsv.GetRoot();
					
					AppendDirRows(store, TreeIter.Zero, item);

					Model = store;
					break;
				//case VolumeType.CDDAVolume
				//	  ...
				//	  break;
				default:
					throw new NotImplementedException("Items view has not been implemented for this volumetype");
			}
		}
		
		public void Clear() {
			if (Model != null) {
				TreeStore store = (TreeStore)Model;
				store.Clear();
			}
		}
		
		public VolumeType CurrentVolumeType {
			get { return currentVolumeType; }		 
		}
		
		public VolumeItem GetItem(TreeIter iter) {
			VolumeItem item = (VolumeItem)Model.GetValue(iter, 2);
			return item;
		}
		
		private void InitFileSystemView(out TreeStore store) {
			currentVolumeType = VolumeType.FileSystemVolume;
			TreeViewColumn col;
				
			CellRendererPixbuf pix = new CellRendererPixbuf();
			CellRendererText txt = new CellRendererText();
			col = new TreeViewColumn();
			col.PackStart(pix, false);
			col.PackStart(txt, false);
			col.SetAttributes(pix, "pixbuf", 0);
			col.SetAttributes(txt, "text", 1);
			AppendColumn(col);
		
			// set up store
			store = new TreeStore(typeof(Gdk.Pixbuf), typeof(string),  /* VolumeItem - not visible */ typeof(FileSystemVolumeItem));
		}
		
		private void AppendDirRows(TreeStore store, TreeIter parent, DirectoryVolumeItem item) {
			bool					parentIsRoot	= parent.Equals(TreeIter.Zero);
			DirectoryVolumeItem[]	dirs			= item.GetDirectories();
			FileVolumeItem[]		files			= item.GetFiles();
			
			// if no files or dirs have been found, add an empty node
			if (dirs.Length == 0 && files.Length == 0) {
				AppendDirValues(store, parent, parentIsRoot, null, STR_EMPTY, null);
			} else {
				foreach(DirectoryVolumeItem dir in dirs) {
					TreeIter iter = AppendDirValues(store, parent, parentIsRoot, GetIcon(dir), dir.Name, dir);
					AppendDirValues(store, iter, false, loadingIcon, STR_LOADING, null);
				}
				
				foreach(FileVolumeItem file in files) {
					AppendDirValues(store, parent, parentIsRoot, GetIcon(file), file.Name, file);
				}
			}			 
		}
		
		private static TreeIter AppendDirValues(TreeStore store, TreeIter parent, bool parentIsRoot, Gdk.Pixbuf icon, string name, VolumeItem item) {
			if (parentIsRoot)
				return store.AppendValues(icon, name, item);
			else
				return store.AppendValues(parent, icon, name, item);
		}
		
		private Gdk.Pixbuf GetIcon(VolumeItem item) {
			return itemIcons.GetIconForItem(item, ICON_SIZE);	
		}
		
		private void OnRowExpanded(object o, RowExpandedArgs args) {
			switch(CurrentVolumeType) {			   
				case VolumeType.FileSystemVolume:
					TreeStore store = (TreeStore)Model;
					
					// get child node of expanded node
					TreeIter child;
					store.IterChildren(out child, args.Iter);

					// test if the first child is the "loading" child node
					if ((GetItem(child) == null) && ((string)Model.GetValue(child, 1) == STR_LOADING)) {
						// append dir children
						DirectoryVolumeItem dir = (DirectoryVolumeItem)GetItem(args.Iter);
						AppendDirRows(store, args.Iter, dir);
					
						// remove "loading" child node
						store.Remove(ref child);
					}
					break;
				default:
					throw new NotImplementedException("View for this VolumeType has not been implemented yet");
			}
									
		}
		
		[GLib.ConnectBefore()]
		private void OnButtonPressEvent(object o, ButtonPressEventArgs args) {
			if (args.Event.Type == Gdk.EventType.TwoButtonPress) {
				TreeIter iter;				  
				if (!GetSelectedIter(out iter))
					return;

				if (Model.IterHasChild(iter)) {
					TreePath path = Model.GetPath(iter);
					if (GetRowExpanded(path))
						CollapseRow(path);
					else
						ExpandRow(path, false);
				}
			}
		}		 
	}
}