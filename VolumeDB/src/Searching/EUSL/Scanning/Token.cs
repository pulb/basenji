// Token.cs
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

namespace VolumeDB.Searching.EUSL.Scanning
{
	internal class Token
	{				
		public TokenKind	kind;
		public String		text;
		public long			value;
		public int			pos;

		public Token() {
			Reset();
		}
		
		public void Reset() {
		  this.kind		= TokenKind.EOF;
		  this.text		= string.Empty;
		  this.value	= -1L;
		  this.pos		= -1;
		}
		
		public override string ToString () {
			return kind.ToString();
		}

	}
}
