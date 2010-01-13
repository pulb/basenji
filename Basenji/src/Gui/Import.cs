/// Import.cs
// 
// Copyright (C) 2010 Patrick Ulbrich
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
		
		private VolumeDatabase database;
		private IImport import;
		
		public Import (VolumeDatabase db) {
			this.database = db;
			this.import = null;
			
			BuildGui();
		}
		
		public event NewVolumeAddedEventHandler NewVolumeAdded;
		
		protected virtual void OnNewVolumeAdded() {
			if (NewVolumeAdded != null)
				NewVolumeAdded(this, new NewVolumeAddedEventArgs(null));
		}
		
		private void OnBtnImportClicked(object sender, EventArgs e) {
			if (import != null && import.IsBusy) {
				
				import.CancelAsync();
				btnImport.Sensitive = false;
				
			} else {
				
				progress.Fraction = .0;				
				progress.Text = string.Empty;
				
				if (rdGnomeCatalog.Active) {
					import = new GnomeCatalogImport(database, 
					                                DbData.GetDbDataPath(database));	
				} else if (rdCdCollect.Active) {
					import = new CdCollectImport(database,
					                             DbData.GetDbDataPath(database));
				} else { // basenji import
					import = new BasenjiImport(database,
					                           null,
					                           DbData.GetDbDataPath(database));
				}
				
				import.ProgressUpdate	+= OnImportProgressUpdate;
				import.ImportCompleted	+= OnImportCompleted;
				
				import.RunAsync();
				btnImport.Label = LBL_ABORT;
				btnClose.Sensitive = false;
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
						OnNewVolumeAdded();
					}				
				});
			} finally {
				Application.Invoke(delegate {
					btnClose.Sensitive = true;
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
	}
	
	// gui initialization
	public partial class Import : Base.WindowBase
	{		
		private RadioButton rdGnomeCatalog;
		private RadioButton rdCdCollect;
		private RadioButton rdBasenji;
		
		private FileChooserButton fcBasenji;
		private ProgressBar progress;
		private Button btnImport;
		private Button btnClose;
			
		protected override void BuildGui() {
			base.BuildGui();
			
			// general window settings
			SetDialogStyle();
			this.DefaultWidth		= 320;
			this.Title				= S._("Import Database");
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.BorderWidth = 0;
			vbOuter.Spacing = 0;
			
			// vbOptions
			VBox vbOptions = new VBox();
			vbOptions.BorderWidth = 12;
			vbOptions.Spacing = 6;
			
			rdGnomeCatalog = new RadioButton(null, "GnomeCatalog");
			vbOptions.PackStart(rdGnomeCatalog, true, true, 0);
			
			rdCdCollect = new RadioButton(rdGnomeCatalog, "CDCollect");
			vbOptions.PackStart(rdCdCollect, true, true, 0);
			
			HBox hbBasenji = new HBox();
			hbBasenji.Spacing = 12;
			
			rdBasenji = new RadioButton(rdGnomeCatalog, "Basenji");			
			fcBasenji = new FileChooserButton(S._("Please select a database to import"),
			                                                    FileChooserAction.Open);
			
			hbBasenji.PackStart(rdBasenji, false, false, 0);
			hbBasenji.PackStart(fcBasenji, true, true, 0);
			
			vbOptions.PackStart(hbBasenji, true, false, 0);
			
			vbOuter.PackStart(vbOptions, true, true, 0);
			
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
			
			vbOuter.PackStart(hbClose, true, true, 0);
			
			this.Add(vbOuter);
			
			// disable unimplemented options
			rdBasenji.Sensitive = false;
			fcBasenji.Sensitive = false;
			rdCdCollect.Sensitive = false;
			
			// event handlers
			this.DeleteEvent += OnDeleteEvent;
			
			ShowAll();
		}
	}
}
