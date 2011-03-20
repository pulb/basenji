// MetadataStore.cs
//
// Copyright (C) 2011 Patrick Ulbrich
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
using System.Text;
using System.Collections.Generic;

namespace VolumeDB.Metadata
{
	public struct MetadataStore
	{		
		public static readonly MetadataStore Empty = new MetadataStore((string)null);
		
		private readonly string packedString;
		
		internal MetadataStore(string metadataString) {
			// NOTE: nullstrings are allowed
			if (string.IsNullOrEmpty(metadataString)) {
				packedString = null;
				return;
			}
			
			if (metadataString[0] != '[')
				throw new ArgumentException("String contains unsupported metadata");
			
			packedString = metadataString;
		}
		
		internal MetadataStore(IEnumerable<MetadataItem> metadata) {
			if (metadata == null) {
				packedString = null;
				return;
			}

			StringBuilder sbHeader	= new StringBuilder();
			StringBuilder sbData	= new StringBuilder();

			sbHeader.Append('[');
			foreach (MetadataItem i in metadata) {
				if (IsBadMetadata(i))
				    continue;
				
				if (sbHeader.Length > 1)
					sbHeader.Append(':');
				
				sbHeader.Append((int)i.Type).Append(':').Append(i.Value.Length.ToString());
				sbData.Append(i.Value);
			}
			sbHeader.Append(']');

			if (sbData.Length == 0)
				packedString = null;
			else
				packedString = (sbHeader.ToString() + sbData.ToString());
		}
		
		internal string MetadataString {
			get {
				return packedString;
			}
		}
		
		public MetadataItem[] ToArray() {
			if (packedString == null) {
				return new MetadataItem[0];
			}

			int headerEndIdx	= packedString.IndexOf(']');
			string strHeader	= packedString.Substring(1,  headerEndIdx - 1);
			string strData		= packedString.Remove(0, headerEndIdx + 1);

			string[] headerVals = strHeader.Split(new char[] { ':' });
			MetadataItem[] metadata = new MetadataItem[headerVals.Length / 2];
			int pos = 0;
			
			for (int i = 0; i < headerVals.Length; i += 2) {
				MetadataType type	= (MetadataType)int.Parse(headerVals[i]);
				int valueLen		= int.Parse(headerVals[i + 1]);
				
				metadata[i / 2] = new MetadataItem(type, strData.Substring(pos, valueLen));
				pos += valueLen;
			}

			return metadata;
		}
		
		public Dictionary<MetadataType, string> ToDictionary() {
			Dictionary<MetadataType, string> dict = new Dictionary<MetadataType, string>();

			if (packedString == null)
			    return dict;

			MetadataItem[] metadata = ToArray();
			foreach (MetadataItem i in metadata) {
				string existing;

				// join items of the same type (e.g. format or filename)
				// (a dictionary can't contain the same key multiple times)
				if (dict.TryGetValue(i.Type, out existing))
					dict[i.Type] =  string.Format("{0}; {1}", existing, i.Value);
				else
					dict.Add(i.Type, i.Value);
			}
			return dict;
		}
		
		public bool IsEmpty {
			get {
				return (packedString == null);
			}
		}
		
		public static bool operator ==(MetadataStore a, MetadataStore b) {
			return a.MetadataString == b.MetadataString;
		}
		
		public static bool operator !=(MetadataStore a, MetadataStore b) {
			return a.MetadataString != b.MetadataString;
		}
		
		public override bool Equals (object o) {
			 if (!(o is MetadataStore))
				return false;
				
			 return this == (MetadataStore)o;
		}
		
		public override int GetHashCode() {
			return (packedString == null) ? string.Empty.GetHashCode() : packedString.GetHashCode();
		}
		
		private static bool IsBadMetadata(MetadataItem item) {
			// skip data that is already available in other
			// database fields or unreliable.
			if (	(item.Type == MetadataType.MIMETYPE) ||
			    	(item.Type == MetadataType.THUMBNAILS) ||
			    	(item.Type == MetadataType.THUMBNAIL_DATA) ||
			    	(item.Type == MetadataType.CONTENT_TYPE)
			    )
				return true;
			
			// skip 0-length durations.
			if ((item.Type == MetadataType.DURATION) && (item.Value == "0"))
				return true;
			
			return false;
		}
	}
}

