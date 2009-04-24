// EUSLSearchCriteria.cs
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
using System.Globalization;
using VolumeDB.Searching.EUSL.Scanning;
using VolumeDB.Searching.EUSL.Parsing;

namespace VolumeDB.Searching.ItemSearchCriteria
{
	public sealed class EUSLSearchCriteria  : ISearchCriteria
	{	
		private ISearchCriteria	searchCriteria;
		
		public EUSLSearchCriteria(string euslQuery)	{
			// TODO :
			// word, number and phrase terms are currently only compared against database field 'Name'.
			// they should probably compared to other common database fields as well.
			// but this would slow down searching a lot.. 
			// maybe some smarter table indexers settings would help.
			
			if (euslQuery == null)
				throw new ArgumentNullException("euslQuery");
				
			// EUSL's default collector is and
			const Collect DEFAULT_COLLECT = Collect.And;

			Parser p = new Parser();
			
			SearchCriteriaGroup	outerOrGroup	= new SearchCriteriaGroup(MatchRule.AnyMustMatch);
			SearchCriteriaGroup innerAndGroup	= null;
			Collect				c				= DEFAULT_COLLECT;
			ISearchCriteria		prevCriteria	= null;
			
			// parser eventhandler definition
			p.CollectParsed += (object sender, CollectParsedEventArgs e) => {
				c = e.Collect;
			};
			
			// parser eventhandler definition
			p.TermParsed += (object sender, TermParsedEventArgs e) => {
				
				// determine current criteria
				ISearchCriteria currCriteria = null;
				switch (e.TermType) {
					case TermType.Number:
						currCriteria = new FreeTextSearchCriteria(	e.Number.ToString(),
																 	FreeTextSearchField.AnyName,
																 	TextCompareOperator.Contains
																 );
						break;
					case TermType.Phrase:
						currCriteria = new FreeTextSearchCriteria(	e.Phrase,
																 	FreeTextSearchField.AnyName,
																 	TextCompareOperator.Contains
																 );
						break;
					case TermType.Word:
						currCriteria = new FreeTextSearchCriteria(	e.Word,
																 	FreeTextSearchField.AnyName,
																 	TextCompareOperator.Contains
																 );
						break;
					case TermType.Select:
					
						string keyword = e.Keyword.ToUpper();
						
						if (keyword == "FILESIZE") {
							
							long byteSize = e.Number;
							if (byteSize == -1L) {
								try {
									byteSize = GetByteSize(e.Word);
								} catch (ArgumentException) {
									throw new ArgumentException(
										S._("Operand for keyword 'filesize' must be a number with an optional multiplier"),
										"euslQuery");
								}
							}
							
							CompareOperator cOp = CompareOperator.Equal;
							switch(e.Relation) {
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
											S._("Invalid compare operator for keyword 'filesize'"),
											"euslQuery");
							}
							
							currCriteria = new FileSizeSearchCriteria(byteSize, cOp);
						
						} else if (keyword == "TYPE") {
						
							if (string.IsNullOrEmpty(e.Word))
								throw new ArgumentException(
									S._("Operand for keyword 'type' must be a string"),
									"euslQuery");
									
							// try to map the word of the type selector to an MediaType
							MediaType type = MediaType.None;							
							try {
								type = MediaType.FromString(e.Word);
							} catch (ArgumentException) {
								throw new ArgumentException(
											string.Format(S._("Unknown type '{0}'"), e.Word),
											"euslQuery");
							}
							
							if (e.Relation != Relation.Contains && e.Relation != Relation.Equal) {
								throw new ArgumentException(
											S._("Keyword 'type' only supports '=' and ':' operators"),
											"euslQuery");
							}
							
							currCriteria = new MediaTypeSearchCriteria(type);
						
						} else {
							// try to map the keyword to freetextsearch fields
							
							IFreeTextSearchField sf;
							
							// try to map the keyword to a VOLUME freetextsearchfield.
							// note: keywords mapping to foreign table fields 
							//       should be prefixed with the respective table name.
							if (keyword == "VOLUME-TITLE") {
								sf = VolumeSearchCriteria.FreeTextSearchField.Title;
							} else {
								// try to map the keyword to an ITEM freetextsearchfield
								try {
									sf = FreeTextSearchField.FromString(e.Keyword);
								} catch (ArgumentException) {
									throw new ArgumentException(
												string.Format(S._("Unknown keyword '{0}'"), e.Keyword),
												"euslQuery");
								}
							}
							
							TextCompareOperator tcOp = TextCompareOperator.Contains;							
							switch(e.Relation) {
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
							currCriteria = new FreeTextSearchCriteria(s, sf, tcOp);
						}
						break;
				}
				
				if (e.ExcludeTerm) {
					currCriteria = new ExcludedSearchCriteria(currCriteria);
				}
				
				// assign previous criteria
				if (prevCriteria != null) {
					if (c == Collect.Or) {
						if (innerAndGroup != null) {
							innerAndGroup.AddSearchCriteria(prevCriteria);
							outerOrGroup.AddSearchCriteria(innerAndGroup);
							innerAndGroup = null;
						} else {
							outerOrGroup.AddSearchCriteria(prevCriteria);
						}
					} else { // and
						if (innerAndGroup == null)
							innerAndGroup = new SearchCriteriaGroup(MatchRule.AllMustMatch);
						
						innerAndGroup.AddSearchCriteria(prevCriteria);
					}
				}
				prevCriteria = currCriteria;

				c = DEFAULT_COLLECT; // restore default collector
			};
			
			try {
				// parse the searchstatement 
				// and call the event handlers.
				// all exceptions occuring in the eventhandlers
				// are thrown here as well.
				p.Parse(euslQuery);
				
			} catch (UnexpectedTokenException e) {
				throw new ArgumentException(
					string.Format(S._("Parsing error: search statement is malformed at position {0}"), e.Token.pos),
					"euslQuery", e);
			} catch (ScannerException e) {
				throw new ArgumentException(
					string.Format(S._("Parsing error: search statement is malformed at position {0}"), e.Pos),
					"euslQuery", e);
			}
			
			// add last remaining criteria
			if (prevCriteria != null) {
				if (innerAndGroup != null) {
					innerAndGroup.AddSearchCriteria(prevCriteria);
					outerOrGroup.AddSearchCriteria(innerAndGroup);
					innerAndGroup = null;
				} else {
					outerOrGroup.AddSearchCriteria(prevCriteria);
				}
			}
			
			searchCriteria = outerOrGroup;
		}
		
		private static long GetByteSize(string sizeStr) {
			int exp = 0;
			
			if (string.IsNullOrEmpty(sizeStr))
				throw new ArgumentException("sizeStr is empty", "sizeStr");
				
			if (sizeStr[sizeStr.Length - 1] == 'B' || sizeStr[sizeStr.Length - 1] == 'b')
				sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
				
			if (sizeStr.Length > 1) {
				switch(sizeStr[sizeStr.Length - 1]) {
					case 'k':
					case 'K':
						exp = 10;
						sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
						break;
					case 'm':
					case 'M':
						exp = 20;
						sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
						break;
					case 'g':
					case 'G':
						exp = 30;
						sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
						break;
					case 't':
					case 'T':
						exp = 40;
						sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
						break;
				}				
			}
			
			double factor;
			NumberStyles ns = NumberStyles.AllowDecimalPoint;
			NumberFormatInfo ni = CultureInfo.InvariantCulture.NumberFormat;
			if (!double.TryParse(sizeStr, ns, ni, out factor))
				throw new ArgumentException("Bad size format");
			
			long bytes = (long)((factor * Math.Pow(2, exp)) + 0.5);
			return bytes;
		}

		#region ISearchCriteria Members
		string ISearchCriteria.GetSqlSearchCondition() {
			return searchCriteria.GetSqlSearchCondition();
		}
		
		SearchCriteriaType ISearchCriteria.SearchCriteriaType {
			get { return searchCriteria.SearchCriteriaType; }
		}
		#endregion
	}
}
