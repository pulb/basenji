// IDSearchCriteria.cs
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
	public sealed class IDSearchCriteria : ISearchCriteria
	{
		private long			id;
		private IDSearchField	field;
		private CompareOperator compareOperator;
		
		public IDSearchCriteria(long id, IDSearchField field, CompareOperator compareOperator) {
			if (id < 0)
				throw new ArgumentException("Invalid id");
				
			this.id					= id;
			this.field				= field;
			this.compareOperator	= compareOperator;
		}
		
		public long Id {
			get { return id; }
		}

		public IDSearchField Field {
			get { return field; }
		}

		public CompareOperator CompareOperator {
			get { return compareOperator; }
		}
		
		#region ISearchCriteria Members

		string ISearchCriteria.GetSqlSearchCondition() {
			return field.GetSqlSearchCondition(id, compareOperator);
		}
		
		SearchCriteriaType ISearchCriteria.SearchCriteriaType {
			get { return Searching.SearchCriteriaType.ItemSearchCriteria; }
		}
		
		#endregion
	}
}
