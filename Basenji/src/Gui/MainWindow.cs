// MainWindow.cs
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

//#pragma warning disable 649

using System;
using System.Threading;
using Gtk;
using System.IO;
using VolumeDB;
using Platform.Common.Globalization;
using PlatformIO = Platform.Common.IO;

namespace Basenji.Gui
{
	public partial class MainWindow : Base.WindowBase
	{
		private VolumeDatabase database = null;
		
		public MainWindow () {
			BuildGui();
			
			SetWindowTitle(null);
			EnableGui(false);
			
			// create default db on first startup
			if (!App.Settings.SettingsFileExists()) {
				string defaultDB = System.IO.Path.Combine(App.Settings.GetSettingsDirectory().FullName, "volumes.vdb");
				// do not overwrite existing db				   
				if (!File.Exists(defaultDB)) {
					// creates default db and if successful,  
					// sets the db path and calls settings.Save()
					OpenDB(defaultDB, true, true, null);
				} else {
					// opens existing default db and if successful, 
					// sets the db path and calls settings.Save()
					OpenDB(defaultDB, false, true, null);
				}
				return;		  
			}
			
			// reopen recent database
			string dbpath = App.Settings.MostRecentDBPath;
			if (App.Settings.OpenMostRecentDB && dbpath.Length > 0) {
				if (!File.Exists(dbpath)) {
					MsgDialog.ShowError(this, S._("Error"), S._("Database '{0}' not found."), dbpath);
					// clear path so the error won't occur again on next startup					
					App.Settings.MostRecentDBPath = string.Empty;
					App.Settings.Save();
				} else {
					OpenDB(dbpath, false, true, null); // volumes list will be refreshed asynchronously
				}
			}			 
		}

		private void OpenDB(string path, bool createNew, bool loadAsync, Util.Callback onsuccess) {
			EnableGui(false); // will be re-enabled after opening AND loading has been completed successfully
			SetWindowTitle(null);				 
			
			// clear views
			tvVolumes.Clear();
			tvItems.Clear();
			itemInfo.Clear();
			
			if (database != null)
				database.Close();

			try {				
				database = new VolumeDatabase(path, createNew);
			} catch (UnsupportedDbVersionException e) {
				MsgDialog.ShowError(this, S._("Unsupported database version"), S._("This database version is not supported."));
				return;
			}
			
			// load volumes
			
			Util.Callback<Volume[]> updateGui = delegate(Volume[] volumes) {
				tvVolumes.Fill(volumes);
				EnableGui(true);
				SetWindowTitle(path);
				SetStatus(string.Format(S._("{0} volumes loaded."), volumes.Length));
				
				App.Settings.MostRecentDBPath = path;
				App.Settings.Save();
				
				if (onsuccess != null)
					onsuccess();  // must be called on the gui thread		   
			};
			
			if (loadAsync) {
				// delegate that will be called when asynchronous volume loading (searching) has been finished
				AsyncCallback cb = delegate(IAsyncResult ar) {
					Volume[] volumes = database.EndSearchVolume(ar);
					Application.Invoke(delegate {
						updateGui(volumes);
//						  tvVolumes.Fill(volumes);
//						  EnableGui(true);
//						  SetWindowTitle(path);
//						  
//						  App.Settings.MostRecentDBPath = path;
//						App.Settings.Save();
//						  
//						  if (onsuccess != null)
//							  onsuccess();	// must be called on the gui thread
					});
				};
				
				database.BeginSearchVolume(cb, null); // returns immediately
				
			} else {
				Volume[] volumes = database.SearchVolume();
				updateGui(volumes);
//				  tvVolumes.Fill(volumes);
//				  EnableGui(true);
//				  SetWindowTitle(path);
//				  
//				  App.Settings.MostRecentDBPath = path;
//				App.Settings.Save();
//				  
//				  if (onsuccess != null)
//					  onsuccess();
			}
		}
		
		private void EnableGui(bool enable)	{
			actAddVolume.Sensitive		= enable;
			actRemoveVolume.Sensitive	= enable;			 
			
			actDBProperties.Sensitive	= enable;
			actSearch.Sensitive			= enable;

			btnAddVolume.Sensitive		= enable;
			btnRemoveVolume.Sensitive	= enable;			
		}
		
		private void SetWindowTitle(string dbPath) {
			if (string.IsNullOrEmpty(dbPath))
				this.Title = App.Name;
			else
				this.Title = string.Format("{0} - {1}", System.IO.Path.GetFileName(dbPath), App.Name);
		}
		
		private void SelectNewDB() {
			string db;
			ResponseType result = FileDialog.Show(FileChooserAction.Save, this, S._("Please enter the name for the new database"), out db);
			
			if (result == ResponseType.Ok && db.Length > 0) {
				if (System.IO.Path.GetExtension(db).Length == 0)
					db += ".vdb";
				
				bool create = true;
				if (File.Exists(db))
					create = (MsgDialog.Show(this, MessageType.Question, ButtonsType.YesNo, S._("Database exists"), S._("Database already exists. Overwrite?")) == ResponseType.Yes);

				if (create) {
					OpenDB(db, true, false, ShowDBProperties); // no async list refresh necessary - new database
					//ShowDBProperties();
				}
			}
		}
		
		private void SelectExistingDB() {
			string db;
			ResponseType result = FileDialog.Show(FileChooserAction.Open, this, S._("Please select a database"), out db);
			
			if (result == ResponseType.Ok && db.Length > 0) {
				// check if the file existst before calling OpenDB()
				// so the currently loaded db won't be unloaded
				if (!File.Exists(db))
					MsgDialog.ShowError(this, S._("Error"), S._("Database not found."));
				else
					OpenDB(db, false, true, null);
			}
		}
		
		private void ShowDBProperties()	{
			DBProperties dbp = new DBProperties(database);
			dbp.Show();
		}
		
		private void AddVolume() {

			PlatformIO.DriveInfo drive;
			
			if (App.Settings.ScannerDevice.Length > 0) {
				try {
					drive = PlatformIO.DriveInfo.FromDevice(App.Settings.ScannerDevice);
				} catch(ArgumentException e) { // e.g. drive not found
					MsgDialog.ShowError(this, S._("Error"), S._("An error occured while accessing drive {0}:\n{1}"), App.Settings.ScannerDevice, e.Message);
					return;
				}
				
			} else {

				DriveSelection ds = new DriveSelection();
				ResponseType result = (ResponseType)ds.Run();
				ds.Destroy();
				if (result != ResponseType.Ok)
					return;
				drive = ds.SelectedDrive;
			}
			
			if (!drive.IsReady) {
				MsgDialog.ShowError(this, S._("Error"), S._("Drive {0} is not ready."), drive.Device); // e.g. no volume inserted
				return;
			}
			
			VolumeScanner vs = new VolumeScanner(database, drive.Device);
			vs.NewVolumeAdded += delegate(object o, NewVolumeAddedEventArgs args) {
				//ListStore store = (ListStore)tvVolumes.Model;
				tvVolumes.AddVolume(args.Volume);
				// TODO : sort list?
			};
		}
		
		private void RemoveVolume() {

			if (tvVolumes.Selection.CountSelectedRows() == 0) {
				MsgDialog.ShowError(this, S._("No volume selected"), S._("Please select a volume record to remove."));
				return;
			}

			ResponseType result = MsgDialog.Show(this, MessageType.Question, ButtonsType.YesNo, S._("Confirmation"), S._("Are you sure you really want to remove the selected volume?"));
			
			if (result == ResponseType.Yes) {
//				TreeModel model;
//				TreeIter iter;
//				
//				  if (!tvVolumes.Selection.GetSelected(out model, out iter))
//					  return;
//				  
//				  Volume volume = (Volume)model.GetValue(iter, 2);
//				database.RemoveVolume(volume.VolumeID);
//				  
//				  // select prev/next row				 
//				  ListStore store = (ListStore)model; 
//				  TreePath p = store.GetPath(iter);
//				  if (!p.Prev())
//					  p.Next();
//				  tvVolumes.Selection.SelectPath(p);
//				  // remove selected row
//				  store.Remove(ref iter);
				
				TreeIter iter;
				if (!tvVolumes.GetSelectedIter(out iter))
					return;
				
				Volume volume = tvVolumes.GetVolume(iter);
				database.RemoveVolume(volume.VolumeID);
				Directory.Delete(DbData.GetVolumeDataPath(database, volume.VolumeID), true);  // remove external db data				
				
				tvVolumes.RemoveVolume(iter);
			}
		}
		
		private void SetStatus(string message) {
			statusbar.Pop(1);
			statusbar.Push(1, message);
		}
		
		private void Quit()	{
			if (database != null)
				database.Close();
			
			// save window state
			int w, h;
			bool isMaximized;
			this.GetSize(out w, out h);
			isMaximized = (this.GdkWindow.State == Gdk.WindowState.Maximized);
			App.Settings.MainWindowWidth = w;
			App.Settings.MainWindowHeight = h;
			App.Settings.MainWindowIsMaximized = isMaximized;
			App.Settings.MainWindowSplitterPosition = hpaned.Position;
			App.Settings.Save();
			
			Application.Quit();
		}
		
		/*
		private enum Columns : int {
			Icon		= 0,
			Id			= 1,
			Title		= 2,
			Added		= 3,
			Files		= 4,
			Size		= 5,
			Category	= 6
		}
		*/
		
//		private void OnBtnNewDBActivated(object sender, System.EventArgs args) {
//			SelectNewDB();
//		}
		
		private void OnActNewDBActivated(object sender, System.EventArgs args) {
			SelectNewDB();
		}
		
//		 private void OnBtnOpenDBActivated(object sender, System.EventArgs args) {
//			SelectExistingDB();
//		}		 
		
		private void OnActOpenDBActivated(object sender, System.EventArgs args) {
			SelectExistingDB();
		}
		
		private void OnActSearchActivated(object sender, System.EventArgs args) {
			ItemSearch s = new ItemSearch(database);
			s.Show();
		}
		
		private void OnActDBPropertiesActivated(object sender, System.EventArgs args) {
			ShowDBProperties();
		}
		
		private void OnBtnAddVolumeClicked(object sender, System.EventArgs args) {
			AddVolume();
		}
		
		private void OnActAddVolumeActivated(object sender, System.EventArgs args) {
			AddVolume();
		}
		
		private void OnBtnRemoveVolumeClicked(object sender, System.EventArgs args) {
			RemoveVolume();
		}
		
		private void OnActRemoveVolumeActivated(object sender, System.EventArgs args) {
			RemoveVolume();
		}
		
		private void OnActPreferencesActivated(object sender, System.EventArgs args) {
			Preferences p = new Preferences();
			p.Show();
		}
		
		private void OnActQuitActivated(object sender, System.EventArgs args) {
			Quit();
		}
		
		[GLib.ConnectBefore()]
		private void OnTvVolumesButtonPressEvent(object o, ButtonPressEventArgs args) {
			 if (args.Event.Type == Gdk.EventType.TwoButtonPress) {
//				  TreeModel model;
//				  TreeIter iter;
//
//				  if (!tvVolumes.Selection.GetSelected(out model, out iter))
//					  return;
//
//				  // load volume properties
//				  Volume volume = (Volume)model.GetValue(iter, 2);
//				  VolumeProperties vp = new VolumeProperties(volume);
//				  vp.Saved += delegate {
//					  model.SetValue(iter, 1, GetVolumeDescription(volume));
//				  };
				
				TreeIter iter;
				if (!tvVolumes.GetSelectedIter(out iter))
					return;
				
				// load volume properties
				Volume volume = tvVolumes.GetVolume(iter);
				VolumeProperties vp = new VolumeProperties(volume);
				vp.Saved += delegate {
					tvVolumes.UpdateVolume(iter, volume);
				};
				
			}		 
		}
		
		private void OnTvVolumesSelectionChanged(object o, EventArgs args) {
//			  TreeModel model;
//			  TreeIter iter;
//
//			  if (!tvVolumes.Selection.GetSelected(out model, out iter))
//				  return;
//
//			  // load volume content in the item tree
//			  Volume volume = (Volume)model.GetValue(iter, 2);
//			  LoadItems(volume);
			
			tvItems.Clear();
			itemInfo.Clear();
			itemInfo.Hide();
			
			TreeIter iter;
			if (!tvVolumes.GetSelectedIter(out iter))
				return;
			
			// load volume content in the item tree
			Volume volume = tvVolumes.GetVolume(iter);
			tvItems.FillRoot(volume);
		}
		
		private void OnTvItemsSelectionChanged(object o, EventArgs args) {
			// get selected item
			TreeIter iter;
			if (!tvItems.GetSelectedIter(out iter))
				return;
			
			VolumeItem item = tvItems.GetItem(iter);
			if (item == null) // null -> not an item row (e.g. the "loading" row)
				return;
			
			itemInfo.ShowInfo(item, database);
		}
		
		private void OnDeleteEvent(object sender, DeleteEventArgs args) {
			Quit();
			args.RetVal = true;
		}		
		
		private void OnActInfoActivated(object sender, System.EventArgs args) {
			About a = new About();
			a.Show();
			return;
		}
	}
	
	// gui initialization
	public partial class MainWindow : Base.WindowBase
	{
		// menubar
		MenuBar menubar;
			
		private Action actFile;
		private Action actEdit;
		private Action actHelp;
		
		private Action actNewDB;
		private Action actOpenDB;
		private Action actQuit;
		
		private Action actAddVolume;
		private Action actRemoveVolume;
		private Action actPreferences;
		
		private Action actInfo;
		
		// toolbar
		private Toolbar toolbar;
		
		private Action actSearch;
		private Action actDBProperties;
		
		// buttons
		private Button btnAddVolume;
		private Button btnRemoveVolume;
		
		// treeviews
		private Widgets.VolumeView	tvVolumes;
		private Widgets.ItemView	tvItems;
		
		// HPanned
		private HPaned hpaned;
		
		// iteminfo
		private Widgets.ItemInfo itemInfo;
		
		// statusbar
		private Statusbar statusbar;
		
		protected override void BuildGui() {
			base.BuildGui();
			
			// restore window state
			int w				= App.Settings.MainWindowWidth;
			int h				= App.Settings.MainWindowHeight;
			bool isMaximized	= App.Settings.MainWindowIsMaximized;
			int splitterPos		= App.Settings.MainWindowSplitterPosition;
			
			//general window settings
			this.BorderWidth	= 2;
			this.DefaultWidth	= w;
			this.DefaultHeight	= h;
			if (isMaximized)			
				this.Maximize();
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.Spacing = 0;
			
			// actiongroup			  
			ActionGroup ag = new ActionGroup("default");			
			
			// file menu
			actFile = CreateAction("file", S._("_File"), null, null, null);
			ag.Add(actFile, null);
			
			actNewDB = CreateAction("newdb", S._("_New Database"), null, Stock.New, OnActNewDBActivated);
			ag.Add(actNewDB, "<control>N");
			
			actOpenDB = CreateAction("opendb", S._("_Open Database"), null, Stock.Open, OnActOpenDBActivated);
			ag.Add(actOpenDB, "<control>O");
			
			actQuit = CreateAction("quit", S._("_Quit"), null, Stock.Quit, OnActQuitActivated);
			ag.Add(actQuit, "<control>Q");			  
			
			// edit menu
			actEdit = CreateAction("edit", S._("_Edit"), null, null, null);
			ag.Add(actEdit, null);
			
			actAddVolume = CreateAction("addvolume", S._("_Add Volume"), null, Stock.Add, OnActAddVolumeActivated);
			ag.Add(actAddVolume, "<control>A");			 
			
			actRemoveVolume = CreateAction("removevolume", S._("_Remove Volume"), null, Stock.Remove, OnActRemoveVolumeActivated);
			ag.Add(actRemoveVolume, "<control>R");
			
			actPreferences = CreateAction("preferences", S._("_Preferences"), null, Stock.Preferences, OnActPreferencesActivated);
			ag.Add(actPreferences, "<control>P");			 
			
			// help menu
			actHelp = CreateAction("help", S._("_Help"), null, null, null);
			ag.Add(actHelp, null);
			
			actInfo = CreateAction("info", S._("_Info"), null, Stock.About, OnActInfoActivated);
			ag.Add(actInfo, "<control>I");	
			
			// shared actions (used in toolbar buttons / menu items)
			actDBProperties = CreateAction("dbproperties", S._("_Database Properties"), null, Stock.Properties, OnActDBPropertiesActivated);
			ag.Add(actDBProperties, "<control>D");
			
			actSearch = CreateAction("searchitems", S._("_Search"), null, Stock.Find, OnActSearchActivated);
			ag.Add(actSearch, "<control>S");
			
			// ui manager
			UIManager manager = new UIManager();
			manager.InsertActionGroup(ag, 0);
			this.AddAccelGroup(manager.AccelGroup);
			
			string ui = @"
			<ui>
				<menubar name=""menubar"">
					<menu action=""file"">
						<menuitem action=""newdb""/>
						<menuitem action=""opendb""/>
						<menuitem action=""dbproperties""/>
						<menuitem action=""searchitems""/>
						<separator/>
						<menuitem action=""quit""/>
					</menu>
					<menu action=""edit"">
						<menuitem action=""addvolume""/>
						<menuitem action=""removevolume""/>
						<separator/>
						<menuitem action=""preferences""/>
					</menu>
					<menu action=""help"">
						<menuitem action=""info""/>
					</menu>
				</menubar>
				<toolbar name=""toolbar"">
					<toolitem action=""dbproperties""/>
					<toolitem action=""searchitems""/>
				</toolbar>
			</ui>
			";
			
			manager.AddUiFromString(ui);
			
			menubar = (MenuBar)manager.GetWidget("/menubar");
			toolbar = (Toolbar)manager.GetWidget("/toolbar");
			
			toolbar.IconSize = IconSize.LargeToolbar; // gtk will use SmallToolbar on windows by default (no custom icons available for this size)
			toolbar.ToolbarStyle = Gtk.ToolbarStyle.Both;
			toolbar.ShowArrow = false;
			
			vbOuter.PackStart(menubar, false, false, 0);
			vbOuter.PackStart(toolbar, false, false, 0);
			
			// hpaned			 
			hpaned = new HPaned();
			//hpaned.CanFocus = true;
			hpaned.Position = splitterPos;
			
			// left vbox			
			VBox vbLeft = new VBox();
			vbLeft.Spacing = 6;
			
			// left scrolled window / treeview (volumes)			
			ScrolledWindow swLeft = CreateScrolledView<Widgets.VolumeView>(out tvVolumes, true);
			vbLeft.PackStart(swLeft, true, true, 0);
			
			// add / remove volume buttonbox			
			HButtonBox bbox1 = new HButtonBox();
			bbox1.Spacing = 6;
			
			btnAddVolume = CreateButton(Stock.Add, true, OnBtnAddVolumeClicked);
			
			bbox1.PackStart(btnAddVolume, false, false, 0);
			
			btnRemoveVolume = CreateButton(Stock.Remove, true, OnBtnRemoveVolumeClicked);
			
			bbox1.PackStart(btnRemoveVolume, false, false, 0);
			
			vbLeft.PackStart(bbox1, false, false, 0);			 
			hpaned.Pack1(vbLeft, false, false);
			
			// right vbox			 
			VBox vbRight = new VBox();
			vbRight.Spacing = 6;
			
			// right scrolled window / treeview (items)			   
			ScrolledWindow swRight = CreateScrolledView<Widgets.ItemView>(out tvItems, true);
			//tvItems.HeadersVisible = false;
			vbRight.PackStart(swRight, true, true, 0);
			
			// item info
			itemInfo = new Widgets.ItemInfo();		  
			
			vbRight.PackStart(itemInfo, false, false, 0);
			hpaned.Pack2(vbRight, false, false);
			
			vbOuter.PackStart(hpaned, true, true, 6);
			
			// statusbar
			statusbar = new Statusbar();
			statusbar.Spacing = 6;
			
			vbOuter.PackStart(statusbar, false, false, 0);
			
			this.Add(vbOuter);
			
			// eventhandlers
			tvVolumes.Selection.Changed += OnTvVolumesSelectionChanged;
			tvVolumes.ButtonPressEvent	+= OnTvVolumesButtonPressEvent;
			
			tvItems.Selection.Changed	+= OnTvItemsSelectionChanged;
			
			this.DeleteEvent			+= OnDeleteEvent;
			
			ShowAll();
		}
	}
}
