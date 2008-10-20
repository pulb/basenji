// Preferences.cs
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

// TODO:
// alternate drive label captions:
// "Scan without promt(ing)" "Ohne Nachfrage scannen"
// "Don't prompt, just scan", "Frage nicht, scanne einfach:"
// "Always scan this drive:" "Immer dieses Laufwerk scannen:"
// "Scan this drive automatically:"

using System;
using System.IO;
using System.Threading;
using Gtk;
using Gdk;
using PlatformIO = Platform.Common.IO;

namespace Basenji.Gui
{
	public partial class Preferences : Base.WindowBase
	{
		private const string SYSTEM_ICON_THEME_NAME = "System";
		
		private bool iconThemeChanged;
		private volatile bool loadingComplete;
			
//		public Preferences() {
//			BuildGui();
//			  FillIconThemes();			
//			  FillDrives();
//			ShowSettings(App.Settings);
//			  iconThemeChanged = false; // must be set after FillIconThemes()!
//		}
		
		public Preferences() {
			BuildGui();
			
			// disable the gui until async drive refreshing has been completed
			this.Child.Sensitive = false;
			loadingComplete = false;			
			
			FillIconThemes();
			
			// drive refreshing is implemented asynchronously, 
			// because this operation can take a few seconds on windows systems.
			// ShowSettings() depends on FillDrives() so it must be called in the same thread.
			new Thread(delegate() {
				Application.Invoke(delegate {				 
					FillDrives();
					ShowSettings(App.Settings);
					iconThemeChanged = false; // must be set after FillIconThemes()!
					
					this.Child.Sensitive = true;
					loadingComplete = true;
				});
				
			}).Start();
			
		}

		private void OnBtnCloseClicked(object sender, System.EventArgs e) {
			Save();
			this.Destroy(); // TODO : not neccessary?
		}
		
		private void OnBtnResetClicked(object sender, System.EventArgs e) {
			ShowSettings(App.Settings.GetDefaults());
		}
		
		private void OnDeleteEvent(object o, Gtk.DeleteEventArgs args) {
			if (loadingComplete) {			  
				Save();
				this.Destroy(); // TODO : not neccessary?
			} else {
				args.RetVal = true; // cancel window deleting		   
			}
		}
		
		private void OnCmbIconThemeChanged(object o, EventArgs args) {
			iconThemeChanged = true;
		}
		
		private void FillDrives() {
			ListStore store = new ListStore(typeof(Pixbuf), typeof(string));
			PlatformIO.DriveInfo[] drives = PlatformIO.DriveInfo.GetDrives(false); // get drives that are _not_ ready, too.

			//string stockID;
			Pixbuf icon;
			
			//stockID = Stock.Cancel;
			icon = RenderIcon(Icons.Icon.Stock_Cancel, IconSize.Button);
			store.AppendValues(icon, S._("None"));
			
			foreach(PlatformIO.DriveInfo d in drives) {
				// list removable drives only (auto scanning of fixed drives does not make sense)
				if (d.DriveType == PlatformIO.DriveType.CDRom || d.DriveType == PlatformIO.DriveType.Removable) {
					//stockID = Util.GetDriveStockIconID(d);
					//icon = this.RenderIcon(stockID, IconSize.Button, string.Empty);
					icon = RenderIcon(Icons.IconUtils.GetDriveIcon(d), IconSize.Button);
					string text = d.Device;
					
					store.AppendValues(icon, text);
				}
			}
			
			CellRendererPixbuf pixbufCellRenderer = new CellRendererPixbuf();			 
			CellRendererText textCellRenderer = new CellRendererText();
			textCellRenderer.Xpad = 6;
			
			cmbScannerDevice.PackStart(pixbufCellRenderer, false);
			cmbScannerDevice.PackStart(textCellRenderer, false);
			
			cmbScannerDevice.AddAttribute(pixbufCellRenderer, "pixbuf", 0);
			cmbScannerDevice.AddAttribute(textCellRenderer, "text", 1);
			
			cmbScannerDevice.Model = store;
		}
		
		private void FillIconThemes() {
			cmbIconTheme.AppendText(SYSTEM_ICON_THEME_NAME);
			if (!string.IsNullOrEmpty(App.Settings.CustomThemeLocation) && Directory.Exists(App.Settings.CustomThemeLocation)) {
				DirectoryInfo[] customThemeDirs = (new DirectoryInfo(App.Settings.CustomThemeLocation)).GetDirectories();
				foreach(DirectoryInfo dir in customThemeDirs)
					cmbIconTheme.AppendText(dir.Name);
			}
		}
		
		private void ShowSettings(Basenji.Settings s) {
			TreeModel model;
			TreeIter iter;
			
			/*
			 * general settings
			 */
			string customThemeName = s.CustomThemeName; 
			model = cmbIconTheme.Model;
			
			// select "System" item
			cmbIconTheme.Active = 0;
			
			if (customThemeName.Length > 0) {
				// select custom icon theme
				for (int i = 0; i < model.IterNChildren(); i++) {				 
					model.IterNthChild(out iter, i);
					if ((string)model.GetValue(iter, 0) == customThemeName) {
						cmbIconTheme.SetActiveIter(iter);
						break;
					}					 
				}
			}
			
			chkReopenDB.Active = s.OpenMostRecentDB;
			
			/*
			 * scanner settings
			 */
			string scannerDevice = s.ScannerDevice;
			model = cmbScannerDevice.Model;
			
			// select "none" device
			model.GetIterFirst(out iter);
			cmbScannerDevice.SetActiveIter(iter);
			
			if (scannerDevice.Length > 0) {
				// select settings device
				for (int i = 0; i < model.IterNChildren(); i++) {				 
					model.IterNthChild(out iter, i);
					if ((string)model.GetValue(iter, 1) == scannerDevice) {
						cmbScannerDevice.SetActiveIter(iter);
						break;
					}					 
				}
			}
			
//			  scaleBufferSize.Value		  	= s.ScannerBufferSize;
			chkGenerateThumbnails.Active	= s.ScannerGenerateThumbnails;
			chkDiscardSymLinks.Active		= s.ScannerDiscardSymLinks;
			chkComputeHashs.Active			= s.ScannerComputeHashs;
		}
		
		private void Save() {
			/*
			 * general settings
			 */
			App.Settings.CustomThemeName = (cmbIconTheme.ActiveText == SYSTEM_ICON_THEME_NAME) ? string.Empty : cmbIconTheme.ActiveText;
			App.Settings.OpenMostRecentDB = chkReopenDB.Active;
			
			/*
			 * scanner settings
			 */				
			string scannerDevice = string.Empty;
			
			if (cmbScannerDevice.Active > 0) {
				TreeIter iter;
				cmbScannerDevice.GetActiveIter(out iter);
				scannerDevice = (string)cmbScannerDevice.Model.GetValue(iter, 1);
			}
			App.Settings.ScannerDevice = scannerDevice;
			
//			  App.Settings.ScannerBufferSize	  = (int)scaleBufferSize.Value;
			App.Settings.ScannerGenerateThumbnails	= chkGenerateThumbnails.Active;
			App.Settings.ScannerDiscardSymLinks 	= chkDiscardSymLinks.Active;
			App.Settings.ScannerComputeHashs		= chkComputeHashs.Active;
			
			App.Settings.Save();
			
			if (iconThemeChanged)			 
				MsgDialog.Show(this, MessageType.Info, ButtonsType.Ok, S._("Restart required"), string.Format(S._("You must restart {0} for icontheme changes to take effect."), App.Name));
		}
	}
	
	// gui initialization
	public partial class Preferences : Base.WindowBase
	{
		private CheckButton chkReopenDB;
		private ComboBox	cmbIconTheme;
		
		private ComboBox	cmbScannerDevice;
//		private HScale		scaleBufferSize;
		private CheckButton chkGenerateThumbnails;
		private CheckButton chkDiscardSymLinks;
		private CheckButton chkComputeHashs;
		private Button		btnReset;
		private Button		btnClose;
		
		protected override void BuildGui() {
			base.BuildGui();
			
			//general window settings
			this.DefaultWidth		= 400;
			this.DefaultHeight		= 400;
			this.Modal				= true;
			this.SkipTaskbarHint	= true;
			this.Title				= S._("Preferences");
			this.Icon				= this.RenderIcon(Basenji.Icons.Icon.Stock_Preferences, IconSize.Menu);
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.Spacing = 24;
			
			// notebook
			Notebook nb = new Notebook();
			nb.CurrentPage = 0;
			
			AppendGeneralPage(nb);
			AppendScannerPage(nb);
			
			vbOuter.PackStart(nb, true, true, 0);
			
			// hbuttonbox
			HButtonBox bbox = new HButtonBox();
			//bbox.Spacing = 6
			
			// reset button
			btnReset = CreateCustomButton(RenderIcon(Icons.Icon.Stock_Clear, IconSize.Button), S._("_Load Defaults"), OnBtnResetClicked);

			bbox.PackStart(btnReset, false, false, 0);
			
			// close buton
			btnClose = CreateButton(Stock.Close, true, OnBtnCloseClicked);

			bbox.PackStart(btnClose, false, false, 0);
			
			vbOuter.PackStart(bbox, false, false, 0);
			
			this.Add(vbOuter);
			
			// event handlers
			cmbIconTheme.Changed	+= OnCmbIconThemeChanged;
			this.DeleteEvent		+= OnDeleteEvent;
			
			ShowAll();
		}
		
		private void AppendGeneralPage(Notebook nb) {
			Table tbl = CreateTable(2, 2);
			tbl.BorderWidth = 12;			 
			
			// label
			TblAttach(tbl, CreateLabel(S._("Icon theme:")), 0, 0);  
			// combobox icon theme
			cmbIconTheme = ComboBox.NewText();
			TblAttach(tbl, cmbIconTheme, 1, 0, AttachOptions.Expand | AttachOptions.Fill | AttachOptions.Shrink, AttachOptions.Fill);
			
			// reopen db checkbox
			chkReopenDB = new CheckButton(S._("Reopen most recent database on startup"));
			TblAttach(tbl, chkReopenDB, 0, 1, 2, 1);
			
			nb.AppendPage(tbl, new Label(S._("General")));		 
		}
		
		private void AppendScannerPage(Notebook nb) {
			Table tbl = CreateTable(4, 2);
			tbl.BorderWidth = 12;
			
			// labels
			TblAttach(tbl, CreateLabel(S._("Don't prompt, always scan:")), 0, 0);			
//			  TblAttach(tbl, CreateLabel("Buffersize:"), 0, 1);
			
			// combobox scannerdevice
			cmbScannerDevice = new ComboBox();
			TblAttach(tbl, cmbScannerDevice, 1, 0, AttachOptions.Expand | AttachOptions.Fill | AttachOptions.Shrink, AttachOptions.Fill);
			
//			  // scale buffersize
//			  scaleBufferSize = new HScale(null);
//			  scaleBufferSize.Adjustment.Lower = 1;
//			  scaleBufferSize.Adjustment.Upper = 100;
//			  scaleBufferSize.Adjustment.PageIncrement = 10;
//			  scaleBufferSize.Adjustment.StepIncrement = 1;
//			  scaleBufferSize.DrawValue = true;
//			  scaleBufferSize.Digits = 0;
//			  
//			  TblAttach(tbl, scaleBufferSize, 1, 1, AttachOptions.Expand | AttachOptions.Fill | AttachOptions.Shrink, AttachOptions.Fill);
			
			// checkbox generateThumbnails
			chkGenerateThumbnails = new CheckButton(S._("Generate Thumbnails"));
			TblAttach(tbl, chkGenerateThumbnails, 0, 1, 2, 1, AttachOptions.Fill, AttachOptions.Fill);
			// FIXME : currently thumbnail generation is implemented for GNOME only
			if (!Platform.Common.Diagnostics.CurrentPlatform.IsGnome)
				chkGenerateThumbnails.Sensitive = false;
			
			// checkbox discardSymLinks
			chkDiscardSymLinks = new CheckButton(S._("Discard symbolic links"));
			TblAttach(tbl, chkDiscardSymLinks, 0, 2, 2, 1, AttachOptions.Fill, AttachOptions.Fill);
			
			// checkbox computeHashs
			chkComputeHashs = new CheckButton(S._("Compute hashcodes for files (slow!)"));
			TblAttach(tbl, chkComputeHashs, 0, 3, 2, 1, AttachOptions.Fill, AttachOptions.Fill);			
			
			nb.AppendPage(tbl, new Label(S._("Scanner")));		 
		}

	}
}
