// SearchCriteriaGroup.cs
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

// TODO: this class is new and fairly untested: completely test in all variations
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace VolumeDB.Searching
{
	public sealed class SearchCriteriaGroup : ISearchCriteria, IEnumerable<ISearchCriteria>
	{
		private MatchRule				membersMatchRule;
		private List<ISearchCriteria>	memberCriteria;
		
		public SearchCriteriaGroup(MatchRule membersMatchRule) {
			this.membersMatchRule	= membersMatchRule;
			this.memberCriteria		= new List<ISearchCriteria>();
		}
		
		public void AddSearchCriteria(ISearchCriteria searchCriteria) {
			if (searchCriteria == null)
				throw new ArgumentNullException("searchCriteria");
			
			memberCriteria.Add(searchCriteria);
		}
		
		public MatchRule MembersMatchRule {
			get { return membersMatchRule; }
		}
		
		public int MemberCount {
			get { return memberCriteria.Count; }
		}
		
		public ISearchCriteria this[int index] {
			get { return memberCriteria[index]; }
		}
		
		private void Append(StringBuilder sql, string condition) {
			if (sql.Length > 0)
				sql.AppendFormat(" {0} ", membersMatchRule.GetSqlLogicalOperator());

			sql.Append('(').Append(condition).Append(')');
		}
		
		#region IEnumerable<ISearchCriteria> Members
		
		IEnumerator<ISearchCriteria> IEnumerable<ISearchCriteria>.GetEnumerator() {
			foreach(ISearchCriteria sc in memberCriteria)
				yield return sc;
		}
		
		#endregion
		
		#region IEnumerable Members
		
		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable<ISearchCriteria>)this).GetEnumerator();
		}
		
		#endregion
		
		#region ISearchCriteria Members

		string ISearchCriteria.GetSqlSearchCondition() {
			StringBuilder sql = new StringBuilder();
			
			foreach(ISearchCriteria sc in memberCriteria) {
				string condition = sc.GetSqlSearchCondition();
				if (condition.Length > 0)
					Append(sql, condition);
			}
			return sql.ToString();
		}
		
		#endregion
	}
}
