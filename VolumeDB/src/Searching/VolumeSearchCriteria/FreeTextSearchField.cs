// FreeTextSearchCriteria.cs
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
	/* 
	 * Fields that the FreetextSearchCriteria can search.
	 * Fields can be combined via the binary or operator ("|").
	 */
	public struct FreeTextSearchField : IFreeTextSearchField
	{
		private static Dictionary<string, FreeTextSearchField> stringMapping = new Dictionary<string, FreeTextSearchField>() {
			{ "TITLE", 			FreeTextSearchField.Title		},
			{ "LOANEDTO",		FreeTextSearchField.LoanedTo	},
			{ "DESCRIPTION",	FreeTextSearchField.Description	},
			{ "KEYWORDS",		FreeTextSearchField.Keywords	}
		};
		
		private uint value;
		
		private FreeTextSearchField(uint value) {
			this.value = value;
		}
		
		// note : ArchiveNo can't be used in a freetextsearchcriteria 
		// since it is often < min searchstring length
		public static FreeTextSearchField None			{ get { return new FreeTextSearchField(0);	}} // required for binary &
		public static FreeTextSearchField Title			{ get { return new FreeTextSearchField(1);	}}
		public static FreeTextSearchField LoanedTo 		{ get { return new FreeTextSearchField(2);	}}		
		public static FreeTextSearchField Description	{ get { return new FreeTextSearchField(4);	}}
		public static FreeTextSearchField Keywords		{ get { return new FreeTextSearchField(8);	}} // keywords of volumes

		public static FreeTextSearchField FromString(string fieldName) {
			FreeTextSearchField sf = FreeTextSearchField.None;
			
			if (fieldName == null)
				throw new ArgumentNullException("fieldName");
				
			if (!stringMapping.TryGetValue(fieldName.ToUpper(), out sf))
				throw new ArgumentException("Unknown fieldname", "fieldName");
			
			return sf;
		}
		
		public static bool operator ==(FreeTextSearchField a, FreeTextSearchField b) {
			return a.value == b.value;
		}
		
		public static bool operator !=(FreeTextSearchField a, FreeTextSearchField b) {
			return a.value != b.value;
		}
		
		public static FreeTextSearchField operator |(FreeTextSearchField a, FreeTextSearchField b) {
			return new FreeTextSearchField(a.value | b.value);
		}
		
		public static FreeTextSearchField operator &(FreeTextSearchField a, FreeTextSearchField b) {
			return new FreeTextSearchField(a.value & b.value);
		}
		
		public override bool Equals (object o) {
			 if (!(o is FreeTextSearchField))
				return false;
				
			 return this == (FreeTextSearchField)o;
		}
		
		public override int GetHashCode() {
			return (int)value;
		}
		
		/* 
		* indicates whether a FreeTextSeachField value is a
		* bitwise combination of multiple FreeTextSeachField values. 
		*/
		public bool IsCombined {
			get {
				return SearchUtils.IsCombined(value);
			}
		}
		
		private bool ContainsField(FreeTextSearchField field) {
			return (this & field) == field;
		}
		
		#region IFreeTextSearchField members
		/* get the sql search condition of this/these field/fields */
		string IFreeTextSearchField.GetSqlSearchCondition(string searchString, TextCompareOperator compareOperator, MatchRule fieldMatchRule) {
			
			StringBuilder sql = new StringBuilder();
			
			if (this.ContainsField(Title))
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Volumes.Title", searchString), fieldMatchRule);
						
			if (this.ContainsField(LoanedTo))
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Volumes.Loaned_To", searchString), fieldMatchRule);
			
			if (this.ContainsField(Description))
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Volumes.Description", searchString), fieldMatchRule);
				
			if (this.ContainsField(Keywords))
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Volumes.Keywords", searchString), fieldMatchRule);
			
			return sql.ToString();
		}
		
		SearchCriteriaType IFreeTextSearchField.ResultingSearchCriteriaType {
			get { return SearchCriteriaType.VolumeSearchCriteria; }
		}
		
		bool IFreeTextSearchField.IsEmpty {
			get {
				return (this == FreeTextSearchField.None);
			}
		}
		#endregion
		
	}
}