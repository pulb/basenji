// DatabaseProperties.cs
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

// TODO : 
// if other VolumeDBDataType types (Volume, VolumeItem...) overwrite Dispose(),
// DatabaseProperties must overwrite as well!
using System;

namespace VolumeDB
{
	public sealed class DatabaseProperties : VolumeDBDataType
	{
		public const int MAX_NAME_LENGTH		= 64;
		public const int MAX_DESCRIPTION_LENGTH = 4096;
		
		// table info required by VolumeDBDataType
		private const			string		tableName			= "DatabaseProperties";
		private static readonly string[]	primarykeyFields	= null;
		
		private string			name;
		private string			description;
		private DateTime		created;
		private int				version;
		private string			guid;
		
		private VolumeDatabase	database;
		
		internal DatabaseProperties(VolumeDatabase database) : base(tableName, primarykeyFields) {
			this.name			= null;
			this.description	= null;
			this.created		= DateTime.MinValue;
			this.version		= 0;
			this.guid			= null;
			
			this.database		= database;
		}
		
		#region read-only properties
		public DateTime Created {
			get				{ return created; }
			internal set	{ created = value; }
		}

		public int Version {
			get				{ return version; }
			internal set	{ version = value; }
		}
		
		public string Guid {
			get				{ return guid; }
			internal set	{ guid = value; }
		}
		#endregion

		#region editable properties
		public string Name {
			get { return name ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_NAME_LENGTH);
				name = value;
			}
		}
		
		public string Description {
			get { return description ?? string.Empty; }
			set {
				EnsurePropertyLength(value, MAX_DESCRIPTION_LENGTH);
				description = value;
			}
		}
		#endregion
		
		internal override void ReadFromVolumeDBRecord(IRecordData recordData) {
			name			= Util.ReplaceDBNull<string>(	recordData["Name"], null);
			description		= Util.ReplaceDBNull<string>(	recordData["Description"], null);
			created			= Util.ReplaceDBNull<DateTime>(	recordData["Created"], DateTime.MinValue);
			version			= (int)(long)				  	recordData["Version"];
			guid			= (string)						recordData["GUID"];
		}

		internal override void WriteToVolumeDBRecord(IRecordData recordData) {
			recordData.AddField("Name",			name);
			recordData.AddField("Description",	description);
			recordData.AddField("Created",		created);
			recordData.AddField("Version",		version);
			recordData.AddField("GUID",			guid);
		}
		
		internal override void InsertIntoDB() {
			throw new NotSupportedException("This object supports updating only");
		}

		public override void UpdateChanges() {
			if (database == null)
				throw new InvalidOperationException("No database associated");

			database.UpdateDBProperties(this);
		}
		
	}
}
