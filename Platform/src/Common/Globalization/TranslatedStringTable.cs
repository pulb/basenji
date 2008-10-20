// TranslatedStringTable.cs
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
using System.Collections;
using System.Collections.Generic;

namespace Platform.Common.Globalization
{	
	public class TranslatedStringTable : IEnumerable<KeyValuePair<string, string>>
	{
		private List<string> untranslatedStrings;
		private List<string> translatedStrings;
		private Dictionary<string, int> untranslatedIndices;
		private Dictionary<string, int> translatedIndices;		
		
		public TranslatedStringTable() {
			untranslatedStrings = new List<string>();
			translatedStrings = new List<string>();
			untranslatedIndices = new Dictionary<string, int>();
			translatedIndices = new Dictionary<string, int>();
		}
		
		public void Add(string untranslatedString, string translatedString) {
			if (untranslatedString == null || translatedString == null)
				throw new ArgumentNullException();
				
			untranslatedStrings.Add(untranslatedString);
			translatedStrings.Add(translatedString);
			untranslatedIndices.Add(translatedString, untranslatedStrings.Count - 1);
			translatedIndices.Add(untranslatedString, translatedStrings.Count - 1);
		}
		
		public bool Contains(string untranslatedString) {
			if (untranslatedString == null)
				throw new ArgumentNullException();
				
			return translatedIndices.ContainsKey(untranslatedString);
		}
		
		public void Clear() {
			untranslatedStrings.Clear();
			translatedStrings.Clear();
			untranslatedIndices.Clear();
			translatedIndices.Clear();
		}
		
		public int Count {
			get { return untranslatedStrings.Count; }
		}		
		
		public ICollection<string> UntranslatedStrings {
			get { return untranslatedStrings; }
		}
		
		public ICollection<string> TranslatedStrings {
			get { return translatedStrings; }
		}
		
		public string GetUntranslatedString(int idx) {
			return untranslatedStrings[idx];
		}
		
		public string GetUntranslatedString(string translatedString) {
			if (translatedString == null)
				throw new ArgumentNullException();
				
			int idx = untranslatedIndices[translatedString];
			return untranslatedStrings[idx];
		}
		
		public string GetTranslatedString(int idx) {
			return translatedStrings[idx];
		}
		
		public string GetTranslatedString(string untranslatedString) {
			if (untranslatedString == null)
				throw new ArgumentNullException();
				
			int idx = translatedIndices[untranslatedString];
			return translatedStrings[idx];
		}
		
		public bool TryGetUntranslatedString(string translatedString, out string untranslatedString) {
			if (translatedString == null)
				throw new ArgumentNullException();
				
			int idx;
			untranslatedString = null;
			if (!untranslatedIndices.TryGetValue(translatedString, out idx))
				return false;
			
			untranslatedString = untranslatedStrings[idx];
			return true;
			
		}
		
		public bool TryGetTranslatedString(string untranslatedString, out string translatedString) {
			if (untranslatedString == null)
				throw new ArgumentNullException();
				
			int idx;
			translatedString = null;
			if (!translatedIndices.TryGetValue(untranslatedString, out idx))
				return false;
			
			translatedString = translatedStrings[idx];
			return true;
		}
		
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			for (int i = 0; i < untranslatedStrings.Count; i++) {
				yield return new KeyValuePair<string, string>(untranslatedStrings[i], translatedStrings[i]);
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			for (int i = 0; i < untranslatedStrings.Count; i++) {
				yield return new DictionaryEntry(untranslatedStrings[i], translatedStrings[i]);
			}
		}
	}
}
