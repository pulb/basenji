// QuantitySearchCriteria.cs
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

namespace VolumeDB.Searching.VolumeSearchCriteria
{
	public sealed class QuantitySearchCriteria : ISearchCriteria
	{	
		private QuantityField	quantityFields;
		private long			quantity;
		private CompareOperator	compareOperator;
		private MatchRule		fieldMatchRule;
		
		public QuantitySearchCriteria(QuantityField fields, long quantity, CompareOperator compareOperator)
		: this(fields, quantity, compareOperator, MatchRule.AnyMustMatch) {}
		
		public QuantitySearchCriteria(QuantityField fields, long quantity, CompareOperator compareOperator, MatchRule fieldMatchRule) {
			if (quantity < 0)
				throw new ArgumentOutOfRangeException("quantity");
			
			this.quantityFields		= fields;
			this.quantity			= quantity;
			this.compareOperator	= compareOperator;
			this.fieldMatchRule		= fieldMatchRule;
		}
		
		public QuantityField QuantityFields {
			get { return quantityFields; }
		}
		
		public long Quantity {
			get { return quantity; }
		}

		public CompareOperator CompareOperator {
			get { return compareOperator; }
		}
		
		#region ISearchCriteria Members

		string ISearchCriteria.GetSqlSearchCondition() {
			  return quantityFields.GetSqlSearchCondition(quantity, compareOperator, fieldMatchRule);
		}

		SearchCriteriaType ISearchCriteria.SearchCriteriaType {
			get { return Searching.SearchCriteriaType.VolumeSearchCriteria; }
		}
		
		#endregion
	}
}
