// AudioCdVolumeEditor.cs
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
using System.Collections.Generic;
using Gtk;
using Basenji.Gui.Base;
using VolumeDB;
using VolumeDB.VolumeScanner;

namespace Basenji.Gui.Widgets.Editors
{
	public class AudioCdVolumeEditor : VolumeEditor
	{
		private Label lblTracks;
		private Label lblDuration;
		
		public AudioCdVolumeEditor() : base(S._("Audio CD")) {}
		
		public override void UpdateInfo(VolumeInfo vi) {
			if (!(vi is AudioCdVolumeInfo))
				throw new ArgumentException(string.Format("must be of type {0}",
				                                          typeof(AudioCdVolumeInfo)), "vi");
			
			base.UpdateInfo(vi);
			AudioCdVolumeInfo avi = (AudioCdVolumeInfo)vi;
			UpdateInfoLabels(avi.Tracks, avi.Duration);
		}
		
		protected override void LoadFromObject(VolumeDB.Volume volume) {
			if (!(volume is AudioCdVolume))
				throw new ArgumentException(string.Format("must be of type {0}",
				                                          typeof(AudioCdVolume)), "volume");

			base.LoadFromObject(volume);
			
			AudioCdVolume avol = (AudioCdVolume)volume;
			UpdateInfoLabels(avol.Tracks, avol.Duration);
		}
		
		protected override void AddInfoLabels(List<InfoLabel> infoLabels) {
			base.AddInfoLabels(infoLabels);
			
			lblTracks	= WindowBase.CreateLabel();
			lblDuration	= WindowBase.CreateLabel();
			
			infoLabels.AddRange( new InfoLabel[] { 
				new InfoLabel(S._("Tracks:"), lblTracks),
				new InfoLabel(S._("Duration:"), lblDuration),
			} );
		}
			
		private void UpdateInfoLabels(int tracks, TimeSpan duration) {
			lblTracks.LabelProp		= tracks.ToString();
			lblDuration.LabelProp	= duration.ToString();
		}
	}
}
