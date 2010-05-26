// EUSLSearchCriteria.cs
// 
// Copyright (C) 2010 Patrick Ulbrich
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
using VolumeDB.Searching.EUSL.Parsing;

namespace VolumeDB.Searching.VolumeSearchCriteria
{
	public sealed class EUSLSearchCriteria : AbstractEUSLSearchCriteria
	{	
		
		private readonly Dictionary<string, QuantityField> quantityFields = new Dictionary<string, QuantityField>() {
			{ "SIZE", QuantityField.Size },
			{ "DIRS", QuantityField.Dirs },
			{ "FILES", QuantityField.Files }
		};
		
		public EUSLSearchCriteria(string euslQuery)	: base(euslQuery) {}
		
		// TODO :
		// word, number and phrase terms are currently only compared against database field 'Title'.
		// they should probably compared to other common database fields as well.
		// but this would slow down searching a lot.. 
		// maybe some smarter table indexers settings would help.
		internal override void OnTermParsed(TermParsedEventArgs e, out ISearchCriteria criteria) {
							
			criteria = null;
			
			switch (e.TermType) {
				case TermType.Number:
					criteria = new FreeTextSearchCriteria(e.Number.ToString(),
				                                      FreeTextSearchField.Title,
				                                      TextCompareOperator.Contains);
					break;
				case TermType.Phrase:
					criteria = new FreeTextSearchCriteria(e.Phrase,
				                                      FreeTextSearchField.Title,
				                                      TextCompareOperator.Contains);
					break;
				case TermType.Word:
					criteria = new FreeTextSearchCriteria(e.Word,
				                                      FreeTextSearchField.Title,
				                                      TextCompareOperator.Contains);
					break;
				case TermType.Select:
				
					string keyword = e.Keyword.ToUpper();
					
					// try to map the keyword to a volumes quantity field
					if (quantityFields.ContainsKey(keyword)) {
						
						long byteSize = e.Number;
						if (byteSize == -1L) {
							try {
								byteSize = GetByteSize(e.Word);
							} catch (ArgumentException) {
								throw new ArgumentException(
									string.Format(S._("Operand for keyword '{0}' must be a number with an optional multiplier"), e.Keyword),
									"euslQuery");
							}
						}
						
						CompareOperator cOp = CompareOperator.Equal;
						switch (e.Relation) {
							case Relation.Equal:
								cOp = CompareOperator.Equal;
								break;
							case Relation.Greater:
								cOp = CompareOperator.Greater;
								break;									
							case Relation.GreaterOrEqual:
								cOp = CompareOperator.GreaterOrEqual;
								break;
							case Relation.Less:
								cOp = CompareOperator.Less;
								break;
							case Relation.LessOrEqual:
								cOp = CompareOperator.LessOrEqual;
								break;
							default:
								throw new ArgumentException(
										string.Format(S._("Invalid compare operator for keyword '{0}'"), e.Keyword),
										"euslQuery");
						}
						
						criteria = new QuantitySearchCriteria(quantityFields[keyword], byteSize, cOp);
					
					} else {
						// try to map the keyword to freetextsearch fields
						
						IFreeTextSearchField sf;
						
						try {
							sf = FreeTextSearchField.FromString(e.Keyword);
						} catch (ArgumentException) {
							throw new ArgumentException(
										string.Format(S._("Unknown keyword '{0}'"), e.Keyword),
										"euslQuery");
						}
						
						TextCompareOperator tcOp = TextCompareOperator.Contains;							
						switch (e.Relation) {
							case Relation.Contains:
								tcOp = TextCompareOperator.Contains;
								break;
							case Relation.Equal:
								tcOp = TextCompareOperator.IsEqual;
								break;
							default:
								throw new ArgumentException(
											S._("Invalid compare operator for a keyword that maps to textual content"),
											"euslQuery");
						}
						
						string s;
						if (e.Number != -1L)
							s = e.Number.ToString();
						else if (e.Word != null)
							s = e.Word;
						else 
							s = e.Phrase;
						
						// throws argument exception if searchstr is too short
						criteria = new FreeTextSearchCriteria(s, sf, tcOp);
					}
					break;
			}
			
			if (e.ExcludeTerm)
				criteria = new ExcludedSearchCriteria(criteria);
		}
	}
}