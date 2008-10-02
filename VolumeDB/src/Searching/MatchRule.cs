// MatchRule.cs
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
	public struct MatchRule
	{
		private int value;
		
		private MatchRule(int value) {
			this.value = value;
		}
		
		public static MatchRule AllMustMatch { get { return new MatchRule(0); }}
		public static MatchRule AnyMustMatch { get { return new MatchRule(1); }}
		
		public static bool operator ==(MatchRule a, MatchRule b) {
			return a.value == b.value;
		}
		
		public static bool operator !=(MatchRule a, MatchRule b) {
			return a.value != b.value;
		}
		
		public override bool Equals (object o) {
			 if (!(o is MatchRule))
				return false;
				
			 return this == (MatchRule)o;
		}
		
		public override int GetHashCode() {
			return value;
		}
		
		internal string GetSqlLogicalOperator() {
			return this == MatchRule.AllMustMatch ? "AND" : "OR";
		}
	}
}
