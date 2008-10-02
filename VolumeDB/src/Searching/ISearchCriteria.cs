// ISearchCriteria.cs
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
	//internal interface ISqlSearchConditionProvider
	//{
	//	  SqlLogicalOperator	  LinkOperator		  { get; }
	//	  SqlSearchCondition[]	  SearchConditions	  { get; }
	//}

	// implement this one explicitely, to hide GetSqlSearchCondition()
	public interface ISearchCriteria
	{
		/*
		 * conventions: 
		 * - returned string must not be null or 0-length
		 * - returned string is only a part of the WHERE clause
		 * - returned string must prefix columns with the tablename (e.g. "(table.a = 1 AND table.b = 2)" )
		 */
		string GetSqlSearchCondition();
	}
}
