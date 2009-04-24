// IDSearchField.cs
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

namespace VolumeDB.Searching.ItemSearchCriteria
{
	/* 
	 * Fields that the IDSearchCriteria can search.
	 */
	public struct IDSearchField
	{
		private int value;
		
		private IDSearchField(int value) {
			this.value = value;
		}
		
		public static IDSearchField ItemID		{ get { return new IDSearchField(1);  }}
		public static IDSearchField VolumeID	{ get { return new IDSearchField(2);  }}
		public static IDSearchField ParentID	{ get { return new IDSearchField(3);  }}
		
		public static bool operator ==(IDSearchField a, IDSearchField b) {
			return a.value == b.value;
		}
		
		public static bool operator !=(IDSearchField a, IDSearchField b) {
			return a.value != b.value;
		}
		
		public override bool Equals (object o) {
			 if (!(o is IDSearchField))
				return false;
				
			 return this == (IDSearchField)o;
		}
		
		public override int GetHashCode() {
			return value;
		}
		
		/* get the sql search condition of this field */
		internal string GetSqlSearchCondition(long id, CompareOperator compareOperator) {
			string fieldName = string.Empty;
			
			if (this == IDSearchField.ItemID)
				fieldName = "ItemID";
			else if (this == IDSearchField.ParentID)
				fieldName = "ParentID";
			else if (this == IDSearchField.VolumeID)
				fieldName = "VolumeID";
			else
				throw new NotImplementedException(string.Format("IDSearchField {0} not implemented", value));
				
			return compareOperator.GetSqlCompareString(fieldName, id.ToString());
		}
		
	}
}
