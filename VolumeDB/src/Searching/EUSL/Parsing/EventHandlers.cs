// EventHandlers.cs
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

namespace VolumeDB.Searching.EUSL.Parsing
{
	internal delegate void TermParsedEventHandler(object sender, TermParsedEventArgs e);
	internal delegate void CollectParsedEventHandler(object sender, CollectParsedEventArgs e);
	
	internal class TermParsedEventArgs : EventArgs
	{
		private TermType	termType;
		private string		word;
		private string		phrase;
		private long		number;
		private string		keyword;
		private Relation	relation;
		private bool		excludeTerm;
		
		public TermParsedEventArgs( TermType tt,
									string		word,
									string		phrase,
									long		number,
									string		keyword,
									Relation	r,
									bool		exclude
									) {
									
			this.termType		= tt;
			this.word			= word;
			this.phrase			= phrase;
			this.number			= number;
			this.keyword		= keyword;
			this.relation		= r;
			this.excludeTerm	= exclude;
		}
		
		public TermType TermType {
			get {
				return termType;
			}
		}
		
		public string Word {
			get {
				if (TermType != TermType.Word && TermType != TermType.Select)
					RaiseInvalidPropertyException();
				return word;
			}
		}		
		
		public string Phrase {
			get {
				if (TermType != TermType.Phrase && TermType != TermType.Select)
					RaiseInvalidPropertyException();
				return phrase;
			}
		}
		
		public long Number {
			get {
				if (TermType != TermType.Number && TermType != TermType.Select)
					RaiseInvalidPropertyException();
				return number;
			}
		}
		
		public string Keyword {
			get {
				if (TermType != TermType.Select)
					RaiseInvalidPropertyException();
				return keyword;
			}
		}
		
		public Relation Relation {
			get {
				if (TermType != TermType.Select)
					RaiseInvalidPropertyException();
				return relation;
			}
		}
		
		public bool ExcludeTerm {
			get {
				return excludeTerm;
			}
		}
		
		private void RaiseInvalidPropertyException() {
			throw new InvalidOperationException(
						"This property is invalid for the current termtype");
		}
	}
	
	internal class CollectParsedEventArgs : EventArgs
	{
		public CollectParsedEventArgs(Collect c) : base() {
			this.Collect = c;
		}
		
		public Collect Collect {
			get;
			private set;
		}
		
	}
}
