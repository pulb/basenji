// FreeTextSearchCriteria.cs
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
	/*
		FreeTextSearchCriteria
		Simple freetextsearch implementation.
		Searches a string in different fields.
	*/
	public sealed class FreeTextSearchCriteria : ISearchCriteria
	{
		private string					searchString;
		private IFreeTextSearchField	fields;
		private TextCompareOperator		compareOperator;
		private MatchRule				fieldMatchRule;
		
		public FreeTextSearchCriteria(string searchString, IFreeTextSearchField fields, TextCompareOperator compareOperator)
		: this(searchString, fields, compareOperator, MatchRule.AnyMustMatch) {}
		
		public FreeTextSearchCriteria(string searchString, IFreeTextSearchField fields, TextCompareOperator compareOperator, MatchRule fieldMatchRule) {
			
			if (searchString == null)
				throw new ArgumentNullException("searchString");

			if (searchString.Length < VolumeDatabase.MIN_SEARCHSTR_LENGTH)
				throw new ArgumentException(string.Format("Length of a searchstring must be at least {0}",
											VolumeDatabase.MIN_SEARCHSTR_LENGTH), "searchString");
				
//			if (fields == FreeTextSearchField.None)
			if (fields == null || fields.IsEmpty)
				throw new ArgumentException("No searchfield specified", "fields");

			this.searchString	   = searchString.Replace("'","''");
			this.fields			   = fields;
			this.compareOperator   = compareOperator;
			this.fieldMatchRule    = fieldMatchRule;
		}
		
		public string SearchString {
			get { return searchString; }
		}

		public IFreeTextSearchField Fields {
			get { return fields; }
		}

		public TextCompareOperator CompareOperator {
			get { return compareOperator; }
		}

		public MatchRule FieldMatchRule {
			get { return fieldMatchRule; }
		}
		
		#region ISearchCriteria Members

		string ISearchCriteria.GetSqlSearchCondition() {
			return fields.GetSqlSearchCondition(searchString, compareOperator, fieldMatchRule);
		}

		SearchCriteriaType ISearchCriteria.SearchCriteriaType {
			get { return fields.ResultingSearchCriteriaType; }
		}
		
		#endregion
	}
}
