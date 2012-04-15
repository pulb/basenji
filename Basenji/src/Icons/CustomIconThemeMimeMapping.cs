// CustomIconThemeMimeMapping.cs
// 
// Copyright (C) 2010, 2012 Patrick Ulbrich
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
using Gdk;

namespace Basenji.Icons
{
	// basic support for custom mimetype icons
	public class CustomIconThemeMimeMapping
	{ 
		// mimetype -> custom mime icon mapping
		// (maps mimetypes to icons that are actually available in the custom icon theme)
		private readonly Dictionary<string, Icon> MIME_MAPPING = MimeCategoryMapping
			.GetMapping<Icon>(/*directoryCategoryData:*/	Icon.Stock_Directory,
			                  /*textCategoryData:*/			Icon.Category_Texts,
			                  /*documentCategoryData:*/		Icon.Category_Documents,
			                  /*musicCategoryData:*/		Icon.Category_Music,
			                  /*movieCategoryData:*/		Icon.Category_Movies,
			                  /*imageCategoryData:*/		Icon.Category_Images,
			                  /*applicationCategoryData:*/	Icon.Category_Applications,
			                  /*archiveCategoryData:*/		Icon.Category_Archives,
			                  /*textCategoryData:*/			Icon.Category_Texts);

		public bool TryGetIconForMimeType(string mimeType, out Icon icon) {
			if (mimeType == null)
				throw new ArgumentNullException("mimeType");
			
			if (mimeType.Length == 0)
				throw new ArgumentException("Argument is emtpy", "mimeType");
			
			return MIME_MAPPING.TryGetValue(mimeType, out icon);
		}
	}
}
