// MediaType.cs
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
using System.Collections.Generic;
using System.Text;

namespace VolumeDB.Searching
{	
	public struct MediaType
	{
		private static Dictionary<string, MediaType> stringMapping = new Dictionary<string, MediaType>() {
			{ "AUDIO",		MediaType.Audio		},
			{ "VIDEO",		MediaType.Video		},
			{ "IMAGE",		MediaType.Image		},
			{ "TEXT",		MediaType.Text		},
			{ "DIRECTORY",	MediaType.Directory	}
		};
		
		private uint value;
		
		private MediaType(uint value) {
			this.value = value;
		}
		
		// mimetype sections
		public static MediaType None		{ get { return new MediaType(0);	}} // required for binary &
		public static MediaType Audio		{ get { return new MediaType(1);	}}
		public static MediaType Video		{ get { return new MediaType(2);	}}		
		public static MediaType Image		{ get { return new MediaType(4);	}}
		public static MediaType Text		{ get { return new MediaType(8);	}}
		public static MediaType Directory	{ get { return new MediaType(16);	}}
		
		public static MediaType FromString(string mediaType) {
			MediaType type = MediaType.None;
			
			if (mediaType == null)
				throw new ArgumentNullException("mediaType");
				
			if (!stringMapping.TryGetValue(mediaType.ToUpper(), out type))
				throw new ArgumentException("Unknown mediatype", "mediaType");
			
			return type;
		}
		
		public static bool operator ==(MediaType a, MediaType b) {
			return a.value == b.value;
		}
		
		public static bool operator !=(MediaType a, MediaType b) {
			return a.value != b.value;
		}
		
		public static MediaType operator |(MediaType a, MediaType b) {
			return new MediaType(a.value | b.value);
		}
		
		public static MediaType operator &(MediaType a, MediaType b) {
			return new MediaType(a.value & b.value);
		}
		
		public override bool Equals (object o) {
			 if (!(o is MediaType))
				return false;
				
			 return this == (MediaType)o;
		}
		
		public override int GetHashCode() {
			return (int)value;
		}
		
		/* 
		* indicates whether a MediaType value is a
		* bitwise combination of multiple MediaType values. 
		*/
		public bool IsCombined {
			get {
				return Util.IsCombined(value);
			}
		}
		
		private bool ContainsType(MediaType type) {
			return (this & type) == type;
		}
		
		private static void Append(StringBuilder sql, string condition, MatchRule typeMatchRule) {
			if (sql.Length > 0)
				sql.AppendFormat(" {0} ", typeMatchRule.GetSqlLogicalOperator());

			sql.Append('(').Append(condition).Append(')');
		}
		
		/* get the sql search condition of this/these type/types */
		internal string GetSqlSearchCondition() { return GetSqlSearchCondition(MatchRule.AnyMustMatch); }
		internal string GetSqlSearchCondition(MatchRule typeMatchRule) {
			
			StringBuilder sql = new StringBuilder();

			if (this.ContainsType(Audio))
				Append(sql, "Items.MimeType LIKE 'audio/%'", typeMatchRule);
			
			if (this.ContainsType(Video))
				Append(sql, "Items.MimeType LIKE 'video/%'", typeMatchRule);
				
			if (this.ContainsType(Image))
				Append(sql, "Items.MimeType LIKE 'image/%'", typeMatchRule);
			
			if (this.ContainsType(Text))
				Append(sql, "Items.MimeType LIKE 'text/%'", typeMatchRule);
				
			if (this.ContainsType(Directory))
				Append(sql, "Items.MimeType = 'x-directory/normal'", typeMatchRule);
				
			return sql.ToString();
		}
		
	}
}
