//  
//  Copyright (C) 2009 Patrick Ulbrich
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Text;
using LibExtractor;

namespace VolumeDB
{	
	internal static class MetaDataHelper
	{		
		public static string PackExtractorKeywords(Keyword[] keywords) {
			if (keywords == null || keywords.Length == 0)
				return null;

			StringBuilder sbHeader	= new StringBuilder();
			StringBuilder sbData	= new StringBuilder();

			sbHeader.Append('[');
			foreach(Keyword kw in keywords) {
				// skip data that is already available in other
				// database fields or unreliable.
				if (	(kw.keywordType == KeywordType.EXTRACTOR_MIMETYPE) ||
				    	(kw.keywordType == KeywordType.EXTRACTOR_THUMBNAILS) ||
				    	(kw.keywordType == KeywordType.EXTRACTOR_THUMBNAIL_DATA)
				    )
						continue;
				
				if (sbHeader.Length > 1)
					sbHeader.Append(':');
				
				sbHeader.Append((int)kw.keywordType).Append(':').Append(kw.keyword.Length.ToString());
				sbData.Append(kw.keyword);
			}
			sbHeader.Append(']');

			if (sbData.Length == 0)
				return null;
			else
				return sbHeader.ToString() + sbData.ToString();
		}

		public static Keyword[] UnpackExtractorKeywords(string strPacked) {
			if (string.IsNullOrEmpty(strPacked))
				return null;

			int headerEndIdx	= strPacked.IndexOf(']');
			string strHeader	= strPacked.Substring(1,  headerEndIdx - 1);
			strPacked			= strPacked.Remove(0, headerEndIdx + 1);

			string[] headerVals = strHeader.Split(new char[] { ':' });
			Keyword[] keywords = new Keyword[headerVals.Length / 2];
			int pos = 0;
			
			for (int i = 0; i < headerVals.Length; i += 2) {
				KeywordType keywordType = (KeywordType)int.Parse(headerVals[i]);
				int keywordLen	= int.Parse(headerVals[i + 1]);
				
				keywords[i / 2] = new Keyword() {
					keywordType	= keywordType,
					keyword		= strPacked.Substring(pos, keywordLen)
				};
				pos += keywordLen;
			}

			return keywords;
		}
		
		// returns the libextractor duration format
		public static string FormatExtractorDuration (double seconds) {
			if (seconds < 60.0)
				return ((int)Math.Round(seconds)).ToString() + "s";
			
			long totalSecs = (long)seconds;
			int mins = (int)(totalSecs / 60);
			int secs = (int)(totalSecs % 60);
			
			if (secs > 0)
				return string.Format("{0}m{1:D2}", mins, secs);
			else
				return string.Format("{0}m", mins);
		}
	}
}
