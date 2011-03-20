// Settings.cs
// 
// Copyright (C) 2008 - 2011 Patrick Ulbrich
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
using System.Reflection;

namespace Basenji
{
	public class Settings
	{
		private const string SETTINGS_FILE			= "settings";
		private const string NON_GNOME_CUSTOM_THEME	= "Tango";
		private const int DEFAULT_SORT_PROPERTY		= (int)Gui.Widgets.VolumeSortProperty.Added;
		
		[SettingsAttribute("ScannerDevice", "")]
		public string ScannerDevice { get; set; }
		
		// WARNING: the higher the value, the longer it 
		// will take the cancellation (triggered by CancelAsync()) to complete.
		[SettingsAttribute("ScannerBufferSize", "10")]
		public int ScannerBufferSize { get; set; }
		
		[SettingsAttribute("ScannerDiscardSymLinks", "0")]
		public bool ScannerDiscardSymLinks  { get; set; }
		
		[SettingsAttribute("ScannerComputeHashs", "0")]
		public bool ScannerComputeHashs { get; set; }
		
		[SettingsAttribute("ScannerGenerateThumbnails", "1")]
		public bool ScannerGenerateThumbnails { get; set; }

		[SettingsAttribute("ScannerExtractMetaData", "1")]
		public bool ScannerExtractMetaData { get; set; }
		
		// 0 = taglib-sharp, 1 = libextractor 0.5.x
		[SettingsAttribute("ScannerMetaDataProvider", "0")]
		public int ScannerMetaDataProvider { get; set; }
		
		// e.g. "pdf, mp3" (used by libextractor metadata provider only)
		[SettingsAttribute("ScannerExtractionBlacklist", "")]
		public string ScannerExtractionBlacklist { get; set; }
		
		[SettingsAttribute("ScannerEnableMusicBrainz", "1")]
		public bool ScannerEnableMusicBrainz { get; set; }
		
		[SettingsAttribute("OpenMostRecentDB", "1")]
		public bool OpenMostRecentDB { get; set; }
		
		[SettingsAttribute("MostRecentDBPath", "")]
		public string MostRecentDBPath { get; set; }
		
		[SettingsAttribute("ShowThumbsInItemLists", "0")]
		public bool ShowThumbsInItemLists { get; set; }
		
		[SettingsAttribute("MainWindowWidth", "800")]
		public int MainWindowWidth { get; set; }
		
		[SettingsAttribute("MainWindowHeight", "480")]
		public int MainWindowHeight { get; set; }
		
		[SettingsAttribute("MainWindowIsMaximized", "0")]
		public bool MainWindowIsMaximized { get; set; }
		
		[SettingsAttribute("ShowItemInfo", "1")]
		public bool ShowItemInfo { get; set; }
		
		[SettingsAttribute("ShowHiddenItems", "0")]
		public bool ShowHiddenItems { get; set; }
		
		[SettingsAttribute("ItemInfoMinimized1", "0")]
		public bool ItemInfoMinimized1 { get; set; }

		[SettingsAttribute("ItemInfoMinimized2", "0")]
		public bool ItemInfoMinimized2 { get; set; }
		
		[SettingsAttribute("MainWindowSplitterPosition", "260")]
		public int MainWindowSplitterPosition { get; set; }
		
		[SettingsAttribute("CustomThemeName", "")]
		public string CustomThemeName { get; set; }
		
		[SettingsAttribute("SearchResultPageSize", "10")]
		public int SearchResultPageSize { get; set; }
		
		[SettingsAttribute("VolumeSortProperty", "3")]
		public int VolumeSortProperty { get; set; }
		
		public Settings() : this(false) {}
		
		private Settings(bool defaults) {
			Reset();
			if (!defaults)
				Load(false);
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
		
		// restore default settings
		public void Reset() {
			PropertyInfo[] propInfos = GetProperties();
			
			foreach (PropertyInfo pi in propInfos) {
				Attribute[] atts = (Attribute[])pi.GetCustomAttributes(typeof(SettingsAttribute), false);
				
				// check if the property has the SettingsAttribute
				if (atts.Length == 0)
					continue;
				
				SettingsAttribute sa = (SettingsAttribute)atts[0];
				SetPropertyValueFromString(pi, sa.DefaultValue);
			}
			
			// manually override with settings determined at runtime
			CustomThemeName = CurrentPlatform.IsGnome ? "" : NON_GNOME_CUSTOM_THEME;
			VolumeSortProperty = DEFAULT_SORT_PROPERTY;
		}
		
		public void Save() {
			// writes in UTF-8 encoding
			using (StreamWriter sw = new StreamWriter(Path.Combine(GetSettingsPath(), SETTINGS_FILE), false)) {
				PropertyInfo[] propInfos = GetProperties();
				
				foreach (PropertyInfo pi in propInfos) {
					Attribute[] atts = (Attribute[])pi.GetCustomAttributes(typeof(SettingsAttribute), false);
					
					// check if the property has the SettingsAttribute
					if (atts.Length == 0)
						continue;
					
					SettingsAttribute sa = (SettingsAttribute)atts[0];
					string val;
					
					if (pi.PropertyType == typeof(bool))
						val = (((bool)pi.GetValue(this, null)) ? "1" : "0");
					else
						val = pi.GetValue(this, null).ToString();
					
					sw.WriteLine("{0} = {1}", sa.Name, val);					
				}
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
			
			Dictionary<string, string> settings = new Dictionary<string, string>();
			
			// read settings in a dictionary for faster access
			using (StreamReader sr = new StreamReader(Path.Combine(GetSettingsPath(), SETTINGS_FILE))) {
				
				string line;
				
				while((line = sr.ReadLine()) != null) {
					string[] pair = line.Split('=');
					
					if (pair.Length != 2)
						continue;
					
					string key = pair[0].Trim();
					string value = pair[1].Trim();

					if (!settings.ContainsKey(key))
						settings.Add(key, value);
				}
			}
			
			// assign settings to the properties
			PropertyInfo[] propInfos = GetProperties();
			
			foreach (PropertyInfo pi in propInfos) {
				Attribute[] atts = (Attribute[])pi.GetCustomAttributes(typeof(SettingsAttribute), false);
				
				// check if the property has the SettingsAttribute
				if (atts.Length == 0)
					continue;
				
				SettingsAttribute sa = (SettingsAttribute)atts[0];
				string val;
				
				if (settings.TryGetValue(sa.Name, out val))
					SetPropertyValueFromString(pi, val);
			}
		}
		
		private void SetPropertyValueFromString(PropertyInfo pi, string val) {
			if (pi.PropertyType == typeof(string)) {
				pi.SetValue(this, val, null);
			} else if (pi.PropertyType == typeof(int)) {
				pi.SetValue(this, int.Parse(val), null);
			} else if (pi.PropertyType == typeof(long)) {
				pi.SetValue(this, long.Parse(val), null);
			} else if (pi.PropertyType == typeof(bool)) {
				pi.SetValue(this, (val == "1" ? true : false), null);
			}
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
		
		private static PropertyInfo[] propertyInfos = null;
		private static PropertyInfo[] GetProperties() {
			if (propertyInfos != null)
				return propertyInfos;
			
			Type t = typeof(Settings);
			propertyInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			return propertyInfos;
		}
		
		[AttributeUsage(AttributeTargets.Property)]
		class SettingsAttribute : Attribute
		{
			public SettingsAttribute(string name, string defaultValue) : base() {
				this.Name = name;
				this.DefaultValue = defaultValue;
			}
			
			public string Name { get; set; }
			public string DefaultValue { get; set; }
		}
	}
}
