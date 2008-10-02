// CurrentPlatform.cs
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

namespace Platform.Common.Diagnostics
{
	public static class CurrentPlatform
	{
		private static readonly bool isWin32;
		private static readonly bool isUnix;
		private static readonly bool isGnome;
		
		static CurrentPlatform() {
			isUnix	= Environment.OSVersion.Platform == PlatformID.Unix;
			// isWin32 = (platform == PlatformID.Win32NT) || (platform == PlatformID.Win32S) || (platform == PlatformID.Win32Windows);
			isWin32 = !isUnix;
			isGnome = Environment.OSVersion.Platform == PlatformID.Unix;
		}
		
		public static bool IsWin32	{ get { return isWin32; } }
		public static bool IsUnix	{ get { return isUnix;	} }
		public static bool IsGnome	{ get { return isGnome; } }
	}
}
