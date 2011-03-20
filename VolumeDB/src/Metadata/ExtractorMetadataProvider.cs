// ExtractorMetadataProvider.cs
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
using System.Linq;
using System.Collections.Generic;
using LibExtractor;

namespace VolumeDB.Metadata
{

	public sealed class ExtractorMetadataProvider : MetadataProvider
	{
		private bool disposed;
		private Extractor extractor;
		
		public ExtractorMetadataProvider () : this(null) {}
		public ExtractorMetadataProvider (string[] extractionBlacklist)	{
			this.disposed = false;
			
			// may throw DllNotFoundException
			this.extractor = Extractor.GetDefault();
			
			if (extractionBlacklist != null) {
				foreach (string ext in extractionBlacklist)
					this.extractor.RemoveLibrary("libextractor_" + ext);
			}
		}
		
		public override IEnumerable<MetadataItem> GetMetadata(string filename) {
			EnsureNotDisposed();
			
			Keyword[] keywords = extractor.GetKeywords(filename);
			// removes duplicates like the same year in idv2 and idv3 tags,
			// does not remove keywords of the same type with different data (e.g. filename)
			keywords = Extractor.RemoveDuplicateKeywords(keywords, DuplicateOptions.DUPLICATES_REMOVE_UNKNOWN);
			// removes whitespace-only keywords
			keywords = Extractor.RemoveEmptyKeywords(keywords);
			
			if (keywords.Length == 0)
				return null;
			
			return keywords.Select<Keyword, MetadataItem>(kw => new MetadataItem((MetadataType)kw.keywordType, kw.keyword));
		}
		
//		protected override void CopyProvider(MetaDataProvider p) {
//			if (!(p is ExtractorMetadataProvider))
//				return;
//			
//			ExtractorMetadataProvider tmp = p as ExtractorMetadataProvider;
//			
//			if (this.ExtractionBlacklist != null) {
//				string arr = new string[this.ExtractionBlacklist.Length];
//				this.ExtractionBlacklist.CopyTo(arr, 0);
//				tmp.ExtractionBlacklist = arr;
//			} else {
//				tmp.ExtractionBlacklist = null;
//			}
//		}
		
		protected override void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					if (extractor != null)
						extractor.Dispose();
				}
				
				disposed = true;
			}
			
			base.Dispose(disposing);
		}
		
		private static bool? extractorInstalled;
		public static bool IsExtractorInstalled {
			get {
				if (!extractorInstalled.HasValue) {
					try {
						// test if an extractor method throws a DllNotFoundException
						Extractor.GetKeywordTypeAsString(KeywordType.EXTRACTOR_TITLE);
						extractorInstalled = true;
					} catch (DllNotFoundException) {
						extractorInstalled = false;
					}
				}
				
				return extractorInstalled.Value;
			}
		}
	}
}
