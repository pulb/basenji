// DuplicateOptions.cs
// 
// Copyright (C) 2009 Patrick Ulbrich <zulu99@gmx.net>
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

namespace LibExtractor
{	
	public enum DuplicateOptions
	{
		NONE						= 0,
		/* ignore the 'type' of the keyword when eliminating duplicates */
		DUPLICATES_TYPELESS			= 1,
		/* remove type 'UNKNOWN' if there is a duplicate keyword of
		   known type, even if usually different types should be
		   preserved */
		DUPLICATES_REMOVE_UNKNOWN	= 2
	}
}
