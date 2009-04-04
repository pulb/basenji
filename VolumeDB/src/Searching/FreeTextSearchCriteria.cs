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
//using System.Text;

namespace VolumeDB.Searching
{
//	  [Flags()]
//	public enum FreeTextSearchField
//	{
//		FileName		= 1,
//		  DirectoryName   = 2,
//		  Location		  = 4,
//		Note			= 8,
//		Keywords		= 16, // keywords of items
//		//TrackInfo = 32
//	}
	
	/*
		FreeTextSearchCriteria
		Simple freetextsearch implementation.
		Searches a string in different fields.
	*/
	public sealed class FreeTextSearchCriteria : ISearchCriteria
	{
		private string					searchString;
		private FreeTextSearchField		fields;
		private TextCompareOperator		compareOperator;
		private MatchRule				fieldMatchRule;
		
		public FreeTextSearchCriteria(string searchString, FreeTextSearchField fields, TextCompareOperator compareOperator) : this(searchString, fields, compareOperator, MatchRule.AnyMustMatch) { }
		public FreeTextSearchCriteria(string searchString, FreeTextSearchField fields, TextCompareOperator compareOperator, MatchRule fieldMatchRule) {
			
			if (searchString == null)
				throw new ArgumentNullException("searchString");

			if (searchString.Length < VolumeDatabase.MIN_SEARCHSTR_LENGTH)
				throw new ArgumentException(string.Format("Length of a searchstring must be at least {0}",
											VolumeDatabase.MIN_SEARCHSTR_LENGTH), "searchString");
				
			if (fields == FreeTextSearchField.None)
				throw new ArgumentException("No searchfield specified", "fields");

			this.searchString	   = searchString.Replace("'","''");
			this.fields			   = fields;
			this.compareOperator   = compareOperator;
			this.fieldMatchRule    = fieldMatchRule;
		}
		
		public string SearchString {
			get { return searchString; }
		}

		public FreeTextSearchField Fields {
			get { return fields; }
		}

		public TextCompareOperator CompareOperator {
			get { return compareOperator; }
		}

		public MatchRule FieldMatchRule {
			get { return fieldMatchRule; }
		}
		
//		  private static string GetCompareStr(TextCompareOperator compareOperator) {
//			  string strCompare;
//			  switch (compareOperator) {
//				  case TextCompareOperator.BeginsWith:
//					  strCompare = "{0} LIKE '{1}%'";
//					  break;
//				  case TextCompareOperator.Contains:
//					  strCompare = "{0} LIKE '%{1}%'";
//					  break;
//				  case TextCompareOperator.EndsWith:
//					  strCompare = "{0} LIKE '%{1}'";
//					  break;
//				  case TextCompareOperator.IsEqual:
//					  //strCompare = "{0} = '{1}'";
//					  strCompare = "{0} LIKE '{1}'"; // case insensitive
//					  break;
//				  case TextCompareOperator.IsNotEqual:
//					  //strCompare = "{0} <> '{1}'";
//					  strCompare = "{0} NOT LIKE '{1}'"; // case insensitive
//					  break;
//				  default:
//					  throw new ArgumentException("Invalid TextCompareOperator.");
//			  }
//			  return strCompare;
//		  }

//		  private static bool ContainsField(FreeTextSearchField fieldMask, FreeTextSearchField field) {
//			  return (fieldMask & field) != FreeTextSearchField.None;
//		  }
		
//		  private void Append(StringBuilder sql, string condition) {
//			  if (sql.Length > 0)
//				  sql.AppendFormat(" {0} ", fieldMatchRule.GetSqlLogicalOperator());
//
//			  sql.Append('(').Append(condition).Append(')');
//		  }
		
		#region ISearchCriteria Members

		string ISearchCriteria.GetSqlSearchCondition() {

			return fields.GetSqlSearchCondition(searchString, compareOperator, fieldMatchRule);
			
//			  StringBuilder sql = new StringBuilder();

//			  string strCompare = GetCompareStr(compareOperator);
//
//			  if (ContainsField(fields, FreeTextSearchField.DirectoryName))
//				  Append(sql, string.Format(strCompare, "Items.Name", searchString)
//					  + string.Format(" AND (Items.ItemType = {0})", (int)VolumeItemType.DirectoryVolumeItem));
//
//			  if (ContainsField(fields, FreeTextSearchField.FileName))
//				  Append(sql, string.Format(strCompare, "Items.Name", searchString)
//					  + string.Format(" AND (Items.ItemType = {0})", (int)VolumeItemType.FileVolumeItem));
//			  
//			  if (ContainsField(fields, FreeTextSearchField.Keywords))
//				  Append(sql, string.Format(strCompare, "Items.Keywords", searchString));
//
//			  if (ContainsField(fields, FreeTextSearchField.Location))
//				  Append(sql, string.Format(strCompare, "Items.Location", searchString)
//					  + string.Format(" AND ((Items.ItemType = {0}) OR (Items.ItemType = {1}))", (int)VolumeItemType.FileVolumeItem, (int)VolumeItemType.DirectoryVolumeItem));
//
//			  if (ContainsField(fields, FreeTextSearchField.Note))
//				  Append(sql, string.Format(strCompare, "Items.Note", searchString));
			
			
			
			
//			  FreeTextSearchField f;
//			  
//			  f = FreeTextSearchField.DirectoryName;
//			  if (ContainsField(fields, f)) {
//				  Append(
//					  sql, compareOperator.GetSqlCompareString(f.GetSqlFieldName(), searchString) 
//					  + string.Format(" AND (Items.ItemType = {0})", (int)VolumeItemType.DirectoryVolumeItem)
//				  );
//			  }
//			  
//			  f = FreeTextSearchField.FileName;
//			  if (ContainsField(fields, f)) {
//				  Append(
//					  sql, compareOperator.GetSqlCompareString(f.GetSqlFieldName(), searchString) 
//					  + string.Format(" AND (Items.ItemType = {0})", (int)VolumeItemType.FileVolumeItem)
//				  );
//			  }
//			  
//			  f = FreeTextSearchField.Keywords;
//			  if (ContainsField(fields, f)) {
//				  Append(sql, compareOperator.GetSqlCompareString(f.GetSqlFieldName(), searchString));
//			  }
//			  
//			  f = FreeTextSearchField.Location;
//			  if (ContainsField(fields, f)) {
//				  Append(
//					  sql, compareOperator.GetSqlCompareString(f.GetSqlFieldName(), searchString) 
//					  + string.Format(" AND ((Items.ItemType = {0}) OR (Items.ItemType = {1}))", (int)VolumeItemType.FileVolumeItem, (int)VolumeItemType.DirectoryVolumeItem)
//				  );
//			  }
//			  
//			  f = FreeTextSearchField.Note;
//			  if (ContainsField(fields, f)) {
//				  Append(sql, compareOperator.GetSqlCompareString(f.GetSqlFieldName(), searchString));
//			  }
			
//			  return sql.ToString();
		}

		#endregion
	}
}
