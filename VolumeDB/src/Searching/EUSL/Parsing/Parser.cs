// Parser.cs
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

/*
	End User Search Language (EUSL)
	
	Slightly modified xesam end user search language, see	http://xesam.org/main/XesamUserSearchLanguage,
															http://grillbar.org/xesam/end-user-sl.old.png
	Modifications:
		* whitespace between [+|-] and Term allowed
		* Number introduced
		* no modifier support
		* Select Term uses custom keywords instead of xesam keywords

	Notes:
		* Collect is optional, default is AND
		* + is ignored, - means NOT


	Grammer in EBNF (http://www.w3.org/TR/2004/REC-xml-20040204/#sec-notation): 
	
		Search_Statement	::= (['+'|'-'] [WS] Term ([WS Collect] WS ['+'|'-'] [WS] Term)*
		
		Term				::= Select | Phrase | Word | Number
		
		Select				::= Keyword [WS] Relation [WS] (Word | Phrase | Number)
		
		Phrase				::= '"' String '"'
		
		Collect				::= 'AND' | 'OR' | 'and' | 'or' | '&&' | '||'
		
		Keyword				::= Any Word that is recognized as a keyword
								and mapped to searchfields by a client app (e.g. by volumedb)
								
		Relation			::= '<' | '>' | '=' | '<=' | '>=' | ':'
		
		WS					::= (#x20 | #x09 | #x0D | #x0A)+
		
		Word				::= Any unicode character sequence that does not start with any terminal used by this grammer
								and does not include WS and Relation terminals ('<', '>', '=', ':').
		                        
		String				::= Any unicode character sequence (inclusive whitespace).
		
		Number				::= ('0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9')+
*/

using System;
using System.Text;

using VolumeDB.Searching.EUSL.Scanning;

namespace VolumeDB.Searching.EUSL.Parsing
{	
	internal class Parser
	{
		private const long NUMBER_NONE = -1L;
		
		private Scanner	scanner;
		private ParsedData parsedData;
		
		public Parser() {
			scanner = new Scanner();
			parsedData = new ParsedData();
		}
		
		/// <summary>
		/// Parses the given Text and raises 
		/// TermParsed and CollectParsed events 
		/// where appropriate.
		/// Throws UnexpectedTokenException and ScannerException.
		/// </summary>
		/// <param name="text">
		/// A <see cref="System.String"/>
		/// </param>
		public void Parse(string text) {
			Init(text);
			ParseStatement();
		}
		
		private void ParseStatement() {
			// Search_Statement	::= (['+'|'-'] [WS] Term ([WS Collect] WS ['+'|'-'] [WS] Term)*
			
			if (scanner.CurrentToken.kind == TokenKind.MINUS) { 
				parsedData.excludeTerm = true;
				scanner.ReadNextToken();
			} else if (scanner.CurrentToken.kind == TokenKind.PLUS) {
				scanner.ReadNextToken();
			}
			
			ParseTerm();
			
			while (scanner.CurrentToken.kind != TokenKind.EOF) {				
				
				if (scanner.CurrentToken.kind == TokenKind.COLLECT_AND || 
					scanner.CurrentToken.kind == TokenKind.COLLECT_OR) {
					ParseCollect();
				}
				
				parsedData.excludeTerm = false;
				
				if (scanner.CurrentToken.kind == TokenKind.MINUS) { 
					parsedData.excludeTerm = true;
					scanner.ReadNextToken();
				} else if (scanner.CurrentToken.kind == TokenKind.PLUS) {
					scanner.ReadNextToken();
				}
			
				ParseTerm();
			}
		}
		
		private void ParseTerm() {
			// Term ::= Select | Phrase | Word | Number
			
			switch (scanner.CurrentToken.kind) {
				case TokenKind.WORD_OR_KEYWORD:
					
					switch (scanner.LookAheadToken.kind) {
						case TokenKind.RELATION_CONTAINS:
						case TokenKind.RELATION_EQUAL:
						case TokenKind.RELATION_GREATER:
						case TokenKind.RELATION_GREATER_OR_EQUAL:
						case TokenKind.RELATION_LESS:
						case TokenKind.RELATION_LESS_OR_EQUAL:
							// interpret current word as keyword
							ParseSelect();
							break;
						default:
							ParseWord(false);
							break;
					}
					
					break;
				case TokenKind.PHRASE:
					ParsePhrase(false);
					break;
				case TokenKind.NUMBER:
					ParseNumber(false);
					break;
				default:
					RaiseUnexpectedTokenException(	TokenKind.WORD_OR_KEYWORD, 
													TokenKind.PHRASE,
													TokenKind.NUMBER);
					break;
			}

		}
		
		private void ParseSelect() {
			// Select ::= Keyword [WS] Relation [WS] (Word | Phrase | Number)

			ParseKeyword();
			ParseRelation();
			
			string word		= null;
			string phrase	= null;
			long number		= NUMBER_NONE;
			
			switch(scanner.CurrentToken.kind) {
				case TokenKind.WORD_OR_KEYWORD:
					ParseWord(true);
					word = parsedData.word;
					break;
				case TokenKind.PHRASE:
					ParsePhrase(true);
					phrase = parsedData.phrase;
					break;
				case TokenKind.NUMBER:
					ParseNumber(true);
					number = parsedData.number;
					break;
				default:
					RaiseUnexpectedTokenException(	TokenKind.WORD_OR_KEYWORD, 
													TokenKind.PHRASE, 
													TokenKind.NUMBER);
					break;
			}
			
			OnTermParsed(	TermType.Select, 
							word,
							phrase,
							number,
							parsedData.keyword,
							parsedData.relation,
							parsedData.excludeTerm);
		}
		
		private void ParsePhrase(bool suppressEvent) {
			// Phrase := '"' String '"'
			
			parsedData.phrase = scanner.CurrentToken.text;
			
			if (!suppressEvent) {
				OnTermParsed(	TermType.Phrase,
								null,
								parsedData.phrase,
								NUMBER_NONE,
								null,
								Relation.None,
								parsedData.excludeTerm);
			}
			scanner.ReadNextToken();
		}
		
		private void ParseWord(bool suppressEvent) {
			// Word	::= Any unicode character sequence that does not start with any terminal used by the grammer
			//			and does not include WS and Relation terminals ('<', '>', '=', ':').
			
			parsedData.word = scanner.CurrentToken.text;
			
			if (!suppressEvent) {
				OnTermParsed(	TermType.Word,
								parsedData.word,
								null,
								NUMBER_NONE,
								null,
								Relation.None,
								parsedData.excludeTerm);
			}
			scanner.ReadNextToken();
		}
		
		private void ParseNumber(bool suppressEvent) {
			// Number ::= ('0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9')+
			
			parsedData.number = scanner.CurrentToken.value;
			
			if (!suppressEvent) {
				OnTermParsed(	TermType.Number,
								null,
								null,
								parsedData.number,
								null,
								Relation.None,
								parsedData.excludeTerm);
			}
			scanner.ReadNextToken();
		}
		
		private void ParseCollect() {
			// Collect ::= 'AND' | 'OR' | 'and' | 'or' | '&&' | '||'
			
			if (scanner.CurrentToken.kind == TokenKind.COLLECT_AND) {
				parsedData.collect = Collect.And;
				OnCollectParsed(parsedData.collect);
			} else {
				parsedData.collect = Collect.Or;
				OnCollectParsed(parsedData.collect);
			}
			scanner.ReadNextToken();
		}
		
		private void ParseKeyword() {
			// Keyword	::= Any Word that is recognized as a keyword
			//				and mapped to searchfields by a client app (e.g. by volumedb)
			
			//try {
			//	parsedData.searchField = SearchField.FromString(scanner.CurrentToken.text);
			//} catch (ArgumentException) {
			//	throw new UnknownSearchFieldException(	"Unknown SearchField",
			//											scanner.CurrentToken.text);
			//}
			parsedData.keyword = scanner.CurrentToken.text;
			 
			scanner.ReadNextToken();
		}
		
		private void ParseRelation() {
			// Relation ::= '<' | '>' | '=' | '<=' | '>=' | ':'
			
			Relation rel = Relation.None;
			switch (scanner.CurrentToken.kind) {
				case TokenKind.RELATION_CONTAINS:
					rel = Relation.Contains;
					break;
				case TokenKind.RELATION_EQUAL:
					rel = Relation.Equal;
					break;
				case TokenKind.RELATION_GREATER:
					rel = Relation.Greater;
					break;
				case TokenKind.RELATION_GREATER_OR_EQUAL:
					rel = Relation.GreaterOrEqual;
					break;
				case TokenKind.RELATION_LESS:
					rel = Relation.Less;
					break;
				case TokenKind.RELATION_LESS_OR_EQUAL:
					rel = Relation.LessOrEqual;
					break;
			}
			parsedData.relation = rel;
			
			scanner.ReadNextToken();
		}
		
		private void RaiseUnexpectedTokenException(params TokenKind[] expectedTokenKinds) {
			
			Token t = scanner.CurrentToken;
			StringBuilder sb = new StringBuilder();
				
			for (int i = 0; i < expectedTokenKinds.Length; i++) {
				if (i > 0)
					sb.Append(" or ");
				sb.Append(expectedTokenKinds[i].ToString());
			}
			
			string msg = string.Format("Expected {0} at position {1} but found {2}", 
								sb.ToString(), t.pos, t.kind);
									
			throw new UnexpectedTokenException( msg,
												t,
												expectedTokenKinds);
		}
		
		private void Init(string text) {
			parsedData.Reset();
			scanner.Text = text;
		}
		
		public event TermParsedEventHandler		TermParsed;
		public event CollectParsedEventHandler	CollectParsed;
		
		protected virtual void OnTermParsed(TermType tt,
											string word,
											string phrase,
											long number,
											string keyword,
											Relation r,
											bool exclude) {
			if (TermParsed != null)
				TermParsed(this, new TermParsedEventArgs(tt, 
														word,
														phrase,
														number,
														keyword,
														r,
														exclude));
		}
		
		protected virtual void OnCollectParsed(Collect c) {
			if (CollectParsed != null)
				CollectParsed(this, new CollectParsedEventArgs(c));
		}
		
		private class ParsedData
		{
			public bool			excludeTerm;
			public string		word;
			public string		phrase;
			public long			number;
			public string		keyword;
			public Relation		relation;
			public Collect		collect;
			
			public ParsedData() {
				Reset();
			}
			
			public void Reset() {
				excludeTerm	= false;
				word		= null;
				phrase		= null;
				number		= NUMBER_NONE;
				keyword		= null;
				relation	= Relation.None;
				collect		= Collect.None;
			}
		}
	}
}
