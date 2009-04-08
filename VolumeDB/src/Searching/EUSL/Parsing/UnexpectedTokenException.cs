// UnexpectedTokenException.cs
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
	internal class UnexpectedTokenException : Exception
	{		
		public UnexpectedTokenException(	string msg,
											Token t,
											params TokenKind[] expectedTokenKinds) : base(msg) {
			
			Token				= t;
			ExpectedTokenKinds	= expectedTokenKinds;
		}
		
		public Token Token {
			get;
			private set;
		}
		
		public TokenKind[] ExpectedTokenKinds {
			get;
			private set;
		}
		
		/*
		public override string Message {
			get {
				StringBuilder sb = new StringBuilder();
				
				for (int i = 0; i < ExpectedTokenKinds.Length; i++) {
					if (i > 0)
						sb.Append(" or ");
					sb.Append(ExpectedTokenKinds[i].ToString());
				}
			
				return string.Format("Expected a {0} ({1}) at position {2} but found {3}", 
									ExpectedTerminalOrNonTerminal, sb.ToString(), Token.pos, Token.kind);
			}
		}*/

	}
}
