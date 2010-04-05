// AudioTrackVolumeItem.cs
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

namespace VolumeDB
{
	public sealed class AudioTrackVolumeItem : VolumeItem
	{
		private int duration;
		
		internal AudioTrackVolumeItem(VolumeDatabase database)
			: base(database, VolumeItemType.AudioTrackVolumeItem)
		{
			duration = 0;
		}
		
		/// <summary>
		/// <para>Required by internal factory methods like AbstractVolumeScanner.GetNewVolumeItem<TVolumeItem>()</para>
		/// <para>Purpose :</para>
		/// <para>
		/// - guarantee that _all_ fields of this type are initialized by the caller 
		///  (in contrast to property initialization, which easily makes you miss a property [in particular if a new one was added..])
		/// </para>
		/// <para>
		/// - seperate fields of a type from fields of its base type (e.g. GetNewVolumeItem<TVolumeItem>() initializes all fields of a the VolumeItem base type. 
		/// Caller code only needs to initialize fields of the derived <TVolumeItem> type)
		/// </para>
		/// </summary>
		internal void SetAudioTrackVolumeItemFields(int duration) {
			this.duration = duration;
		}
		
		internal override void ReadFromVolumeDBRecord(IRecordData recordData) {
			base.ReadFromVolumeDBRecord(recordData);
			
			duration = (int)(long)recordData["Size"];
		}
		
		internal override void WriteToVolumeDBRecord(IRecordData recordData) {
			base.WriteToVolumeDBRecord(recordData);
			
			recordData.AddField("Size", duration);
		}
		
		#region read-only properties
		public int Duration {
			get {
				return duration;
			}
			internal set {
				duration = value;
			}
		}
		
		// expose artist metadata trough a dedicated property
		private string artist = null;
		public string Artist {
			get {
				if (artist == null) {
					Dictionary<string, string> metaData = ParseMetaData();
					// artist metadate is only available if the cd has been
					// scanned with metadata extraction enabled
					if (!metaData.TryGetValue("artist", out artist))
						artist = string.Empty;
				}
				return artist;
			}
		}
		#endregion
	}
}
