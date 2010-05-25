// AbstractEUSLSearchCriteria.cs
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
using System.Globalization;
using VolumeDB.Searching.EUSL.Scanning;
using VolumeDB.Searching.EUSL.Parsing;

namespace VolumeDB.Searching
{
	public abstract class AbstractEUSLSearchCriteria : ISearchCriteria
	{	
		private ISearchCriteria	searchCriteria;
		
		public AbstractEUSLSearchCriteria(string euslQuery) {
			
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
				ISearchCriteria currCriteria;
				
				OnTermParsed(e, out currCriteria);
				
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
		
		/*protected*/ internal abstract void OnTermParsed(TermParsedEventArgs e, out ISearchCriteria criteria);
		
		protected static long GetByteSize(string sizeStr) {
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
