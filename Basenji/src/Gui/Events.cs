// Events.cs
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

using VolumeDB;

namespace Basenji.Gui
{	
	public delegate void NewVolumeAddedEventHandler(object o, NewVolumeAddedEventArgs args);
	
	public class NewVolumeAddedEventArgs : EventArgs
	{	
		private Volume volume;
		
		public NewVolumeAddedEventArgs(Volume volume) : base() {
			this.volume = volume;
		}
		
		public Volume Volume { get {return volume; } }
	}
}