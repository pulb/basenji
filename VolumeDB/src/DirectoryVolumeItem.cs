// DirectoryVolumeItem.cs
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

using System;

namespace VolumeDB
{
	public sealed class DirectoryVolumeItem : FileSystemVolumeItem, IContainerItem
	{
		internal DirectoryVolumeItem(VolumeDatabase database)
			: base(database, VolumeItemType.DirectoryVolumeItem)
		{
		}
		
		#region IContainerItem Members
		IChildItem[] IContainerItem.GetItems() {
			// TODO:
			// return null or 0-length-array if no entry exists? (does GetChildItems() has to take this into account?)
			// how does DirectoryInfo.GetDirctories() behave in this regard?
			return Database.GetChildItems<IChildItem>(VolumeID, IsSymLink ? SymLinkTargetID : ItemID);
		}
		
		IContainerItem[] IContainerItem.GetContainers() {
			// TODO:
			// return null or 0-length-array if no entry exists? (does GetChildContainerItems() has to take this into account?)
			// how does DirectoryInfo.GetDirctories() behave in this regard?
			return Database.GetChildContainerItems<IContainerItem>(VolumeID, IsSymLink ? SymLinkTargetID : ItemID);
		}
		#endregion
		
		// DirectoryVolumeItem specific implementation of IChildItem.GetParent()
		public DirectoryVolumeItem GetParent() {
			return (DirectoryVolumeItem) ((IChildItem)this).GetParent();
		}
		
		// DirectoryVolumeItem specific implementation of IContainerItem.GetContainers()
		public DirectoryVolumeItem[] GetDirectories() {
			//return (DirectoryVolumeItem[]) ((IContainerItem)this).GetContainers();
			return Database.GetChildContainerItems<DirectoryVolumeItem>(VolumeID, IsSymLink ? SymLinkTargetID : ItemID);
		}
		
		// DirectoryVolumeItem specific implementation of IContainerItem.GetItems()
		public FileVolumeItem[] GetFiles() {
			//return (FileVolumeItem[]) ((IContainerItem)this).GetItems();
			return Database.GetChildItems<FileVolumeItem>(VolumeID, IsSymLink ? SymLinkTargetID : ItemID); 
		}
		
		internal override void WriteToVolumeDBRecord(IRecordData recordData) {
			base.WriteToVolumeDBRecord(recordData);
			recordData.AddField("IsContainer", true);
		}
	}
}
