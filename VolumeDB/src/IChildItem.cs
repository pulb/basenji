// IChildItem.cs
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
	/*	interface for abstract hierarchical parent/child item access
	 *
	 *	should be implemented explicitely since
	 *	 a) it is not meant to be called on a VolumeItem directly 
	 *		(rather via "entry" methods like Volume.GetRootItem())
	 *	 b) specific VolumeItem implementations implement less abstract members
	 *		(i.e. alias-methods with a more specific/meaningful name)
	 *		that map to members of this interface.
	 */
	public interface IChildItem
	{
		IContainerItem	GetParent();
		string			Name { get; }
		VolumeItemType	GetVolumeItemType();
	}
}
