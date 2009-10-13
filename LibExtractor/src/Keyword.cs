// Keyword.cs
// 
// Copyright (C) 2008 Patrick Ulbrich, zulu99@gmx.net
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
using System.Runtime.InteropServices;

namespace LibExtractor
{
	[StructLayout(LayoutKind.Sequential)]
	public class Keyword {
		/* the keyword that was found */
		[MarshalAs(UnmanagedType.LPStr)]
		public string keyword;
		/* the type of the keyword (classification) */
		public KeywordType keywordType;
		/* the next entry in the list */
		internal IntPtr next;

		public override string ToString() {
			return string.Format("{0} - {1}", Extractor.GetKeywordTypeAsString(keywordType), keyword);
		}

	}
}
