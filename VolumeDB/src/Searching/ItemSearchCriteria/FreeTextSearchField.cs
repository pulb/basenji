// FreeTextSearchField.cs
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

// TODO : 
// allow text searching in fields MimeType, MetaData or
// write specific SearchCriteria classes (analogous FileSizeSearchCriteria)?
#define ALLOW_FREETEXTSEARCH_MIMETYPE
#define ALLOW_FREETEXTSEARCH_METADATA

using System;
using System.Collections.Generic;
using System.Text;

namespace VolumeDB.Searching.ItemSearchCriteria
{
	/* 
	 * Fields that the FreetextSearchCriteria can search.
	 * Fields can be combined via the binary or operator ("|").
	 */
	public struct FreeTextSearchField : IFreeTextSearchField
	{
		private static Dictionary<string, FreeTextSearchField> stringMapping = new Dictionary<string, FreeTextSearchField>() {
			{ "FILENAME", 		FreeTextSearchField.FileName		},
			{ "DIRECTORYNAME",	FreeTextSearchField.DirectoryName	},
			{ "LOCATION",		FreeTextSearchField.Location		},
			{ "NOTE",			FreeTextSearchField.Note			},
			{ "KEYWORDS",		FreeTextSearchField.Keywords		},
#if ALLOW_FREETEXTSEARCH_MIMETYPE			
			{ "MIMETYPE",		FreeTextSearchField.MimeType		},
#endif
#if ALLOW_FREETEXTSEARCH_METADATA
			{ "METADATA",		FreeTextSearchField.MetaData		}
#endif			
		};
		
		private uint value;
		
		private FreeTextSearchField(uint value) {
			this.value = value;
		}
		
		public static FreeTextSearchField None			{ get { return new FreeTextSearchField(0);	}} // required for binary &
		public static FreeTextSearchField FileName		{ get { return new FreeTextSearchField(1);	}}
		public static FreeTextSearchField DirectoryName { get { return new FreeTextSearchField(2);	}}		
		public static FreeTextSearchField Location		{ get { return new FreeTextSearchField(4);	}}
		public static FreeTextSearchField Note			{ get { return new FreeTextSearchField(8);	}}
		public static FreeTextSearchField Keywords		{ get { return new FreeTextSearchField(16); }} // keywords of items
#if ALLOW_FREETEXTSEARCH_MIMETYPE
		public static FreeTextSearchField MimeType		{ get { return new FreeTextSearchField(32); }}
#endif
#if ALLOW_FREETEXTSEARCH_METADATA
		public static FreeTextSearchField MetaData		{ get { return new FreeTextSearchField(64); }}
#endif
		public static FreeTextSearchField AnyName		{ get { return new FreeTextSearchField((FileName | DirectoryName).value); }}
		
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
			
			if (this.ContainsField(AnyName)) {
				// search name fields of _all_ possible volume itemtypes (e.g. DirectoryName, FileName, ...)
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Items.Name", searchString), fieldMatchRule);
			} else {
				if (this.ContainsField(DirectoryName)) {
					SearchUtils.Append(
						sql, compareOperator.GetSqlCompareString("Items.Name", searchString) 
						+ string.Format(" AND (Items.ItemType = {0})", (int)VolumeItemType.DirectoryVolumeItem),
						fieldMatchRule
					);
				}
				
				if (this.ContainsField(FileName)) {
					SearchUtils.Append(
						sql, compareOperator.GetSqlCompareString("Items.Name", searchString) 
						+ string.Format(" AND (Items.ItemType = {0})", (int)VolumeItemType.FileVolumeItem),
						fieldMatchRule
					);
				}
			}
			
			if (this.ContainsField(Keywords)) {
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Items.Keywords", searchString), fieldMatchRule);
			}
			
			if (this.ContainsField(Location)) {
				SearchUtils.Append(
					sql, compareOperator.GetSqlCompareString("Items.Location", searchString) 
					+ string.Format(" AND ((Items.ItemType = {0}) OR (Items.ItemType = {1}))", (int)VolumeItemType.FileVolumeItem, (int)VolumeItemType.DirectoryVolumeItem),
					fieldMatchRule
				);
			}
			
			if (this.ContainsField(Note)) {
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Items.Note", searchString), fieldMatchRule);
			}
			
#if ALLOW_FREETEXTSEARCH_MIMETYPE
			if (this.ContainsField(MimeType)) {
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Items.MimeType", searchString), fieldMatchRule);
			}
#endif
#if ALLOW_FREETEXTSEARCH_METADATA
			if (this.ContainsField(MetaData)) {
				SearchUtils.Append(sql, compareOperator.GetSqlCompareString("Items.MetaData", searchString), fieldMatchRule);
			}
#endif

			return sql.ToString();
		}
		
		SearchCriteriaType IFreeTextSearchField.ResultingSearchCriteriaType {
			get { return SearchCriteriaType.ItemSearchCriteria; }
		}
		
		bool IFreeTextSearchField.IsEmpty {
			get {
				return (this == FreeTextSearchField.None);
			}
		}
		#endregion
		
	}
}
