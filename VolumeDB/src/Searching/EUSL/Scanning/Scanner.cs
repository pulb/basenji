// Scanner.cs
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

// TODO :	currently all whitespace is ignored.
//			it should possibly be returned as a token (tokendkind.whitespace)
//			and compared against the grammer by the parser.

using System;
using System.Text;

namespace VolumeDB.Searching.EUSL.Scanning
{	
	// scanner that analyzes the given text in volumedatabe search language format 
	// and segments it into tokens
	internal class Scanner
	{	
		private const char CHAR_EOF = char.MinValue; // signals EOF
		
		private string			text;
		private Token			currentToken;
		private Token			lookAheadToken;
		
		private int				currentPos;
		private char			currentChar;
		
		private StringBuilder	wordBuf;
		
		/// <summary>
		/// Constructor that initializes the scanner with the given Text.
		/// The first token can be accessed through the CurrentToken property,
		/// the following token through the LookAheadToken property.
		/// Throws ScannerException.
		/// </summary>
		/// <param name="text">
		/// A <see cref="System.String"/>
		/// </param>
		public Scanner(string text) {

			if (text == null)
				throw new ArgumentNullException("text");
			
			wordBuf			= new StringBuilder();
			 
			currentToken	= new Token();
			lookAheadToken	= new Token();
			
			Reset(text);
		}
		
		/// <summary>
		/// Default constructor. 
		/// Throws ScannerException.
		/// </summary>
		public Scanner() : this(string.Empty) {}
		
		public string Text {
			get { return text; }
			set { Reset(value);	}
		}
		
		public Token CurrentToken {
			get { return currentToken; }
		}
		
		public Token LookAheadToken {
			get { return lookAheadToken; }
		}		
		
		public void ReadNextToken() {
			Token tmpToken	= currentToken;
		   	currentToken	= lookAheadToken;
		   	lookAheadToken	= tmpToken;
		   	
		   	lookAheadToken.Reset();
		   	
		   	Scan();
		   	SkipWhiteSpace();
		}
		
		private void ReadNextChar() {
			if (currentPos < (text.Length - 1)) {
		   		currentPos++;
		   		currentChar = text[currentPos];
		   	} else {
		   		currentPos	= text.Length;
		   		currentChar	= CHAR_EOF;
		   	}
		}
		
		private void Scan() {
			// token can be one of these:
			// MINUS, PLUS, WORD_OR_KEYWORD, PHRASE, NUMBER, 
			// COLLECT (&&, ||, and, AND, or, OR), RELATION
   			
   			lookAheadToken.pos = currentPos;
   			
   			// evaluate start char
   			switch (currentChar) {
   				case CHAR_EOF:
   					lookAheadToken.kind = TokenKind.EOF;
   					break;
   				case '-':
   					lookAheadToken.kind = TokenKind.MINUS;
	   				ReadNextChar();
   					break;
   				case '+':
   					lookAheadToken.kind = TokenKind.PLUS;
	   				ReadNextChar();
   					break;
   				case '"':
   					ScanPhrase();
   					break;
   				case '&':
   					ReadNextChar();
   					if (currentChar == '&') {
   						lookAheadToken.kind = TokenKind.COLLECT_AND;
	   					ReadNextChar();
   					} else {
   						lookAheadToken.kind = TokenKind.UNKNOWN;
   					}
   					break;
   				case '|':
   					ReadNextChar();
   					if (currentChar == '|') {
   						lookAheadToken.kind = TokenKind.COLLECT_OR;
	   					ReadNextChar();
   					} else {
   						lookAheadToken.kind = TokenKind.UNKNOWN;
   					}
   					break;
   				case '<':
   					ReadNextChar();
   					if (currentChar == '=') {
	   					lookAheadToken.kind = TokenKind.RELATION_LESS_OR_EQUAL;
	   					ReadNextChar();
   					} else {
   						lookAheadToken.kind = TokenKind.RELATION_LESS;
   					}
   					break;
   				case '>':
   					ReadNextChar();
   					if (currentChar == '=') {
	   					lookAheadToken.kind = TokenKind.RELATION_GREATER_OR_EQUAL;
	   					ReadNextChar();
   					} else {
   						lookAheadToken.kind = TokenKind.RELATION_GREATER;
   					}
   					break;
   				case '=':
   					lookAheadToken.kind = TokenKind.RELATION_EQUAL;
   					ReadNextChar();
   					break;
   				case ':':
   					lookAheadToken.kind = TokenKind.RELATION_CONTAINS;
   					ReadNextChar();
   					break;
   				default:
   					if (IsValidTokenAutomatChar(currentChar)) {
   						TokenAutomat();
   					} else {
   						lookAheadToken.kind = TokenKind.UNKNOWN;
   						ReadNextChar();
   					}
   					break;
   			}
		}
		
   		private void ScanPhrase() {
   			wordBuf.Length = 0; // reset stringbuilder
   			
   			ReadNextChar(); // consume phrase start delimeter
   			while (currentChar != '"' && currentChar != CHAR_EOF) {
   				wordBuf.Append(currentChar);
   				ReadNextChar();
   			}
   			
   			if (currentChar == CHAR_EOF) {
   				throw new ScannerException("Phrase it not terminated (EOF)", currentPos);
   			} else {
   				ReadNextChar(); // consume phrase end delimeter
   			}
   			
   			lookAheadToken.kind = TokenKind.PHRASE;
   			lookAheadToken.text = wordBuf.ToString();
   		}
   		
   		/* token automat states
   		 *	
   		 * (Start)
   		 *	  |
		 *    +--'A'------------------------------------------------------------------------------------------------->(A)-'N'->(AN)-'D'->(AND)
   		 *	  |                                                                                                        |        |         |
   		 *    +--'a'--------------------------------------------------------------------->(a)-'n'->(an)-'d'->(and)     |        |         |
   		 *	  |                                                                            |        |         |        |        |         |
   		 *    +--'O'--------------------------------------------------->(O)-'R'->(OR)      |        |         |        |        |         |
   		 *    |                                                          |        |        |        |         |        |        |         |
   		 *    +--'o'--------------------------------->(o)-'r'->(or)      |        |        |        |         |        |        |         |
   		 *    |                                        |        |        |        |        |        |         |        |        |         |
   		 *    +--Digit---------------->(Number)        |        |        |        |        |        |         |        |        |         |
   		 *    |                               |        |        |        |        |        |        |         |        |        |         |
   		 *    |                         ^     |        |        |        |        |        |        |         |        |        |         |
         *    |                         |     |        |        |        |        |        |        |         |        |        |         |
         *    |                         +Digit+        |        |        |        |        |        |         |        |        |         |
         *    |                               |        |        |        |        |        |        |         |        |        |         |
         *    |                               +->(End) |        +->(End) |        +->(End) |        |         +->(End) |        |         +->(End)
         *    |                               |        |        |        |        |        |        |         |        |        |         |
         *    |                               C        |        C        |        C        |        |         C        |        |         C
         *    |                               |        |        |        |        |        |        |         |        |        |         |
   		 *    |                               v        v        v        v        v        v        v         v        v        v         v
   		 *    |
   		 *    +--C \ { A, a, O, o, Digit }-->(                                        Word or Keyword                                      ) +-> (End)
   		 *                                                                                                                                   |
   		 *                                                                                 ^                                                 |
   		 *                                                                                 |________________________C________________________|
   		 *
   		 *    C := { valid automat chars } (see IsValidAutomatChar())
   		 */

   		private const int INCOMPLETE_STATE = (1 << 1);
		private enum ScanState : int {
			Start				= (1 << 2),
			A					= (1 << 3)	| INCOMPLETE_STATE,
			AN					= (1 << 4)	| INCOMPLETE_STATE,
			AND					= (1 << 5),
			a					= (1 << 6)	| INCOMPLETE_STATE,
			an					= (1 << 7)	| INCOMPLETE_STATE,
			and					= (1 << 8),
			O					= (1 << 9)	| INCOMPLETE_STATE,
			OR					= (1 << 10),
			o					= (1 << 11)	| INCOMPLETE_STATE,
			or					= (1 << 12),
			Number				= (1 << 13),
			Word_Or_Keyword		= (1 << 14)			
		}
		
		private void TokenAutomat() {
			// token can be one of these (MINUS, PLUS, PHRASE and RELATION have been filtered out in Scan()):
			// WORD_OR_KEYWORD, NUMBER, COLLECT (and, AND, or, OR)
			
   			long num		= 0;
   			wordBuf.Length	= 0; // reset stringbuilder
   			ScanState state	= ScanState.Start;   			

			// run automat
   			while (IsValidTokenAutomatChar(currentChar)) {
   				switch (state) {
   					case ScanState.Start:
   						switch (currentChar) {
   							case 'A':
   								state = ScanState.A;
   								break;
   							case 'a':
   								state = ScanState.a;
   								break;
   							case 'O':
   								state = ScanState.O;
   								break;
   							case 'o':
   								state = ScanState.o;
   								break;
   							default:
   								if (char.IsDigit(currentChar)) {
   									AppendDigit(ref num, currentChar);
   									state = ScanState.Number;
   								} else {
   									wordBuf.Append(currentChar);
   									state = ScanState.Word_Or_Keyword;
   								}
   								break;
   						}
   						break;
   					case ScanState.A:
   						if (currentChar == 'N') {
   							state = ScanState.AN;
   						} else {
   							wordBuf.Append('A').Append(currentChar);
   							state = ScanState.Word_Or_Keyword;
   						}
   						break;
   					case ScanState.AN:
   						if (currentChar == 'D') {
   							state = ScanState.AND;
   						} else {
   							wordBuf.Append("AN").Append(currentChar);
   							state = ScanState.Word_Or_Keyword;
   						}
   						break;
   					case ScanState.AND:
   						wordBuf.Append("AND").Append(currentChar);
   						state = ScanState.Word_Or_Keyword;
   						break;
   					case ScanState.O:
	   					if (currentChar == 'R') {
	   						state = ScanState.OR;
   						} else {
   							wordBuf.Append('O').Append(currentChar);
   							state = ScanState.Word_Or_Keyword;
   						}
   						break;
   					case ScanState.OR:
   						wordBuf.Append("OR").Append(currentChar);
   						state = ScanState.Word_Or_Keyword;
   						break;
   					case ScanState.a:
   						if (currentChar == 'n') {
   							state = ScanState.an;
   						} else {
   							wordBuf.Append('a').Append(currentChar);
   							state = ScanState.Word_Or_Keyword;
   						}
   						break;
   					case ScanState.an:
   						if (currentChar == 'd') {
   							state = ScanState.and;
   						} else {
   							wordBuf.Append("an").Append(currentChar);
   							state = ScanState.Word_Or_Keyword;
   						}
   						break;
   					case ScanState.and:
   						wordBuf.Append("and").Append(currentChar);
   						state = ScanState.Word_Or_Keyword;
   						break;
   					case ScanState.o:
	   					if (currentChar == 'r') {
	   						state = ScanState.or;
   						} else {
   							wordBuf.Append('o').Append(currentChar);
   							state = ScanState.Word_Or_Keyword;
   						}
   						break;
   					case ScanState.or:
   						wordBuf.Append("or").Append(currentChar);
   						state = ScanState.Word_Or_Keyword;
   						break;
   					case ScanState.Number:
   						if (char.IsNumber(currentChar)) {
   							AppendDigit(ref num, currentChar);
   						} else { // e.g. 10a, 10b, 0x10
   							wordBuf.Append(num).Append(currentChar);
   							state = ScanState.Word_Or_Keyword;
   						}
   						break;
   					case ScanState.Word_Or_Keyword:
   						wordBuf.Append(currentChar);
   						break;   					 
				}
				ReadNextChar();
   			}
   			
   			// evaluate automat state and set token
   			switch (state) {
   				case ScanState.AND:
   				case ScanState.and:
   					lookAheadToken.kind = TokenKind.COLLECT_AND;
   					break;
   				case ScanState.OR:
   				case ScanState.or:
   					lookAheadToken.kind = TokenKind.COLLECT_OR;
   					break;
   				case ScanState.Number:
   					lookAheadToken.kind = TokenKind.NUMBER;
   					lookAheadToken.value = num;
   					break;
   				default:
   					lookAheadToken.kind = TokenKind.WORD_OR_KEYWORD;
   					if (((int)state & INCOMPLETE_STATE) == INCOMPLETE_STATE) {
   						// set word text of incomplete states (A, AN, a, an, O, o)
   						lookAheadToken.text = state.ToString(); // reflection magic
   					} else {
   						lookAheadToken.text = wordBuf.ToString();
   					}
   					break;
   			}
		}
		
		// Note: 
		// when changing this condition make sure the following chars are EXCLUDED:
		//    - CHAR_EOF
		//	  - whitespace (e.g. space, tab, CR, LF)
		//    - '<', '>', '=', ':' (in contrast to +/- and collect, 
		//	    relations are not required to be prefixed by whitespace.)
		//
		// '.' should be allowed in any case since users often search for something like picture.jpg
		// '-' and '_' may be identified as whitespace instead (e.g. in SkipWhiteSpace()), like google does.
		// 
		// Also don't forget to adjust the the automat sketch and the grammer sktetch in Parser.cs.
		private static bool IsValidTokenAutomatChar(char c) {
			return	!(	c == CHAR_EOF || char.IsWhiteSpace(c) ||
						c == '<' || c == '>' || c == '=' || c == ':');
			/*
			return 	char.IsLetterOrDigit(c) ||
					c == '.' || c == ',' || c == '&' || 
					c == '_' || c == '-' || c == '+' ||
					c == '#' || c == '%' || c == '~' ||
					c == '$' || c == '\'' || c == '\"' ||
					c == '!' || c == '?';
			*/
		}
		
		private void AppendDigit(ref long num, char c) {
			long n = num;
			
			num *= 10;
   			num += (long)(c - '0');
   			
   			if (num < n)  {
   				// overflow detected.
   				// a number token can't be passed to the exception 
   				// since the number could have become a word token 
   				// in a later state as well. 
   				// so just pass the scanner position instead.
   				throw new ScannerException("Number is too big", currentPos);
   			}
		}
		
		private void SkipWhiteSpace() {
			while (char.IsWhiteSpace(currentChar)) {
				ReadNextChar();
			}
		}
		
		private void Reset(string text) {
			this.text = text;
			
			currentToken.Reset();
			lookAheadToken.Reset();
			
			currentPos	= -1;
			currentChar	= CHAR_EOF;
			
			ReadNextChar();		// init currentChar
			SkipWhiteSpace();	// skip possible whitespace in the beginning
			ReadNextToken();	// init lookahead token
			ReadNextToken();	// init first token
		}
	}
}
