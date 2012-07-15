// ExtensionMethods.cs
// 
// Copyright (C) 2012 Patrick Ulbrich
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
	public static class ExtensionMethods
	{
		public static VolumeDriveType ToVolumeDriveType (this Platform.Common.IO.DriveType driveType) {
			VolumeDriveType vdt;

			switch (driveType) {
				case Platform.Common.IO.DriveType.CDRom:
					vdt = VolumeDriveType.CDRom;
					break;
				case Platform.Common.IO.DriveType.Fixed:
					vdt = VolumeDriveType.Harddisk;
					break;
				case Platform.Common.IO.DriveType.Ram:
					vdt = VolumeDriveType.Ram;
					break;
				case Platform.Common.IO.DriveType.Network:
					vdt = VolumeDriveType.Network;
					break;
				case Platform.Common.IO.DriveType.Removable:
					vdt = VolumeDriveType.Removable;
					break;
				case Platform.Common.IO.DriveType.Unknown:
					vdt = VolumeDriveType.Unknown;
					break;
				default:
					throw new Exception("Invalid DriveType");
			}
			
			return vdt;
		}
	}
}

