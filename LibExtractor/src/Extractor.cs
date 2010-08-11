// Extractor.cs
// 
// Copyright (C) 2008 - 2010 Patrick Ulbrich <zulu99@gmx.net>
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

// NOTE:
//
// The following functions have been implemented directly (based on the libextractor original code)
// as a pinvoke call into the native library would involve a complicated conversion
// of the managed Keyword[] array into a unmanaged linked list.
// On top of that the native library would also try to free that list :-(.
// The code of those functions is so simple that it isn't worth it anyway...
//
// EXTRACTOR_KeywordList * EXTRACTOR_removeDuplicateKeywords(EXTRACTOR_KeywordList * list, unsigned int options);
// EXTRACTOR_KeywordList * EXTRACTOR_removeEmptyKeywords (EXTRACTOR_KeywordList * list);
// EXTRACTOR_KeywordList * EXTRACTOR_removeKeywordsOfType(EXTRACTOR_KeywordList * list, EXTRACTOR_KeywordType type);
// const char * EXTRACTOR_extractLast(EXTRACTOR_KeywordType type, EXTRACTOR_KeywordList * keywords);
// const char * EXTRACTOR_extractLastByString(const char * type, EXTRACTOR_KeywordList * keywords);

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibExtractor
{
	public class Extractor : IDisposable
	{
		private IntPtr pExtractors;
		private bool disposed;
		
		public Extractor() {
			disposed = false;
			pExtractors = IntPtr.Zero;
		}
		
		~Extractor() {
			Dispose(false);
		}
		
		/// 
		/// Instance members
		///
		public void LoadDefaultLibraries() {
			EnsureNotDisposed();
			if (pExtractors != IntPtr.Zero)
				RemoveAllLibraries();
				
			pExtractors = EXTRACTOR_loadDefaultLibraries();
		}

		public void LoadConfigLibraries(string config) {
			EnsureNotDisposed();
			EnsureValidStringParam(config, "config");
			// prev parameter may be null, so don't test for loaded extractors.
			pExtractors = EXTRACTOR_loadConfigLibraries(pExtractors, config);
		}
		
		public void AddLibrary(string library) {
			EnsureNotDisposed();
			EnsureValidStringParam(library, "library");
			// prev parameter may be null, so don't test for loaded extractors.
			pExtractors = EXTRACTOR_addLibrary(pExtractors, library);
		}
		
		public void AddLibraryLast(string library) {
			EnsureNotDisposed();
			EnsureValidStringParam(library, "library");
			// prev parameter may be null, so don't test for loaded extractors.
			pExtractors = EXTRACTOR_addLibraryLast(pExtractors, library);
		}
		
		public void RemoveLibrary(string library) {
			EnsureNotDisposed();
			EnsureValidStringParam(library, "library");
			// prev parameter may be null, so don't test for loaded extractors.
			pExtractors = EXTRACTOR_removeLibrary(pExtractors, library);
		}
		
		public void RemoveAllLibraries() {
			EnsureNotDisposed();
			EXTRACTOR_removeAll(pExtractors);
			pExtractors = IntPtr.Zero;
		}
		
		public Keyword[] GetKeywords(string filename) {
			EnsureNotDisposed();
			EnsureValidStringParam(filename, "filename");
			EnsureExtractors();
			
			List<Keyword> list = GetKeywordsInternal(EXTRACTOR_getKeywords(pExtractors, filename));
			return list.ToArray();
		}
		
		public Keyword[] GetKeywords(IntPtr data, int size) {
			EnsureNotDisposed();
			EnsureExtractors();
			
			if (data == IntPtr.Zero)
				throw new ArgumentException("Data must not be a null pointer", "data");
			if (size <= 0)
				throw new ArgumentException("Size must be greater than 0", "size");
			
			List<Keyword> list = GetKeywordsInternal(EXTRACTOR_getKeywords2(pExtractors, data, size));
			return list.ToArray();
		}
		
		public Keyword[] GetKeywords(byte[] data) {
			EnsureNotDisposed();
			EnsureExtractors();
			
			if (data == null)
				throw new ArgumentNullException("data");
			if (data.Length == 0)
				throw new ArgumentException("Length of data must be greater than 0", "data");
				
			IntPtr pMem = IntPtr.Zero;
			try {
				pMem = Marshal.AllocHGlobal(data.Length);
				Marshal.Copy(data, 0, pMem, data.Length);
				return GetKeywords(pMem, data.Length);
			} finally {
				if (pMem != IntPtr.Zero)
					Marshal.FreeHGlobal(pMem);
			}
		}
		
		private List<Keyword> GetKeywordsInternal(IntPtr pKeywords) {
			try {
				List<Keyword> list = new List<Keyword>();
				IntPtr p = pKeywords;
				
				while (p != IntPtr.Zero) {
					Keyword k = (Keyword)Marshal.PtrToStructure(p, typeof(Keyword));
					
					k.keyword = RemoveBOM(k.keyword);
					
					list.Add(k);
					p = k.next;
				}
				return list;
			} finally {
				if (pKeywords != IntPtr.Zero)
					EXTRACTOR_freeKeywords(pKeywords);
			}
		}
		
		/// 
		/// Static members
		///
		
		// Returns an Extractor instance with the default library set loaded.
		public static Extractor GetDefault() {
			Extractor e = new Extractor();
			e.LoadDefaultLibraries();
			return e;
		}
		
		public static string GetKeywordTypeAsString(KeywordType type) {
			// NOTE : string does NOT need to be freed.
			IntPtr pStr = EXTRACTOR_getKeywordTypeAsString(type);
			string str = Marshal.PtrToStringAnsi(pStr);
			return str;
		}
		
		public static KeywordType GetHighestKeywordTypeNumber() {
			return EXTRACTOR_getHighestKeywordTypeNumber();
		}
		
		public static Keyword[] RemoveDuplicateKeywords(Keyword[] keywords, DuplicateOptions options) {
			int removed = 0;
			
			for (int i = 0; i < keywords.Length; i++) {
				Keyword current = keywords[i];
				
				if (current == null)
					continue;
				
				KeywordType type = current.keywordType;
				string keyword = current.keyword;
				
				for (int j = 0; j < keywords.Length; j++) {
					Keyword pos = keywords[j];
					
					if ((i == j) || (pos == null))
						continue;
					
					if ( (pos.keyword == keyword) &&
					 ( (pos.keywordType == type) ||
					   ( ((options & DuplicateOptions.DUPLICATES_TYPELESS) > 0) &&
					     ( (pos.keywordType == KeywordType.EXTRACTOR_SPLIT) ||
					       (type != KeywordType.EXTRACTOR_SPLIT)) ) ||
					   ( ((options & DuplicateOptions.DUPLICATES_REMOVE_UNKNOWN) > 0) &&
					     (pos.keywordType == KeywordType.EXTRACTOR_UNKNOWN)) ) ) {
						
						if (removed == 0) {
							// do not modify the original array.
							// lazy copy - copy only if the array
							// will be modified.
							keywords = CopyKeywords(keywords);
						}
						
						// mark keyword as removed.
						keywords[j] = null;
						removed++;
					}
				}
			}
			
			return RemoveNullKeywords(keywords, removed);
		}
		
		public static Keyword[] RemoveEmptyKeywords(Keyword[] keywords) {
			List<Keyword> lst = null;
			
			for (int i = 0; i < keywords.Length; i++) {
				Keyword pos = keywords[i];
				string keyword = pos.keyword;
				bool allWhite = true;
				
				if (keyword != null) {
					for (int j = 0; j < keyword.Length; j++) {
						if (!char.IsWhiteSpace(keyword[j])) {
							allWhite = false;
							break;
						}
					}
				}
				
				if (allWhite) {
					if (lst == null) {
						// lazy copy -
						// copy only if keywords will actually be removed.
						lst = new List<Keyword>(keywords.Length);
						lst.AddRange(keywords);
					}
					lst.Remove(pos);
				}
			}
			
			if (lst == null)
				return keywords;
			else
				return lst.ToArray();
		}
		
		public static Keyword[] RemoveKeywordsOfType(Keyword[] keywords, KeywordType type) {
			List<Keyword> lst = null;
			
			for (int i = 0; i < keywords.Length; i++) {
				Keyword pos = keywords[i];
				
				if (pos.keywordType == type) {
					if (lst == null) {
						// lazy copy -
						// copy only if keywords will actually be removed.
						lst = new List<Keyword>(keywords.Length);
						lst.AddRange(keywords);
					}
					lst.Remove(pos);
				}
			}
			
			if (lst == null)
				return keywords;
			else
				return lst.ToArray();
		}
		
		public static string ExtractLast(KeywordType type, Keyword[] keywords) {
			string result = null;
			for (int i = 0; i < keywords.Length; i++) {
				Keyword pos = keywords[i];
				if (pos.keywordType == type) {
					result = pos.keyword;
				}
			}
			return result;
		}
		
		// NOTE : does not work with translated strings.
		public static string ExtractLastByString(string type, Keyword[] keywords) {
			string result = null;
			for (int i = 0; i < keywords.Length; i++) {
				Keyword pos = keywords[i];
				if (GetKeywordTypeAsString(pos.keywordType) == type) {
					result = pos.keyword;
				}
			}
			return result;
		}
		
		private static Keyword[] CopyKeywords(Keyword[] original) {
			Keyword[] copy = new Keyword[original.Length];
			Array.Copy(original, copy, original.Length);
			return copy;
		}
		
		private static Keyword[] RemoveNullKeywords(Keyword[] keywords, int nullCount) {
			if (nullCount < 1)
				return keywords;
			
			Keyword[] copy = new Keyword[keywords.Length - nullCount];
			int n = 0;
			
			for (int i = 0; i < keywords.Length; i++) {
				if (keywords[i] != null)
					copy[n++] = keywords[i];
			}
			
			return copy;
		}
		
		// remove unicode byte ordering mark
		private static string RemoveBOM(string s) {
			const char BOM_UTF16_BE = (char)0xFEFF;
			const char BOM_UTF16_LE = (char)0xFFFE;
			
			if ((!string.IsNullOrEmpty(s)) && (s[0] == BOM_UTF16_BE || s[0] == BOM_UTF16_LE))
				return s.Substring(1, s.Length -1);
			else
				return s;
		}
		
		/// 
		/// Cleanup stuff
		///
		
		#region IDisposable members
		public void Dispose() {
			Dispose(true);
		}
		
		private void Dispose(bool disposing) {
			if (!disposed) {
				if (pExtractors != IntPtr.Zero)
						RemoveAllLibraries();
						
				if (disposing) 						
					GC.SuppressFinalize(this);
			
				disposed = true;
			}
		}
		#endregion
		
		/// 
		/// Helper methods
		///
		
		private void EnsureNotDisposed() {
			if (disposed)
				throw new ObjectDisposedException("Extractor");
		}
		
		private void EnsureExtractors() {
			if (pExtractors == IntPtr.Zero)
				throw new InvalidOperationException("No extractor libraries loaded");
		}
		
		private void EnsureValidStringParam(string param, string paramName) {
			if (param == null)
				throw new ArgumentNullException(paramName);
			if (param.Length == 0)
				throw new ArgumentException("Parameter must not be null", paramName);
		}
		
		/// 
		/// Native libextractor imports
		///
		
		#region Native imports
		[DllImport("libextractor-1")]
		private static extern IntPtr EXTRACTOR_loadDefaultLibraries();
		
		[DllImport("libextractor-1")]
		private static extern IntPtr EXTRACTOR_loadConfigLibraries(IntPtr prev, string config);
		
		[DllImport("libextractor-1")]
		private static extern IntPtr EXTRACTOR_addLibrary(IntPtr prev, string library);
		
		[DllImport("libextractor-1")]
		private static extern IntPtr EXTRACTOR_addLibraryLast(IntPtr prev, string library);
		
		[DllImport("libextractor-1")]
		private static extern IntPtr EXTRACTOR_removeLibrary(IntPtr prev, string library);
			
		[DllImport("libextractor-1")]
		private static extern void EXTRACTOR_removeAll(IntPtr libraries);
		
		[DllImport("libextractor-1")]
		private static extern IntPtr EXTRACTOR_getKeywords(IntPtr extractors, string filename);
		
		[DllImport("libextractor-1")]
		private static extern IntPtr EXTRACTOR_getKeywords2(IntPtr extractors, IntPtr data, int size);
		
		[DllImport("libextractor-1")]
		private static extern void EXTRACTOR_freeKeywords(IntPtr keywords);		
		
		[DllImport("libextractor-1")]
		private static extern IntPtr EXTRACTOR_getKeywordTypeAsString(KeywordType type);
		
		[DllImport("libextractor-1")]
		private static extern KeywordType EXTRACTOR_getHighestKeywordTypeNumber();
		#endregion
	}
}
