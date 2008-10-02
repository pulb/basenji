// CompareOperator.cs
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
	public struct CompareOperator
	{
		private int value;
		
		private CompareOperator(int value) {
			this.value = value;
		}
		
		public static CompareOperator Less				{ get { return new CompareOperator(0); }}		
		public static CompareOperator LessOrEqual		{ get { return new CompareOperator(1); }}		
		public static CompareOperator Equal				{ get { return new CompareOperator(2); }}		
		public static CompareOperator NotEqual			{ get { return new CompareOperator(3); }}		
		public static CompareOperator Greater			{ get { return new CompareOperator(4); }}	 
		public static CompareOperator GreaterOrEqual	{ get { return new CompareOperator(5); }}
		
		public static bool operator ==(CompareOperator a, CompareOperator b) {
			return a.value == b.value;
		}
		
		public static bool operator !=(CompareOperator a, CompareOperator b) {
			return a.value != b.value;
		}
		
		public override bool Equals (object o) {
			 if (!(o is CompareOperator))
				return false;
				
			 return this == (CompareOperator)o;
		}
		
		public override int GetHashCode() {
			return value;
		}
		
		internal string GetSqlCompareString(string fieldName, string searchString) {
			string strOp = null;
			if (this == CompareOperator.Equal)
					strOp = "=";
			else if (this == CompareOperator.Greater)
					strOp = ">";
			else if (this == CompareOperator.GreaterOrEqual)
					strOp = ">=";
			else if (this == CompareOperator.Less)
					strOp = "<";
			else if (this == CompareOperator.LessOrEqual)
					strOp = "<=";
			else if(this == CompareOperator.NotEqual)
					strOp = "<>";

			return string.Format("{0} {1} {2}", fieldName, strOp, searchString);
		}

	}
}
