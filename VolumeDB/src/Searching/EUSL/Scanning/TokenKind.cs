// TokenKind.cs
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
	internal enum TokenKind
	{
		MINUS,
		PLUS,
		WORD_OR_KEYWORD, /* a word is interpreted as a keyword by the parser if it is followed by a relation operator */
		PHRASE,
		NUMBER,
		COLLECT_AND,
		COLLECT_OR,
		RELATION_LESS,
		RELATION_LESS_OR_EQUAL,
		RELATION_GREATER,
		RELATION_GREATER_OR_EQUAL,
		RELATION_EQUAL,
		RELATION_CONTAINS,
		EOF,
		UNKNOWN
	}
}
