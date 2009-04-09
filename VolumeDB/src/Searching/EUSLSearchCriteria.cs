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
using VolumeDB.Searching.EUSL.Scanning;
using VolumeDB.Searching.EUSL.Parsing;

namespace VolumeDB.Searching
{
	public class EUSLSearchCriteria  : ISearchCriteria
	{	
		private ISearchCriteria searchCriteria;
		
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
							
							if (e.Number == -1L)
								throw new ArgumentException(
									"Operand for keyword 'filesize' must be a number",
									"euslQuery");
								
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
											"Invalid compare operator for keyword 'filesize'",
											"euslQuery");
							}
							
							currCriteria = new FileSizeSearchCriteria(e.Number, cOp);
						
						/*else if (keyword == "TYPE") {
							
							// TODO : implement MediaTypeSearchCriteria
							// and MediaType struct (e.g. Audio, Video, Image, Text, ...)
							// Mediatypes are mapped to the the beginning of the mimetype field.
							// e.g. LIKE 'text/%', LIKE 'audio/%' ...
							
							if (string.IsNullOrEmtpy(e.Word)
								throw new ArgumentException(
									"Operand for keyword 'type' must be a string",
									"euslQuery");
									
							// try to map the keyword to an MediaType
							MediaType mt = MediaType.None;
							
							try {
								MediaType.FromString(e.Keyword);
							} catch (ArgumentException) {
								throw new ArgumentException(
											string.Format("Unknown Type '{0}'", e.Word),
											"euslQuery");
							}
							
							if (e.Relation != Relation.Contains && e.Relation != Relation.Equal) {
								throw new ArgumentException(
											"Keyword 'type' only supports '=' and ':' operators",
											"euslQuery");
							}
							
							currCriteria = new MediaTypeSearchCriteria(mt);
						*/	
						} else {
						
							// try to map the keyword to a freetextsearchfield 
							FreeTextSearchField sf = FreeTextSearchField.None;
							try {
								sf = FreeTextSearchField.FromString(e.Keyword);
							} catch (ArgumentException) {
								throw new ArgumentException(
											string.Format("Unknown keyword '{0}'", e.Keyword),
											"euslQuery");
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
												"Invalid compare operator for a keyword that maps to textual content",
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
					string.Format("Parsing error: search statement is malformed at position {0}", e.Token.pos),
					"euslQuery", e);
			} catch (ScannerException e) {
				throw new ArgumentException(
					string.Format("Parsing error: search statement is malformed at position {0}", e.Pos),
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
		
		#region ISearchCriteria Members
		string ISearchCriteria.GetSqlSearchCondition() {
			return searchCriteria.GetSqlSearchCondition();
		}
		#endregion
	}
}
