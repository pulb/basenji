// AudioCdRootVolumeItem.cs
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
	public sealed class AudioCdRootVolumeItem : VolumeItem, IContainerItem
	{
		internal AudioCdRootVolumeItem(VolumeDatabase database)
			: base(database, VolumeItemType.AudioCdRootVolumeItem)
		{
		}
		
		#region IContainerItem Members
		IChildItem[] IContainerItem.GetItems() {
			return Database.GetChildItems<IChildItem>(VolumeID, ItemID);
		}
		
		IContainerItem[] IContainerItem.GetContainers() {
			return new IContainerItem[0];
		}
		#endregion
		
		// AudioCdRootVolumeItem specific implementation of IContainerItem.GetItems()
		public AudioTrackVolumeItem[] GetTracks() {
			return Database.GetChildItems<AudioTrackVolumeItem>(VolumeID, ItemID);
		}
		
		internal override void WriteToVolumeDBRecord(IRecordData recordData) {
			base.WriteToVolumeDBRecord(recordData);
			recordData.AddField("IsContainer", true);
		}
	}
}
