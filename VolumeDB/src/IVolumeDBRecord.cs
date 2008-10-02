// IVolumeDBRecord.cs
// 
// Copyright (C) 2008 Patrick Ulbrich
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
using System.Collections.Generic;

namespace VolumeDB
{
	/// <summary>
	/// Internal interface to hide members ment to be called by VolumeDatabase reader/writer methods exclusively.
	/// Must be implemented explicitely, for member hiding to take effect.
	/// </summary>
	internal interface IVolumeDBRecord
	{
		string			TableName			{ get; }
		string[]		PrimaryKeyFields	{ get; }

		// in particular this property must not be visible by default
		// (altering this property would break VolumeDatabase.InsertMedia()/Item() and VolumeDatabase.UpdateMedia()/Item())
		bool			IsNew				{ get; set; }

		IRecordData		GetRecordData();
		void			SetRecordData(IRecordData recordData);
	}
}
