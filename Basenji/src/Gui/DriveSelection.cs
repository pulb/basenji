// DriveSelection.cs
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
using System.Threading;
using Gtk;
using Gdk;
using Basenji.Gui.Base;
using Platform.Common.IO;
using Platform.Common.Diagnostics;

namespace Basenji.Gui
{
	public partial class DriveSelection : DialogBase
	{
		// specifies whether the root filesystem "/" should be listed or not
		private const bool EXCLUDE_ROOT_FS = true;
		
		private bool		isDestroyed;
		private DriveInfo	selectedDrive;
		
		public DriveSelection() {
			BuildGui();

			btnOk.Sensitive = false;
			isDestroyed = false;
			selectedDrive = null;
			
			RefreshListAsync();
		}
		
		public DriveInfo SelectedDrive {
			get { return selectedDrive; }
		}
		
		/*
		 * drive refreshing is implemented asynchronously, 
		 * because this operation can take a few seconds on windows systems.
		 */
		private void RefreshListAsync() {
			//btnOk.Sensitive = false;
			
			if (CurrentPlatform.IsWin32) {
				ListStore store = new ListStore(typeof(string));
				store.AppendValues(S._("Waiting for drives..."));
				SetColumns(tvDrives, true);
				tvDrives.Model = store;
				/*ColumnsAutosize();*/
			}

			new Thread(
				delegate() {
					ListStore store = new ListStore(typeof(Pixbuf), typeof(string), typeof(string), typeof(string), /*not visible - driveinfo data*/typeof(object));
					DriveInfo[] drives = DriveInfo.GetDrives(true); // list ready drives only
					TreeIter selectedIter = TreeIter.Zero;
					
					foreach (DriveInfo d in drives) {
						if (EXCLUDE_ROOT_FS && (d.IsMounted && d.RootPath == "/"))
							continue;
						
						//string stockID = Util.GetDriveStockIconID(d);
						//Pixbuf icon = this.RenderIcon(stockID, IconSize.Dialog, string.Empty);
						Pixbuf icon = RenderIcon(Icons.IconUtils.GetDriveIcon(d), IconSize.Dialog);
						
						string drive = string.IsNullOrEmpty(d.Device) ? S._("Unknown") : d.Device;
						string label = GetLabel(d);
						string size = Util.GetSizeStr(d.TotalSize);

						TreeIter iter = store.AppendValues(icon, drive, label, size, d);

						// preselect the first cdrom drive found
						if ((selectedIter.Stamp == TreeIter.Zero.Stamp) && d.DriveType == DriveType.CDRom)
							selectedIter = iter;
					}
					
					// if no cdrom drive was selected, select first drive
					if (selectedIter.Stamp == TreeIter.Zero.Stamp)
						store.GetIterFirst(out selectedIter);
						
					if (!isDestroyed) {
						// only access gui components from the gui thread 
						Application.Invoke(delegate {
							SetColumns(tvDrives, false);
							tvDrives.Model = store;
							/*ColumnsAutosize();*/

							// select selectedIter							
							tvDrives.Selection.SelectIter(selectedIter);

							//btnOk.Sensitive = true;
						});
					}
				}).Start();
		}
		
		
		private static void SetColumns(TreeView tv, bool waiting) {
			foreach (TreeViewColumn c in tv.Columns)
				tv.RemoveColumn(c);

			TreeViewColumn col;
			CellRendererText leftAlignedTR = new CellRendererText();
			leftAlignedTR.Xalign = 0.0f;
			leftAlignedTR.Ellipsize = Pango.EllipsizeMode.End;
			
			if (waiting) {
				col = new TreeViewColumn(string.Empty, leftAlignedTR, "text", 0);
				col.Resizable = true;
				col.Alignment = 0.0f;
				tv.AppendColumn(col);
				
			} else {
				col = new TreeViewColumn(string.Empty, new CellRendererPixbuf(), "pixbuf", 0);
				col.Resizable = false;
				col.Expand = false;
				tv.AppendColumn(col);

				col = new TreeViewColumn(S._("Drive"), leftAlignedTR, "text", 1);
				col.Resizable = true;
				col.Expand = true;
				col.Alignment = 0.0f;
				tv.AppendColumn(col);
				
				col = new TreeViewColumn(S._("Label"), leftAlignedTR, "text", 2);
				col.Resizable = true;
				col.Expand = true;
				col.Alignment = 0.0f;
				tv.AppendColumn(col);

				CellRendererText rightAlignedTR = new CellRendererText();
				rightAlignedTR.Xalign = 1.0f;
				
				col = new TreeViewColumn(S._("Size"), rightAlignedTR, "text", 3);
				col.Resizable = true;
				col.Expand = false;
				col.Alignment = 1.0f;
				tv.AppendColumn(col);
			 }
		}
		
		private bool IsListReady {
			get {
				// Columns.Length == 1 : the list has no drive entries (it shows the waiting message)
				return (tvDrives.Columns.Length > 1);
			}
		}
		
		private static string GetLabel(DriveInfo d) {
			string label = d.VolumeLabel;
			if (label.Length > 0)
				return label;
			else if(d.IsMounted && d.RootPath == "/")
				return S._("Filesystem");
			else
				return "--";
		}

//		private static int GetSelectedIndex(TreeView tv) {
//			  TreePath[] rows = tv.Selection.GetSelectedRows();
//			  if (rows.Length == 0)
//				  return -1;
//			  
//			  return rows[0].Indices[0];			
//		}
		
		private void OnObjectDestroyed(object o, EventArgs e) {
			isDestroyed = true;
		}
		
		private void OnTvDrivesSelectionChanged(object o, EventArgs args) {
			if (!IsListReady)
				return;

			TreeModel model;
			TreeIter iter;
			
			if (tvDrives.Selection.GetSelected(out model, out iter)) {
				// enable ok button on first selection
				if (selectedDrive == null)
					btnOk.Sensitive = true;
				
				selectedDrive = (DriveInfo)model.GetValue(iter, 4);
				
				if (Global.EnableDebugging) {
					Debug.WriteLine("selected drive '{0}'", selectedDrive.Device);
				}
			}	 
		}
		
		[GLib.ConnectBefore()]
		private void OnTvDrivesKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {
			if (IsListReady) {
				if ((selectedDrive != null) && (args.Event.Key == Gdk.Key.Return))
					this.Respond(ResponseType.Ok);
			}
		}
		
		[GLib.ConnectBefore()]
		private void OnTvDrivesButtonPressEvent(object o, Gtk.ButtonPressEventArgs args) {
			if (IsListReady) {
				
				TreePath path;
				tvDrives.GetPathAtPos((int)args.Event.X, (int)args.Event.Y, out path);
				if (path == null)
					return;
				
				if (args.Event.Type == EventType.TwoButtonPress)
					this.Respond(ResponseType.Ok);
			}
		}

	}
	
	// gui initialization
	public partial class DriveSelection : DialogBase
	{
		private TreeView	tvDrives;
		private Button		btnOk;
		private Button		btnCancel;
		
		protected override void BuildGui() {
			base.BuildGui();
			
			//general window settings
			this.BorderWidth		= 0 /* = 2 */; // TODO : somehow the dialog already has a 2 px border.. vbox? bug in gtk#?
			this.Title				= S._("Please select a drive to scan");
			//this.DefaultWidth		= 320;
			this.DefaultHeight		= 340;

			// drives treeview
			ScrolledWindow sw = WindowBase.CreateScrolledView<TreeView>(out tvDrives, false);
			
			// set min width of the scrolled window widget
			sw.WidthRequest = 320;
			
			tvDrives.KeyPressEvent		+= OnTvDrivesKeyPressEvent;
			tvDrives.ButtonPressEvent	+= OnTvDrivesButtonPressEvent;
			tvDrives.Selection.Changed	+= OnTvDrivesSelectionChanged;
			
			this.VBox.PackStart(sw, true, true, 0);
			
			// actionarea
			InitActionArea(out btnOk, out btnCancel, null, null);
			
			this.Destroyed += OnObjectDestroyed;
			
			ShowAll();
		}
	}
}
