// AudioTrackItemEditor.cs
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
	public class AudioTrackItemEditor : ItemEditor
	{
		private Label lblDuration;
		private Label lblMimeType;
		
		public AudioTrackItemEditor() : base(S._("Audio CD track")) {}
		
		protected override void LoadFromObject(VolumeDB.VolumeItem item) {
			if (!(item is AudioTrackVolumeItem))
				throw new ArgumentException(string.Format("must be of type {0}",
				                                          typeof(AudioTrackVolumeItem)), "item");

			base.LoadFromObject(item);
			
			AudioTrackVolumeItem avi = (AudioTrackVolumeItem)item;
			
			UpdateLabel(lblDuration, avi.Duration.ToString());
			UpdateLabel(lblMimeType, avi.MimeType);
		}
		
		protected override void AddInfoLabels(List<InfoLabel> infoLabels) {
			base.AddInfoLabels(infoLabels);
			
			lblDuration			= WindowBase.CreateLabel();
			lblMimeType			= WindowBase.CreateLabel();
			
			infoLabels.AddRange( new InfoLabel[] { 
				new InfoLabel(S._("Duration") + ":",	lblDuration),
				new InfoLabel(S._("Type") + ":",		lblMimeType)
			} );
		}
	}
}
