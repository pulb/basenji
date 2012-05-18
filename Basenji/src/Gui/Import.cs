/// Import.cs
// 
// Copyright (C) 2010, 2012 Patrick Ulbrich
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
using VolumeDB;
using VolumeDB.Import;

namespace Basenji.Gui
{
	public partial class Import : Base.WindowBase
	{
		private static readonly string LBL_IMPORT = S._("Import");
		private static readonly string LBL_ABORT = S._("Abort");
		private static readonly string LBL_FORMAT_EMPTY = S._("Please select a database.");
		
		private VolumeDatabase database;
		private IImport import;
		
		public Import (VolumeDatabase db) {
			this.database = db;
			this.import = null;
			
			BuildGui();
			btnImport.Sensitive = false; // will be enabled on file selection
		}
		
		public event EventHandler VolumesImported;
		
		protected virtual void OnVolumesImported() {
			if (VolumesImported != null)
				VolumesImported(this, new EventArgs());
		}
		
		private void OnBtnImportClicked(object sender, EventArgs e) {
			if (import != null && import.IsBusy) {
				
				import.CancelAsync();
				btnImport.Sensitive = false;
				
			} else {
				
				progress.Fraction = .0;				
				progress.Text = string.Empty;
				
				import.RunAsync();
				btnImport.Label = LBL_ABORT;
				btnClose.Sensitive = false;
				fcDatabase.Sensitive = false;
			}
		}
		
		private void OnImportProgressUpdate(object sender, ProgressUpdateEventArgs e) {
			Application.Invoke(delegate {
				progress.Fraction = e.Completed / 100.0;
				progress.Text = string.Format(S._("{0:0}% completed."),
				                              e.Completed);
			});
		}	
		
		private void OnImportCompleted(object sender, ImportCompletedEventArgs e) {
			try {
				Application.Invoke(delegate {
					if (e.Error != null) {
						if (e.Error is System.IO.FileNotFoundException) {
							MsgDialog.ShowError(this, S._("Import failed"),
							                    S._("Database not found."));
						}
						progress.Text = S._("Import failed!");
					} else if (e.Cancelled) {
						progress.Text = S._("Import aborted.");
					} else {
						progress.Text = S._("Import completed successfully.");
						OnVolumesImported();
					}				
				});
			} finally {
				Application.Invoke(delegate {
					btnClose.Sensitive = true;
					fcDatabase.Sensitive = true;
					btnImport.Sensitive = true;
					btnImport.Label = LBL_IMPORT;
				});
			}
		}	
		
		private void OnBtnCloseClicked(object sender, EventArgs e) {
			this.Destroy();
		}
		
		private void OnDeleteEvent(object o, Gtk.DeleteEventArgs args) {
			if (import != null && import.IsBusy) {		 
				MsgDialog.ShowError(this, S._("Import in progress"),
				                    S._("You must stop the import before closing this window."));
				args.RetVal = true;
			}
		}
		
		private void OnFcDatabaseSelectionChanged (object sender, EventArgs e) {
			
			if (string.IsNullOrEmpty(fcDatabase.Filename)) {
				import = null;
				lblFormat.Text = LBL_FORMAT_EMPTY;
				btnImport.Sensitive = false;
				return;
			}
			
			string sourceDbPath = fcDatabase.Filename;
			string dbDataPath = PathUtil.GetDbDataPath(database);
			int buffSize = App.Settings.ScannerBufferSize;
			string ext = System.IO.Path.GetExtension(sourceDbPath);
			
			if (ext.Length == 0)
				import = null;
			else
				import = AbstractImport.GetImportByExtension(ext.Substring(1), sourceDbPath, 
				                                              database, dbDataPath, buffSize);
			
			if (import == null) {
				lblFormat.Text = S._("Unknown format.");
				btnImport.Sensitive = false;
			} else {
				import.ProgressUpdate	+= OnImportProgressUpdate;
				import.ImportCompleted	+= OnImportCompleted;
				
				lblFormat.Text = import.Name;
				btnImport.Sensitive = true;
			}
		}
	}
	
	// gui initialization
	public partial class Import : Base.WindowBase
	{		
		private FileChooserButton	fcDatabase;
		private Label				lblFormat;
		private ProgressBar			progress;
		private Button				btnImport;
		private Button				btnClose;
			
		protected override void BuildGui() {
			base.BuildGui();
			
			// general window settings
			SetModal();
			//this.DefaultWidth	= 320;
			this.Title			= S._("Import Database");
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.BorderWidth = 0;
			vbOuter.Spacing = 0;
			
			// tblDatabase
			Table tblDatabase = WindowBase.CreateTable(2, 2);
			tblDatabase.BorderWidth = 12;
			
			fcDatabase = new FileChooserButton(S._("Please select a database to import"),
			                               FileChooserAction.Open);
			
			// set min width of the filechooser widget
			// (translated labels may make it smaller otherwise)
			fcDatabase.WidthRequest = 220;
			
			FileFilter allFilter = new FileFilter();
			allFilter.Name = S._("All supported formats");
			fcDatabase.AddFilter(allFilter);
			foreach (var ext in AbstractImport.GetSupportedExtensions()) {
				FileFilter ff = new FileFilter();
				ff.Name = string.Format(S._ (".{0} files"), ext);
				ff.AddPattern("*." + ext);
				fcDatabase.AddFilter(ff);
				allFilter.AddPattern("*." + ext);
			}
			
			lblFormat = WindowBase.CreateLabel(LBL_FORMAT_EMPTY);
			
			AttachOptions xAttachOpts = AttachOptions.Expand | AttachOptions.Fill;
			AttachOptions yAttachOpts = AttachOptions.Fill;
			
			WindowBase.TblAttach(tblDatabase, WindowBase.CreateLabel(S._("Database:")), 0, 0);
			WindowBase.TblAttach(tblDatabase, WindowBase.CreateLabel(S._("Format:")), 0, 1);
			WindowBase.TblAttach(tblDatabase, fcDatabase, 1, 0, xAttachOpts, yAttachOpts);
			WindowBase.TblAttach(tblDatabase, lblFormat, 1, 1, xAttachOpts, yAttachOpts);
			
			vbOuter.PackStart(tblDatabase, true, true, 0);

			// progressbar and button
			HBox hbProgress = new HBox();
			hbProgress.BorderWidth = 12;
			hbProgress.Spacing = 6;
			
			progress = new ProgressBar();
			hbProgress.PackStart(progress, true, true, 0);
			
			btnImport = WindowBase.CreateButton(LBL_IMPORT, false, OnBtnImportClicked);
			hbProgress.PackStart(btnImport, false, false, 0);
			
			vbOuter.PackStart(hbProgress, true, false, 0);
			
			// separator
			vbOuter.PackStart(new HSeparator(), true, true, 0);
			
			// close button
			HBox hbClose = new HBox();
			hbClose.BorderWidth = 12;
			hbClose.Spacing = 6;
			
			btnClose = WindowBase.CreateButton(Stock.Close, true, OnBtnCloseClicked);
			hbClose.PackEnd(btnClose, false, false, 0);
			
			vbOuter.PackStart(hbClose, false, false, 0);
			
			this.Add(vbOuter);
			
			// event handlers
			this.DeleteEvent += OnDeleteEvent;
			fcDatabase.SelectionChanged += OnFcDatabaseSelectionChanged;
			
			ShowAll();
		}
	}
}
