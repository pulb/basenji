// TextCompareOperator.cs
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

namespace VolumeDB.Searching
{
	public struct TextCompareOperator
	{
		private int value;
		
		private TextCompareOperator(int value) {
			this.value = value;
		}
		
		public static TextCompareOperator BeginsWith	{ get { return new TextCompareOperator(0); }}
		public static TextCompareOperator Contains		{ get { return new TextCompareOperator(1); }}
		public static TextCompareOperator EndsWith		{ get { return new TextCompareOperator(2); }}
		public static TextCompareOperator IsEqual		{ get { return new TextCompareOperator(3); }}
		public static TextCompareOperator IsNotEqual	{ get { return new TextCompareOperator(4); }}
		
		public static bool operator ==(TextCompareOperator a, TextCompareOperator b) {
			return a.value == b.value;
		}
		
		public static bool operator !=(TextCompareOperator a, TextCompareOperator b) {
			return a.value != b.value;
		}
		
		public override bool Equals (object o) {
			 if (!(o is TextCompareOperator))
				return false;
				
			 return this == (TextCompareOperator)o;
		}
		
		public override int GetHashCode() {
			return value;
		}
		
		internal string GetSqlCompareString(string fieldName, string searchString) {
			string strCompare = null;
			if (this == TextCompareOperator.BeginsWith)
					strCompare = "{0} LIKE '{1}%'";
			else if (this == TextCompareOperator.Contains)
					strCompare = "{0} LIKE '%{1}%'";
			else if (this == TextCompareOperator.EndsWith)
					strCompare = "{0} LIKE '%{1}'";
			else if (this == TextCompareOperator.IsEqual)
					strCompare = "{0} LIKE '{1}'"; // case insensitive	//strCompare = "{0} = '{1}'";
			else if (this == TextCompareOperator.IsNotEqual)
					strCompare = "{0} NOT LIKE '{1}'"; // case insensitive	 //strCompare = "{0} <> '{1}'";
			
			return string.Format(strCompare, fieldName, searchString);
		}
		
	}
}
