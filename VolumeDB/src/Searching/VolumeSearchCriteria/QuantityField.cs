// QuantityField.cs
// 
// Copyright (C) 2009 Patrick Ulbrich
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
using System.Collections.Generic;
using System.Text;

namespace VolumeDB.Searching.VolumeSearchCriteria
{	
	public struct QuantityField
	{
		private static Dictionary<string, QuantityField> stringMapping = new Dictionary<string, QuantityField>() {
			{ "FILES",		QuantityField.Files	},
			{ "DIRS",		QuantityField.Dirs	},
			{ "SIZE",		QuantityField.Size	}
		};
		
		private uint value;
		
		private QuantityField(uint value) {
			this.value = value;
		}
		
		public static QuantityField None	{ get { return new QuantityField(0);	}} // required for binary &
		public static QuantityField Files	{ get { return new QuantityField(1);	}}
		public static QuantityField Dirs	{ get { return new QuantityField(2);	}}
		public static QuantityField Size	{ get { return new QuantityField(4);	}}
		
		public static QuantityField FromString(string quantityField) {
			QuantityField field = QuantityField.None;
			
			if (quantityField == null)
				throw new ArgumentNullException("quantityField");
				
			if (!stringMapping.TryGetValue(quantityField.ToUpper(), out field))
				throw new ArgumentException("Unknown fieldname", "quantityField");
			
			return field;
		}
		
		public static bool operator ==(QuantityField a, QuantityField b) {
			return a.value == b.value;
		}
		
		public static bool operator !=(QuantityField a, QuantityField b) {
			return a.value != b.value;
		}
		
		public static QuantityField operator |(QuantityField a, QuantityField b) {
			return new QuantityField(a.value | b.value);
		}
		
		public static QuantityField operator &(QuantityField a, QuantityField b) {
			return new QuantityField(a.value & b.value);
		}
		
		public override bool Equals (object o) {
			 if (!(o is QuantityField))
				return false;
				
			 return this == (QuantityField)o;
		}
		
		public override int GetHashCode() {
			return (int)value;
		}
		
		/* 
		* indicates whether a QuantityField value is a
		* bitwise combination of multiple QuantityField values. 
		*/
		public bool IsCombined {
			get {
				return SearchUtils.IsCombined(value);
			}
		}
		
		private bool ContainsField(QuantityField field) {
			return (this & field) == field;
		}
		
		/* get the sql search condition of this/these field/fields */
		internal string GetSqlSearchCondition(long quantity, CompareOperator compareOperator, MatchRule fieldMatchRule) {
			
			StringBuilder sql = new StringBuilder();
			string strQuantity = quantity.ToString();
			
			if (this.ContainsField(Files))
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Volumes.Files", strQuantity), fieldMatchRule);
			
			if (this.ContainsField(Dirs))
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Volumes.Dirs", strQuantity), fieldMatchRule);
			
			if (this.ContainsField(Size))
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Size.Size", strQuantity), fieldMatchRule);
			
			return sql.ToString();
		}
	}
}
