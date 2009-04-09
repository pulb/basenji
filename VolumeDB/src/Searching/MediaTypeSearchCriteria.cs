// MediaTypeSearchCriteria.cs
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

namespace VolumeDB.Searching
{
	public class MediaTypeSearchCriteria : ISearchCriteria
	{		
		private MediaType types;
		private MatchRule typeMatchRule;
		
		public MediaTypeSearchCriteria(MediaType types) : this(types, MatchRule.AnyMustMatch) { }
		public MediaTypeSearchCriteria(MediaType types, MatchRule typeMatchRule) {
			if (types == MediaType.None)
				throw new ArgumentException("No type specified", "types");

			this.types			= types;
			this.typeMatchRule 	= typeMatchRule;
		}
		
		public MediaType Types {
			get { return types; }
		}
		
		public MatchRule TypeMatchRule {
			get { return typeMatchRule; }
		}
		
		#region ISearchCriteria Members
		string ISearchCriteria.GetSqlSearchCondition() {
			return types.GetSqlSearchCondition(typeMatchRule);
		}
		#endregion
	}
}
