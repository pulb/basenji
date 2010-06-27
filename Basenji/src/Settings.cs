// Settings.cs
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
using System.IO;
using System.Collections.Generic;
using Platform.Common.Diagnostics;

namespace Basenji
{	
	public class Settings
	{		
		private const string SETTINGS_FILE			= "settings";
		private const string NON_GNOME_CUSTOM_THEME	= "Tango";
		
		private Dictionary<string, string> properties;
		
		public Settings() : this(false) {}
		
		private Settings(bool defaults) {
			Reset();
			if (!defaults)
				Load(false);
		}
		
		public string ScannerDevice {
			get { return properties["ScannerDevice"]; }
			set { properties["ScannerDevice"] = value; }
		}
		
		public int ScannerBufferSize {
			get { return int.Parse(properties["ScannerBufferSize"]); }
			set { properties["ScannerBufferSize"] = value.ToString(); }
		}
		
		public bool ScannerDiscardSymLinks {
			get { return properties["ScannerDiscardSymLinks"] == "1"; }
			set { properties["ScannerDiscardSymLinks"] = value ? "1" : "0"; }
		}
		
		public bool ScannerComputeHashs {
			get { return properties["ScannerComputeHashs"] == "1"; }
			set { properties["ScannerComputeHashs"] = value ? "1" : "0"; }
		}
		
		public bool ScannerGenerateThumbnails {
			get { return properties["ScannerGenerateThumbnails"] == "1"; }
			set { properties["ScannerGenerateThumbnails"] = value ? "1" : "0"; }
		}

		public bool ScannerExtractMetaData {
			get { return properties["ScannerExtractMetaData"] == "1"; }
			set { properties["ScannerExtractMetaData"] = value ? "1" : "0"; }
		}
		
		public string ScannerExtractionBlacklist {
			get { return properties["ScannerExtractionBlacklist"]; }
			set { properties["ScannerExtractionBlacklist"] = value; }
		}
		
		public bool ScannerEnableMusicBrainz {
			get { return properties["ScannerEnableMusicBrainz"] == "1"; }
			set { properties["ScannerEnableMusicBrainz"] = value ? "1" : "0"; }
		}
		
		public bool OpenMostRecentDB {
			get { return properties["OpenMostRecentDB"] == "1"; }
			set { properties["OpenMostRecentDB"] = value ? "1" : "0"; }
		}
		
		public string MostRecentDBPath {
			get { return properties["MostRecentDBPath"]; }
			set { properties["MostRecentDBPath"] = value; }
		}
		
		public bool ShowThumbsInItemLists {
			get { return properties["ShowThumbsInItemLists"] == "1"; }
			set { properties["ShowThumbsInItemLists"] = value ? "1" : "0"; }
		}
		
		public int MainWindowWidth {
			get { return int.Parse(properties["MainWindowWidth"]); }
			set { properties["MainWindowWidth"] = value.ToString(); }
		}
		
		public int MainWindowHeight {
			get { return int.Parse(properties["MainWindowHeight"]); }
			set { properties["MainWindowHeight"] = value.ToString(); }
		}
		
		public bool MainWindowIsMaximized {
			get { return properties["MainWindowIsMaximized"] == "1"; }
			set { properties["MainWindowIsMaximized"] = value ? "1" : "0"; }
		}

		public bool ItemInfoMinimized1 {
			get { return properties["ItemInfoMinimized1"] == "1"; }
			set { properties["ItemInfoMinimized1"] = value ? "1" : "0"; }
		}

		public bool ItemInfoMinimized2 {
			get { return properties["ItemInfoMinimized2"] == "1"; }
			set { properties["ItemInfoMinimized2"] = value ? "1" : "0"; }
		}
		
		public int MainWindowSplitterPosition {
			get { return int.Parse(properties["MainWindowSplitterPosition"]); }
			set { properties["MainWindowSplitterPosition"] = value.ToString(); }
		}
		
		public string CustomThemeLocation {
			get { return properties["CustomThemeLocation"]; }
			set { properties["CustomThemeLocation"] = value; }
		}
		
		public string CustomThemeName {
			get { return properties["CustomThemeName"]; }
			set { properties["CustomThemeName"] = value; }
		}
		
		public int SearchResultPageSize {
			get { return int.Parse(properties["SearchResultPageSize"]); }
			set { properties["SearchResultPageSize"] = value.ToString(); }
		}
		
		public int VolumeSortProperty {
			get { return int.Parse(properties["VolumeSortProperty"]); }
			set { properties["VolumeSortProperty"] = value.ToString(); }
		}
		
		// restore default settings
		public void Reset() {
			properties = new Dictionary<string, string>();
			
			properties.Add("ScannerDevice",					"");
			properties.Add("ScannerBufferSize",				"10"); // WARNING: the higher the value, the longer it will take the cancellation (triggered by CancelAsync()) to complete.
			properties.Add("ScannerDiscardSymLinks",		"0");			 
			properties.Add("ScannerComputeHashs",			"0");
			properties.Add("ScannerGenerateThumbnails",		"1");
			properties.Add("ScannerExtractMetaData",		"1");
			properties.Add("ScannerExtractionBlacklist",	""); // e.g. "pdf, mp3"
			properties.Add("ScannerEnableMusicBrainz",		"1");
			properties.Add("OpenMostRecentDB",				"1");
			properties.Add("MostRecentDBPath",				"");
			properties.Add("ShowThumbsInItemLists",			"0");
			properties.Add("MainWindowWidth",				"800");
			properties.Add("MainWindowHeight",				"480");
			properties.Add("MainWindowIsMaximized",			"0");
			properties.Add("ItemInfoMinimized1",			"0");
			properties.Add("ItemInfoMinimized2",			"0");
			properties.Add("MainWindowSplitterPosition",	"260");
			properties.Add("CustomThemeLocation",			Path.Combine("data", "themes"));
			properties.Add("CustomThemeName",				CurrentPlatform.IsGnome ? "" : NON_GNOME_CUSTOM_THEME);
			properties.Add("SearchResultPageSize",			"10");
			properties.Add("VolumeSortProperty",			((int)Gui.Widgets.VolumeSortProperty.Added).ToString());
		}
		
		// returns a Settings instance with defaults loaded
		public /*static*/ Settings GetDefaults() {
			return new Settings(true);
		}
		
		// returns the directory where the settingsfile is located
		public /*static*/ DirectoryInfo GetSettingsDirectory() {
			return new DirectoryInfo(GetSettingsPath());
		}
		
		// determines if the settingsfile exists (has been written yet)		   
		public /*static*/ bool SettingsFileExists() {
			return File.Exists(Path.Combine(GetSettingsPath(), SETTINGS_FILE));		   
		}
		
		public void Save() {
			// writes in UTF-8 encoding
			using (StreamWriter sw = new StreamWriter(Path.Combine(GetSettingsPath(), SETTINGS_FILE), false)) {
				foreach(KeyValuePair<string, string> pair in properties)
					sw.WriteLine("{0} = {1}", pair.Key, pair.Value);
			}
		}
		
		public void Reload() {
			Load(true);
		}
		
		private void Load(bool throwException) {
			if (!SettingsFileExists()) {
				if (throwException)
					throw new FileNotFoundException("Settingsfile not found. Settings have probably not been saved yet.");
				return;
			}
			
			using(StreamReader sr = new StreamReader(Path.Combine(GetSettingsPath(), SETTINGS_FILE))) {
				string line;
				while((line = sr.ReadLine()) != null) {
					string[] pair = line.Split('=');
					
					if (pair.Length != 2)
						continue;
					
					string key = pair[0].Trim();
					string value = pair[1].Trim();

					if (properties.ContainsKey(key))
						properties[key] = value;
				}
			}
			
			//return true;
		}
		
		private static string GetSettingsPath() {
			// support a local settings dir for users that want to
			// carry Basenji around on a USB key.
			string localSettingsPath = Path.Combine(Environment.CurrentDirectory,
			                                        "app_data");
			
			if (Directory.Exists(localSettingsPath)) {
				return localSettingsPath;
			} else {
				string appDataPath	= Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				string settingsPath = Path.Combine(appDataPath, App.Name);
	
				if (!Directory.Exists(settingsPath))
					Directory.CreateDirectory(settingsPath);
				
				return settingsPath;
			}
		}
	}
}
