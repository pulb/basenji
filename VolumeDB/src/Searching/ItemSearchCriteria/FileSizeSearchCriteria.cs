// FileSizeSearchCriteria.cs
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
	public sealed class FileSizeSearchCriteria : ISearchCriteria
	{
		private long			fileSize;
		private CompareOperator compareOperator;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileSize">filesize in bytes</param>
		/// <param name="compareOperator">operator that specifies how to compare filesizes</param>
		public FileSizeSearchCriteria(long fileSize, CompareOperator compareOperator) {
			if (fileSize < 0)
				throw new ArgumentOutOfRangeException("fileSize");

			this.fileSize		   = fileSize;
			this.compareOperator   = compareOperator;
		}
		
		public long FileSize {
			get { return fileSize; }
		}

		public CompareOperator CompareOperator {
			get { return compareOperator; }
		}
		
		#region ISearchCriteria Members

		string ISearchCriteria.GetSqlSearchCondition() {
//			  string strOp;
//			  switch (compareOperator) {
//				  case CompareOperator.Equal:
//					  strOp = "=";
//					  break;
//				  case CompareOperator.Greater:
//					  strOp = ">";
//					  break;
//				  case CompareOperator.GreaterOrEqual:
//					  strOp = ">=";
//					  break;
//				  case CompareOperator.Less:
//					  strOp = "<";
//					  break;
//				  case CompareOperator.LessOrEqual:
//					  strOp = "<=";
//					  break;
//				  case CompareOperator.NotEqual:
//					  strOp = "<>";
//					  break;
//				  default:
//					  throw new Exception("Invalid CompareOperator.");
//			  }
//			  return string.Format("(Items.Size {0} {1}) AND (ItemType = {2})", strOp, fileSize, (int)VolumeItemType.FileVolumeItem);
			  return string.Format("({0}) AND (Items.ItemType = {1})",
			  						compareOperator.GetSqlCompareString("Items.Size", fileSize.ToString()),
			  						(int)VolumeItemType.FileVolumeItem);
		}

		SearchCriteriaType ISearchCriteria.SearchCriteriaType {
			get { return Searching.SearchCriteriaType.ItemSearchCriteria; }
		}
		
		#endregion
		
	}
}
