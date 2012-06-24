// FileSystemVolumeEditor.cs
// 
// Copyright (C) 2008 - 2012 Patrick Ulbrich
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
using VolumeDB;
using VolumeDB.VolumeScanner;

namespace Basenji.Gui.Widgets.Editors
{	
	public class FileSystemVolumeEditor : VolumeEditor
	{
		private Label lblFiles;
		private Label lblDirectories;
		private Label lblTotalSize;
		
		public FileSystemVolumeEditor() : base(S._("Filesystem")) {}
		
		public override void UpdateInfo(VolumeInfo vi) { 
			if (!(vi is FilesystemVolumeInfo))
				throw new ArgumentException(string.Format("must be of type {0}",
				                                          typeof(FilesystemVolumeInfo)), "vi");
			
			base.UpdateInfo(vi);
			FilesystemVolumeInfo fsvi = (FilesystemVolumeInfo)vi;
			UpdateInfoLabels(fsvi.Files, fsvi.Directories, fsvi.Size);
		}
		
		protected override void LoadFromObject(VolumeDB.Volume volume) {
			if (!(volume is FileSystemVolume))
				throw new ArgumentException(string.Format("must be of type {0}",
				                                          typeof(FileSystemVolume)), "volume");

			base.LoadFromObject(volume);
			
			FileSystemVolume fsvol = (FileSystemVolume)volume;
			UpdateInfoLabels(fsvol.Files, fsvol.Directories, fsvol.Size);
		}
		
		protected override void AddInfoLabels(List<InfoLabel> infoLabels) {
			base.AddInfoLabels(infoLabels);
			
			lblFiles		= WindowBase.CreateLabel();
			lblDirectories	= WindowBase.CreateLabel();
			lblTotalSize	= WindowBase.CreateLabel();
			
			infoLabels.AddRange( new InfoLabel[] { 
				new InfoLabel(S._("Files:"), lblFiles),
				new InfoLabel(S._("Directories:"), lblDirectories),
				new InfoLabel(S._("Total size:"), lblTotalSize)
			} );
		}
			
		private void UpdateInfoLabels(long files, long directories, long totalSize) {
			UpdateLabel(lblFiles, files.ToString());
			UpdateLabel(lblDirectories, directories.ToString());
			UpdateLabel(lblTotalSize, Util.GetSizeStr(totalSize));
		}
	}
}
