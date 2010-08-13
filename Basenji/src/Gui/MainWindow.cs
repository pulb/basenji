// MainWindow.cs
// 
// Copyright (C) 2008 - 2010 Patrick Ulbrich
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
using System.IO;
using VolumeDB;
using VolumeDB.Searching;
using VolumeDB.Searching.VolumeSearchCriteria;
using PlatformIO = Platform.Common.IO;

namespace Basenji.Gui
{
	public partial class MainWindow : Base.WindowBase
	{
		private VolumeDatabase	database = null;
		private volatile bool	windowDeleted;
		private ISearchCriteria	lastSuccessfulSearchCriteria;
		private RecentManager	recentManager;		
		
		private readonly RecentData recentData = new RecentData() {
			AppName = App.Name,
			AppExec = App.Name.ToLower() + " %u",
			MimeType = "application/x-sqlite3",
		};
					
		public MainWindow (string dbPath) {
			recentManager = RecentManager.Default;
			
			// retrieve the sort property from settings
			// (from default settings if the settings file does not exist yet)
			Widgets.VolumeSortProperty sp;
			bool desc;
			GetVolumeSortProperty(out sp, out desc);
			
			BuildGui();
			
			windowDeleted = false;
			lastSuccessfulSearchCriteria = null;
			
			SetWindowTitle(null);
			EnableGui(false);
			
			// set the volumeview's sort property
			// (before filling it with volumes) 
			tvVolumes.SetSortProperty(sp, desc);
			
			// create default db on first startup
			if (!App.Settings.SettingsFileExists()) {
				// creates (or opens existing) default db and if successful,  
				// sets the db path and calls settings.Save()
				OpenOrCreateDefaultDB(false);
				return;		  
			}
			
			if (dbPath != null) {
				if (!File.Exists(dbPath)) {
					MsgDialog.ShowError(this,
					                    S._("Error"),
					                    S._("Database '{0}' not found."),
					                    dbPath);
				} else {
					// volumes list will be refreshed asynchronously
					OpenDB(dbPath, false, true, null);
				}
				
				return;
			}
			
			// reopen recent database
			dbPath = App.Settings.MostRecentDBPath;
			if (App.Settings.OpenMostRecentDB && dbPath.Length > 0) {
				if (!File.Exists(dbPath)) {
					MsgDialog.ShowError(this,
					                    S._("Error"),
					                    S._("Database '{0}' not found."),
					                    dbPath);
					
					// clear path so the error won't occur again on next startup					
					App.Settings.MostRecentDBPath = string.Empty;
					App.Settings.Save();
				} else {
					// volumes list will be refreshed asynchronously
					OpenDB(dbPath, false, true, null);
				}
			}			 
		}

		private void OpenOrCreateDefaultDB(bool confirmCreation) {
			// do not overwrite existing db				   
			if (File.Exists(App.DefaultDB)) {
				// opens existing default db and if successful, 
				// sets the db path and calls settings.Save()
				OpenDB(App.DefaultDB, false, true, null);				
			} else {
				if (confirmCreation && !(MsgDialog.Show(this,
				                                       MessageType.Question,
				                                       ButtonsType.YesNo,
				                                       S._("Database not found"),
				                                       S._("Default database not found. Create?")) == ResponseType.Yes)) {
					return;
				}
				// creates default db and if successful,  
				// sets the db path and calls settings.Save()
				OpenDB(App.DefaultDB, true, true, null);
			}
		}
		
		private void OpenDB(string path, bool createNew, bool loadAsync, System.Action onsuccess) {
			EnableGui(false); // will be re-enabled after opening AND loading has been completed successfully
			SetWindowTitle(null);				 
			
			// clear views
			tvVolumes.Clear();
			tvItems.Clear();
			itemInfo.Clear();
			itemInfo.Hide();
			
			if (database != null)
				database.Close();

			lastSuccessfulSearchCriteria = null;
			
			try {				
				database = new VolumeDatabase(path, createNew);
				database.SearchItemResultsLimit = App.SEARCH_RESULTS_LIMIT;
			} catch (UnsupportedDbVersionException) {
				MsgDialog.ShowError(this,
				                    S._("Unsupported database version"),
				                    S._("This database version is not supported."));
				return;
			}
			
			// load volumes
			
			Action<Volume[]> updateGui = (Volume[] volumes) => {
				tvVolumes.Fill(volumes);

				// select first volume
				/*
				// this clearly harms startup time.
				TreeIter iter;
				if (tvVolumes.Model.GetIterFirst(out iter))
					tvVolumes.Selection.SelectIter(iter);
				*/
				
				EnableGui(true);
				SetWindowTitle(path);
				SetTempStatus(string.Format(S._("{0} volumes loaded."), volumes.Length));
				
				recentManager.AddFull("file://" + path, recentData);
				
				App.Settings.MostRecentDBPath = path;
				App.Settings.Save();
				
				if (onsuccess != null)
					onsuccess();  // must be called on the gui thread		   
			};
			
			if (loadAsync) {
				// delegate that will be called 
				// when asynchronous volume loading (searching) has been finished.
				AsyncCallback cb = (IAsyncResult ar) => {
					Volume[] volumes;
					
					try {
						volumes = database.EndSearchVolume(ar);
					} catch (Exception ex) {
						Application.Invoke(delegate {
							SetStatus(string.Format(S._("An error occured while loading the volume list: {0}"),
							                        ex.Message));
						});
						return;
					}
					
					Application.Invoke(delegate {
						updateGui(volumes);
					});
				};
				
				database.BeginSearchVolume(cb, null); // returns immediately
				
			} else {
				Volume[] volumes = database.SearchVolume();
				updateGui(volumes);
			}
		}
		
		private void EnableGui(bool enable)	{
			actAddVolume.Sensitive		= enable;
			actRemoveVolume.Sensitive	= enable;
			actEditVolume.Sensitive		= enable;
			
			actDBProperties.Sensitive	= enable;
			actImport.Sensitive			= enable;
			actSearch.Sensitive			= enable;
			
			txtSearchString.Sensitive	= enable;
		}
		
		private void SetWindowTitle(string dbPath) {
			if (string.IsNullOrEmpty(dbPath))
				this.Title = App.Name;
			else
				this.Title = string.Format("{0} - {1}", System.IO.Path.GetFileName(dbPath), App.Name);
		}
		
		private void SelectNewDB() {
			string db;
			ResponseType result = FileDialog.Show(FileChooserAction.Save,
			                                      this,
			                                      S._("Please enter the name for the new database"),
			                                      out db);
			
			if (result == ResponseType.Ok && db.Length > 0) {
				if (System.IO.Path.GetExtension(db).Length == 0)
					db += ".vdb";
				
				bool create = true;
				if (File.Exists(db))
					create = (MsgDialog.Show(this,
					                         MessageType.Question,
					                         ButtonsType.YesNo,
					                         S._("Database exists"),
					                         S._("Database already exists. Overwrite?")) == ResponseType.Yes);

				if (create) {
					OpenDB(db, true, false, ShowDBProperties); // no async list refresh necessary - new database
					//ShowDBProperties();
				}
			}
		}
		
		private void SelectExistingDB() {
			string db;
			ResponseType result = FileDialog.Show(FileChooserAction.Open,
			                                      this,
			                                      S._("Please select a database"),
			                                      out db);
			
			if (result == ResponseType.Ok && db.Length > 0) {
				// check if the file existst before calling OpenDB()
				// so the currently loaded db won't be unloaded.
				if (!File.Exists(db))
					MsgDialog.ShowError(this,
					                    S._("Error"),
					                    S._("Database not found."));
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
					MsgDialog.ShowError(this,
					                    S._("Error"),
					                    S._("An error occured while accessing drive {0}:\n{1}"),
					                    App.Settings.ScannerDevice,
					                    e.Message);
					
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
				MsgDialog.ShowError(this,
				                    S._("Error"),
				                    S._("Drive {0} is not ready."),
				                    drive.Device); // e.g. no volume inserted
				
				return;
			}
			
			if (!drive.IsMounted && !drive.HasAudioCdVolume) {
				MsgDialog.ShowError(this,
				                    S._("Error"),
				                    S._("Drive {0} is neither mounted nor does it contain an audio cd."),
				                    drive.Device);
				
				return;
			}
			
			VolumeScanner vs = new VolumeScanner(database, drive);
			vs.NewVolumeAdded += (object o, NewVolumeAddedEventArgs args) => {
				if (lastSuccessfulSearchCriteria != null) {
					// the volumes treeview is filtered,
					// so refill the treeview using the last sucessful searchcriteria.
					// (the freshly added volume may be matched by that criteria.)
					SearchVolumeAsync(lastSuccessfulSearchCriteria, null);
				} else {
					// volumes treeview isn't filtered and contains all volumes,
					// so just append.
					tvVolumes.AddVolume(args.Volume);
				}
				
			};
		}
		
		private void RemoveVolume() {
			TreeIter iter;
			
			if (!tvVolumes.GetSelectedIter(out iter)) {
				MsgDialog.ShowError(this,
				                    S._("No volume selected"),
				                    S._("Please select a volume record to remove."));
				return;
			}

			ResponseType result = MsgDialog.Show(this, 
			                                     MessageType.Question, 
			                                     ButtonsType.YesNo, 
			                                     S._("Confirmation"), 
			                                     S._("Are you sure you really want to remove the selected volume?"));
			
			if (result == ResponseType.Yes) {
				Volume volume = tvVolumes.GetVolume(iter);
				database.RemoveVolume(volume.VolumeID);
				
				// remove external db data
				string dbDataPath = PathUtil.GetDbDataPath(database);
				string volumeDataPath = DbData.GetVolumeDataPath(dbDataPath, volume.VolumeID);
				if (Directory.Exists(volumeDataPath))
					Directory.Delete(volumeDataPath, true);
				
				tvVolumes.RemoveVolume(iter);
			}
		}
		
		private void EditVolume() {
			TreeIter iter;
			
			if (!tvVolumes.GetSelectedIter(out iter)) {
				MsgDialog.ShowError(this,
				                    S._("No volume selected"),
				                    S._("Please select a volume to edit."));
				return;
			}
			
			// load volume properties
			Volume volume = tvVolumes.GetVolume(iter);
			VolumeProperties vp = new VolumeProperties(volume);
			vp.Saved += delegate {
				tvVolumes.UpdateVolume(iter, volume);
			};
		}
		
		private void EditItem() {
			TreeIter iter;
			
			if (!tvItems.GetSelectedIter(out iter)) {
				MsgDialog.ShowError(this,
				                    S._("No item selected"),
				                    S._("Please select an item to edit."));
				return;
			}
			
			// load item properties
			VolumeItem item = tvItems.GetItem(iter);
			
			// null -> not an item row (e.g. the "loading" row)
			if (item == null)
				return;
			
			new ItemProperties(item);
//			ip.Saved += delegate {
//				tvItems.UpdateItem(iter, item);
//			};
		}
		
		private void BeginVolumeSearch() {
			ISearchCriteria criteria = null;
			
			if (txtSearchString.Text.Length > 0) {
				try {
					criteria = new EUSLSearchCriteria(txtSearchString.Text);
				} catch (ArgumentException e) {
					SetTempStatus(Util.FormatExceptionMsg(e));
					return;
				}
			}
			
			System.Action oncompleted = () => {
				Application.Invoke(delegate {
					// treeview filling has stolen the focus.
					txtSearchString.GrabFocus();
				});
			};
					
			SearchVolumeAsync(criteria, oncompleted);
		}
		
		private void SearchVolumeAsync(ISearchCriteria criteria, System.Action onsearchcompled) {			
			// delegate that will be called 
			// when asynchronous volume searching has been finished.
			AsyncCallback cb = (IAsyncResult ar) => {
				if (windowDeleted)
					return;
				
				try {
					Volume[] volumes = database.EndSearchVolume(ar);
					
					Application.Invoke(delegate {
						tvVolumes.Fill(volumes);
						SetStatus(string.Empty);
						
						// remember last successful searchcriteria
						// (that has been used to successfully fill the treeview).
						// (set in gtk thread to avoid race conditions.)
						lastSuccessfulSearchCriteria = (ISearchCriteria)ar.AsyncState;
					});
				} catch (Exception e) {
					if (e is TimeoutException) {
						// couldn't get connection lock
						Application.Invoke(delegate {
							SetStatus(S._("Timeout: another search is probably still in progress."));
						});
					} else {
						Application.Invoke(delegate {
							//SetStatus(Util.FormatExceptionMsg(e));
							SetStatus(string.Empty);
						});
						throw;
					}
				} finally {
					if (onsearchcompled != null)
						onsearchcompled();
				}
			};
			
			try {
				SetStatus(S._("Searching..."));
				
				if (criteria != null)
					database.BeginSearchVolume(criteria, cb, criteria);
				else
					database.BeginSearchVolume(cb, null);
			} catch (Exception) {
				SetStatus(string.Empty);
				throw;			  
			}
		}
		
		private void SetStatus(string message) {
			statusbar.Pop(1);
			statusbar.Push(1, message);
		}
		
		private void SetTempStatus(string message) {
			SetStatus(message);
			GLib.Timeout.Add(3000, delegate {
				statusbar.Pop(1);
				return false;
			});
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
			App.Settings.ItemInfoMinimized1 = itemInfo.Minimized;
			App.Settings.Save();
			
			Application.Quit();
		}
		
		private static string AppendDots(string s) {
			return string.Format("{0} ...", s);
		}
		
		private static void GetVolumeSortProperty(out Widgets.VolumeSortProperty sortProperty, out bool descending) {
			int sp = App.Settings.VolumeSortProperty;
			bool desc = false;
			
			if (sp < 0) {
				sp *= (-1);
				desc = true;
			}
			
			sortProperty = (Widgets.VolumeSortProperty)sp;
			descending = desc;
		}
		
		private static void SaveVolumeSortProperty(Widgets.VolumeSortProperty sortProperty, bool descending) {
			int sp = (int)sortProperty;
			if (descending)
				sp *= (-1);
			
			App.Settings.VolumeSortProperty = sp;
			App.Settings.Save();
		}
		
		private void OnActNewDBActivated(object sender, System.EventArgs args) {
			SelectNewDB();
		}
		
		private void OnActOpenDBActivated(object sender, System.EventArgs args) {
			SelectExistingDB();
		}
		
		private void OnActOpenDefaultDBActivated(object sender, System.EventArgs args) {
			OpenOrCreateDefaultDB(true);
		}
		
		private void OnActRecentlyUsedActivated(object sender, System.EventArgs args) {
			RecentAction act = (RecentAction)sender;
			string path = act.CurrentUri.Replace("file://", string.Empty);
			
			if (!File.Exists(path)) {
				MsgDialog.ShowError(this, S._("Error"), S._("Database '{0}' not found."), path);
				return;
			}
			
			// volumes list will be refreshed asynchronously
			OpenDB(path, false, true, null); 
		}
		
		private void OnActImportActivated(object sender, System.EventArgs args) {
			Import import = new Import(database);
			import.VolumesImported += delegate {
				if (lastSuccessfulSearchCriteria != null) {
					// the volumes treeview is filtered,
					// so refill the treeview using the last sucessful searchcriteria.
					// (the imported volumes may be matched by that criteria.)
					SearchVolumeAsync(lastSuccessfulSearchCriteria, null);
				} else {
					// volumes treeview isn't filtered and contains all volumes
					SearchVolumeAsync(null, null);
				}
				
			};
			
			import.Show();
		}
		
		private void OnActSearchActivated(object sender, System.EventArgs args) {
			ItemSearch s = new ItemSearch(database);
			s.Show();
		}
		
		private void OnActDBPropertiesActivated(object sender, System.EventArgs args) {
			ShowDBProperties();
		}
		
		private void OnActAddVolumeActivated(object sender, System.EventArgs args) {
			AddVolume();
		}
		
		private void OnActRemoveVolumeActivated(object sender, System.EventArgs args) {
			RemoveVolume();
		}
		
		private void OnActEditVolumeActivated(object sender, System.EventArgs args) {
			EditVolume();
		}
		
		private void OnSortActionActivated(object sender, System.EventArgs args) {
			// do not sort / save settings 
			// if an action is activated in buildGui()
			if (!buildGuiCompleted)
				return;
			
			Widgets.VolumeSortProperty sp = Widgets.VolumeSortProperty.Added;
			bool desc = actVolumesSortDescending.Active;
			
			if (sender == actVolumesSortDescending) {
				foreach (var a in volumeSortActions) {
					if (a.Active) {
						sp = (Widgets.VolumeSortProperty)a.Value;
						break;
					}
				}
			} else { // sortfield action
				RadioAction act = (RadioAction)sender;
				sp = (Widgets.VolumeSortProperty)act.Value;
			}
			
			tvVolumes.SetSortProperty(sp, desc);
			SaveVolumeSortProperty(sp, desc); 
		}
		
		private void OnActEditItemActivated(object sender, System.EventArgs args) {
			EditItem();
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
			TreePath path;
			tvVolumes.GetPathAtPos((int)args.Event.X, (int)args.Event.Y, out path);
			//if (path == null)
			//	return;
				
			if ((args.Event.Button == 1) && (args.Event.Type == Gdk.EventType.TwoButtonPress)) {
				if (path != null)
					EditVolume();
			} else if ((args.Event.Button == 3) && (args.Event.Type == Gdk.EventType.ButtonPress)) {
				uint btn = args.Event.Button;
				uint time = args.Event.Time;
				if (path != null)
					volumeContextMenu.Popup(null, null, null, btn, time);
				else
					volumeSortContextMenu.Popup(null, null, null, btn, time);
			}
		}
		
		[GLib.ConnectBefore()]
		private void OnTvVolumesKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {
			if (args.Event.Key == Gdk.Key.Return)
				EditVolume();
		}
		
		private void OnTvVolumesSelectionChanged(object o, EventArgs args) {
			tvItems.Clear();
			itemInfo.Clear();
			itemInfo.Hide();
			
			TreeIter iter;
			if (!tvVolumes.GetSelectedIter(out iter))
				return;
			
			// load volume content in the item tree
			Volume volume = tvVolumes.GetVolume(iter);
			tvItems.FillRoot(volume, database);
		}
		
		[GLib.ConnectBefore()]
		private void OnTvItemsButtonPressEvent(object o, ButtonPressEventArgs args) {
			TreePath path;
			tvItems.GetPathAtPos((int)args.Event.X, (int)args.Event.Y, out path);
			if (path == null)
				return;
				
			if ((args.Event.Button == 1) && (args.Event.Type == Gdk.EventType.TwoButtonPress)) {
				EditItem();
			} else if ((args.Event.Button == 3) && (args.Event.Type == Gdk.EventType.ButtonPress)) {
				uint btn = args.Event.Button;
				uint time = args.Event.Time;
				itemContextMenu.Popup(null, null, null, btn, time);
			}
		}
		
		[GLib.ConnectBefore()]
		private void OnTvItemsKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {
			if (args.Event.Key == Gdk.Key.Return)
				EditItem();
		}
		
		private void OnTvItemsSelectionChanged(object o, EventArgs args) {
			// get selected item
			TreeIter iter;
			if (!tvItems.GetSelectedIter(out iter))
				return;
			
			VolumeItem item = tvItems.GetItem(iter);
			// null -> not an item row (e.g. the "loading" row)
			if (item == null)
				return;
			
			itemInfo.ShowInfo(item, database);
		}
		
		private void OnTxtSearchStringSearch(object o, Widgets.SearchEventArgs args) {
			BeginVolumeSearch();
		}
		
		private void OnDeleteEvent(object sender, DeleteEventArgs args) {
			windowDeleted = true;
			Quit();
			args.RetVal = true;
		}		
		
		private void OnActInfoActivated(object sender, System.EventArgs args) {
			About a = new About();
			a.Run();
			a.Destroy();
		}
	}
	
	// gui initialization
	public partial class MainWindow : Base.WindowBase
	{
		// menubar
		MenuBar menubar;
			
		private Gtk.Action actFile;
		private Gtk.Action actEdit;
		private Gtk.Action actHelp;
		
		private Gtk.Action actNewDB;
		private Gtk.Action actOpenDB;
		private Gtk.Action actOpenDefaultDB;
		private Gtk.Action actDBProperties;
		private Gtk.Action actImport;
		private Gtk.Action actSearch;
		private Gtk.Action actQuit;
		private Gtk.RecentAction actRecentlyUsed;
		
		private Gtk.Action actAddVolume;
		private Gtk.Action actRemoveVolume;
		private Gtk.Action actEditVolume;
		private Gtk.Action actPreferences;
		
		private Gtk.Action actInfo;
		
		private Gtk.Action actEditItem;
		
		private Gtk.Action actVolumesSortBy;
		private Gtk.ToggleAction actVolumesSortDescending;
		private Gtk.RadioAction[] volumeSortActions;
		
		// toolbar
		private Toolbar toolbar;
		
		// context menus
		private Menu volumeContextMenu;
		private Menu volumeSortContextMenu;
		private Menu itemContextMenu;
		
		// search entry
		private Widgets.SearchEntry txtSearchString;
		
		// treeviews
		private Widgets.VolumeView	tvVolumes;
		private Widgets.ItemView	tvItems;
		
		// HPanned
		private HPaned hpaned;
		
		// iteminfo
		private Widgets.ItemInfo itemInfo;
		
		// statusbar
		private Statusbar statusbar;
		
		private bool buildGuiCompleted = false;
		
		protected override void BuildGui() {
			base.BuildGui();
			
			// restore window state
			int w					= App.Settings.MainWindowWidth;
			int h					= App.Settings.MainWindowHeight;
			bool isMaximized		= App.Settings.MainWindowIsMaximized;
			int splitterPos			= App.Settings.MainWindowSplitterPosition;
			bool itemInfoMinimized	= App.Settings.ItemInfoMinimized1;
			
			// general window settings
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
			
			actNewDB = CreateAction("newdb", AppendDots(S._("_New Database")), null, Stock.New, OnActNewDBActivated);
			ag.Add(actNewDB, "<control>N");
			
			actOpenDB = CreateAction("opendb", AppendDots(S._("_Open Database")), null, Stock.Open, OnActOpenDBActivated);
			ag.Add(actOpenDB, "<control>O");
			
			actOpenDefaultDB = CreateAction("open_default_db", S._("Open Default Database"), null, null, OnActOpenDefaultDBActivated);
			ag.Add(actOpenDefaultDB);
			
			actDBProperties = CreateAction("dbproperties", S._("_Database Properties"), null, Stock.Properties, OnActDBPropertiesActivated);
			ag.Add(actDBProperties, "<control>D");
			
			actImport = CreateAction("import", AppendDots(S._("_Import")), null, null, OnActImportActivated);
			ag.Add(actImport, "<control>I");
			
			actQuit = CreateAction("quit", S._("_Quit"), null, Stock.Quit, OnActQuitActivated);
			ag.Add(actQuit, "<control>Q");			  
			
			RecentFilter filter = new RecentFilter();
			filter.AddApplication(App.Name);
			
			actRecentlyUsed = new RecentAction("recent_files", S._("Recent Databases"), null, null, recentManager);
			actRecentlyUsed.ShowNumbers = true;
			actRecentlyUsed.SortType = RecentSortType.Mru;
			actRecentlyUsed.AddFilter(filter);
			actRecentlyUsed.ItemActivated += OnActRecentlyUsedActivated;
			ag.Add(actRecentlyUsed, null);
			
			// edit menu
			actEdit = CreateAction("edit", S._("_Edit"), null, null, null);
			ag.Add(actEdit, null);
			
			actPreferences = CreateAction("preferences", S._("_Preferences"), null, Stock.Preferences, OnActPreferencesActivated);
			ag.Add(actPreferences, "<control>P");			 
			
			// help menu
			actHelp = CreateAction("help", S._("_Help"), null, null, null);
			ag.Add(actHelp, null);
			
			actInfo = CreateAction("info", S._("_Info"), null, Stock.About, OnActInfoActivated);
			ag.Add(actInfo);	
			
			// shared actions (used in toolbar buttons / menu items / context menus)
			actAddVolume = CreateAction("addvolume", S._("_Add Volume"), null, Stock.Add, OnActAddVolumeActivated);
			actAddVolume.IsImportant = true;
			ag.Add(actAddVolume, "<control>A");
			
			actRemoveVolume = CreateAction("removevolume", S._("_Remove Volume"), null, Stock.Remove, OnActRemoveVolumeActivated);
			ag.Add(actRemoveVolume, "<control>R");
			
			actEditVolume = CreateAction("editvolume", S._("_Edit Volume"), null, Stock.Edit, OnActEditVolumeActivated);
			ag.Add(actEditVolume, "<control>E");
			
			actSearch = CreateAction("searchitems", S._("_Search"), null, Stock.Find, OnActSearchActivated);
			ag.Add(actSearch, "<control>S");
			
			// context menus
			actEditItem = CreateAction("edititem", S._("Edit Item"), null, Stock.Edit, OnActEditItemActivated);
			ag.Add(actEditItem, null);
			
			actVolumesSortBy = CreateAction("volumes_sortby", S._("Sort by"), null, null, null);
			ag.Add(actVolumesSortBy, null);
			
			actVolumesSortDescending = CreateToggleAction("volumes_sortdescending", S._("Descending"), null, null, OnSortActionActivated);
			ag.Add(actVolumesSortDescending, null);
			
			RadioAction tmp = CreateRadioAction("volumes_sortby_archiveno", S._("Archive No."), null, null, (int)Widgets.VolumeSortProperty.ArchiveNo, null, OnSortActionActivated);
			volumeSortActions = new Gtk.RadioAction[] {
				tmp,
				CreateRadioAction("volumes_sortby_added", S._("Date added"), null, null, (int)Widgets.VolumeSortProperty.Added, tmp.Group, OnSortActionActivated),
				CreateRadioAction("volumes_sortby_title", S._("Title"), null, null, (int)Widgets.VolumeSortProperty.Title, tmp.Group, OnSortActionActivated),
				CreateRadioAction("volumes_sortby_drivetype", S._("Drivetype"), null, null, (int)Widgets.VolumeSortProperty.DriveType, tmp.Group, OnSortActionActivated),
				CreateRadioAction("volumes_sortby_category", S._("Category"), null, null, (int)Widgets.VolumeSortProperty.Category, tmp.Group, OnSortActionActivated)
			};
			
			// retrieve sort property from settings
			Widgets.VolumeSortProperty sp;
			bool desc;
			GetVolumeSortProperty(out sp, out desc);
			
			foreach (var a in volumeSortActions) {
				ag.Add(a, null);
				
				if (a.Value == (int)sp)
					a.Active = true;
			}
			actVolumesSortDescending.Active = desc;
			
			// ui manager
			UIManager manager = new UIManager();
			manager.InsertActionGroup(ag, 0);
			this.AddAccelGroup(manager.AccelGroup);
			
			string sortMenu = @"
			<menu action=""volumes_sortby"">
				<menuitem action=""volumes_sortby_archiveno""/>
				<menuitem action=""volumes_sortby_added""/>
				<menuitem action=""volumes_sortby_title""/>
				<menuitem action=""volumes_sortby_category""/>
				<menuitem action=""volumes_sortby_drivetype""/>
				<separator/>
				<menuitem action=""volumes_sortdescending""/>
			</menu>";
			
			string ui = string.Format(@"
			<ui>
				<menubar name=""menubar"">
					<menu action=""file"">
						<menuitem action=""newdb""/>
						<menuitem action=""opendb""/>
						<menuitem action=""open_default_db""/>
						<menuitem action=""recent_files""/>
						<separator/>
						<menuitem action=""dbproperties""/>
						<menuitem action=""import""/>
						<menuitem action=""searchitems""/>
						<separator/>
						<menuitem action=""quit""/>
					</menu>
					<menu action=""edit"">
						<menuitem action=""addvolume""/>
						<menuitem action=""removevolume""/>
						<menuitem action=""editvolume""/>
						<separator/>
						<menuitem action=""preferences""/>
					</menu>
					<menu action=""help"">
						<menuitem action=""info""/>
					</menu>
				</menubar>
				<toolbar name=""toolbar"">
					<toolitem action=""addvolume""/>
					<separator/>
					<toolitem action=""searchitems""/>
				</toolbar>
				<popup name=""volume_contextmenu"">
					<menuitem action=""editvolume""/>
					<menuitem action=""removevolume""/>
					<separator/>
					{0}
				</popup>
				<popup name=""volume_sort_contextmenu"">
					{0}
				</popup>
				<popup name=""item_contextmenu"">
					<menuitem action=""edititem""/>
				</popup>
			</ui>
			", sortMenu);
			
			manager.AddUiFromString(ui);
			
			menubar					= (MenuBar)manager.GetWidget("/menubar");
			toolbar					= (Toolbar)manager.GetWidget("/toolbar");
			volumeContextMenu		= (Menu)manager.GetWidget("/volume_contextmenu");
			volumeSortContextMenu	= (Menu)manager.GetWidget("/volume_sort_contextmenu");
			itemContextMenu			= (Menu)manager.GetWidget("/item_contextmenu");
			
			// gtk will use SmallToolbar on windows by default 
			// (no custom icons available for this size)
			toolbar.IconSize		= IconSize.LargeToolbar;
			toolbar.ToolbarStyle	= Gtk.ToolbarStyle.BothHoriz;
			toolbar.ShowArrow		= false;
			
			vbOuter.PackStart(menubar, false, false, 0);
			vbOuter.PackStart(toolbar, false, false, 0);
			
			// hpaned			 
			hpaned = new HPaned();
			hpaned.BorderWidth = 6;
			//hpaned.CanFocus = true;
			hpaned.Position = splitterPos;
			
			// left vbox			
			VBox vbLeft = new VBox();
			vbLeft.Spacing = 6;
			
			// left scrolled window / treeview (volumes)			
			ScrolledWindow swLeft = CreateScrolledView<Widgets.VolumeView>(out tvVolumes, true);
			vbLeft.PackStart(swLeft, true, true, 0);
			
			// volumes filter entry
			txtSearchString = new Widgets.SearchEntry();
			txtSearchString.PlaceholderText = S._("Filter volumes");
			
			txtSearchString.SetPresets(new Widgets.SearchEntryPreset[] {
				new Widgets.SearchEntryPreset(string.Format("title ({0})", S._("default")), "title:", null),
				new Widgets.SearchEntryPreset("loanedto", "loanedto:", null),
				new Widgets.SearchEntryPreset("description", "description:", null),
				new Widgets.SearchEntryPreset("keywords", "keywords:", null),
				new Widgets.SearchEntryPreset("files", "files", "< 10"),
				new Widgets.SearchEntryPreset("dirs", "dirs", "< 10"),
				new Widgets.SearchEntryPreset("size", "size", "> 700mb")
			});
			
			vbLeft.PackStart(txtSearchString, false, false, 0);
			
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
			
			vbOuter.PackStart(hpaned, true, true, 0);
			
			// statusbar
			statusbar = new Statusbar();
			statusbar.Spacing = 6;
			
			vbOuter.PackStart(statusbar, false, false, 0);
			
			this.Add(vbOuter);
			
			// eventhandlers
			txtSearchString.Search			+= OnTxtSearchStringSearch;
			tvVolumes.Selection.Changed		+= OnTvVolumesSelectionChanged;
			tvVolumes.ButtonPressEvent		+= OnTvVolumesButtonPressEvent;
			tvVolumes.KeyPressEvent			+= OnTvVolumesKeyPressEvent;
			
			tvItems.ButtonPressEvent		+= OnTvItemsButtonPressEvent;
			tvItems.KeyPressEvent			+= OnTvItemsKeyPressEvent;
			tvItems.Selection.Changed		+= OnTvItemsSelectionChanged;
			
			this.DeleteEvent				+= OnDeleteEvent;
			
			ShowAll();

			// must be called _after_ ShowAll()
			itemInfo.Minimized = itemInfoMinimized;
			itemInfo.Hide();
			
			buildGuiCompleted = true;
		}
	}
}
