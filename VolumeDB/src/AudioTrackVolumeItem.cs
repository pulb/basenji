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
		#endregion
	}
}
