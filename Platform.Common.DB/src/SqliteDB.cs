// SqliteDB.cs
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
using System.IO;
using System.Data;

#if WIN32
// requires http://sourceforge.net/projects/sqlite-dotnet2
using System.Data.SQLite;
#else
using Mono.Data.Sqlite;
#endif

// TODO : http://www.mono-project.com/SQL_Lite)
/*
If it finds an integer value, it uses DateTime.FromFileTime, 
which is the reverse of how it encodes DateTimes if you insert a DateTime via parameters. 
If it finds a string value, it uses DateTime.Parse, but note that Parse is a very slow operation. 
So with Sqlite3, DateTimes should be put into DATE or DATETIME columns in the database either through parameters or by turning it into a long with ToFileTime yourself, 
and then they will be read back as DateTimes.
-> Wenn ich das mit ToFileTime() speichere, ist es aber nicht mehr kompatibel mit anderen SQL Servern (z.b. MySQL oder?)
-> Betrifft doch eigentlich nur VolumeDatabase.SqlPrepareValue() und CreateTables()(sqlInsert=..) ? Da statt .ToString() .ToFileTime() nutzen?
*/
namespace Platform.Common.DB
{
	public static class SqliteDB
	{
		public static IDbConnection Open(string dbPath, bool create) {
			if (dbPath == null)
				throw new ArgumentNullException("dbPath");
				
			if (create) {
				if (File.Exists(dbPath))
					File.Delete(dbPath);
#if WIN32
				SQLiteConnection.CreateFile(dbPath);
#else
				SqliteConnection.CreateFile(dbPath);
#endif				
			} else {
				if (!File.Exists(dbPath))
					throw new FileNotFoundException(string.Format("Database '{0}' not found", dbPath));
			}
				
			IDbConnection conn;
			string connStr = string.Format("Data Source={0}", dbPath);
#if WIN32
			conn = new SQLiteConnection(connStr);
#else
			conn = new SqliteConnection(connStr);
#endif
			conn.Open();
			return conn;  
		} 
	}
}