// Util.cs
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
	internal static class Util
	{
		/*
		/// <summary>
		/// Checks wheter an array holds null references only
		/// </summary>
		/// <param name="arr">the array to be checked</param>
		/// <returns>returns true if the array contains one ore more non-null elements, otherweise false</returns>
		public static bool IsNullArray(System.Array arr) {
			bool r = true;
			foreach(object o in arr) {
				if (o != null) {
					r = false;
					break;
				}
			}
			return r;
		}
		*/

		/// <summary>
		/// Checks wheter an array holds null references only
		/// </summary>
		/// <param name="arr">the array to be checked</param>
		/// <returns>returns true if the array contains one ore more non-null elements, otherweise false</returns>
		public static bool IsNullArray<T>(T[] arr) {
			return Array.TrueForAll<T>(arr,
				delegate(T val) { 
					return val == null; 
				}
				);
		}
	}
}
