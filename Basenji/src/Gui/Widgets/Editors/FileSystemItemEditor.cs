// FileSystemItemEditor.cs
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
using System.Collections.Generic;
using Gtk;
using Basenji.Gui.Base;
using VolumeDB;

namespace Basenji.Gui.Widgets.Editors
{
	public abstract class FileSystemItemEditor : ItemEditor
	{
		private Label lblLocation;
		private Label lblLastWriteTime;
		private Label lblMimeType;
		
		protected FileSystemItemEditor(string itemType) : base(itemType) {}
		
		protected override void LoadFromObject(VolumeDB.VolumeItem item) {
			if (!(item is FileSystemVolumeItem))
				throw new ArgumentException(string.Format("must be of type {0}",
				                                          typeof(FileSystemVolumeItem)), "item");

			base.LoadFromObject(item);
			
			FileSystemVolumeItem fsvi = (FileSystemVolumeItem)item;
			
			UpdateLabel(lblLocation, fsvi.Location);
			UpdateLabel(lblLastWriteTime, fsvi.LastWriteTime.ToString());
			UpdateLabel(lblMimeType, string.IsNullOrEmpty(fsvi.MimeType) ? "-" : fsvi.MimeType);
		}
		
		protected override void AddInfoLabels(List<InfoLabel> infoLabels) {
			base.AddInfoLabels(infoLabels);
			
			lblLocation				= WindowBase.CreateLabel();
			lblLocation.Ellipsize	= Pango.EllipsizeMode.End;
			
			lblLastWriteTime		= WindowBase.CreateLabel();
			lblMimeType				= WindowBase.CreateLabel();
			lblMimeType.Ellipsize	= Pango.EllipsizeMode.End;
			
			infoLabels.AddRange( new InfoLabel[] { 
				new InfoLabel(S._("Location") + ":",		lblLocation),
				new InfoLabel(S._("Last write time") + ":",	lblLastWriteTime),
				new InfoLabel(S._("Filetype") + ":",		lblMimeType)
			} );
		}
	}
}
