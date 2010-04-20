// MimeCategoryMapping.cs
// 
// Copyright (C) 2010 Patrick Ulbrich
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

namespace Basenji
{
	// class that maps data items to mime cateogories (e.g. pictures)
	public static class MimeCategoryMapping
	{
		public static Dictionary<string, T> GetMapping<T>(T directoryCategoryData,
		                                                  T textCategoryData,
		                                                  T documentCategoryData,
		                                                  T musicCategoryData,
		                                                  T movieCategoryData,
		                                                  T imageCategoryData,
		                                                  T applicationCategoryData,
		                                                  T archiveCategoryData,
		                                                  T developmentCategoryData) {
			// mimetype -> category data mapping
			Dictionary<string, T> mapping = new Dictionary<string, T>() {
				/* directories */
				{ "x-directory/normal",									directoryCategoryData },
				/* text */
				{ "text/plain",											textCategoryData },
				/* documents */
				{ "application/vnd.oasis.opendocument.text",			documentCategoryData },
				{ "application/vnd.oasis.opendocument.spreadsheet",		documentCategoryData },
				{ "application/vnd.oasis.opendocument.presentation",	documentCategoryData },
				{ "application/rtf",									documentCategoryData },
				{ "application/msword",									documentCategoryData },
				{ "application/vnd.ms-excel",							documentCategoryData },
				{ "application/pdf",									documentCategoryData },
				{ "application/xml",									documentCategoryData },
				{ "text/html",											documentCategoryData },
				/* music */				
				{ "audio/mpeg",											musicCategoryData },
				{ "audio/mp4",											musicCategoryData },
				{ "audio/x-flac",										musicCategoryData },
				{ "application/ogg",									musicCategoryData },
				{ "audio/ogg",											musicCategoryData },
				{ "audio/x-wav",										musicCategoryData },
				{ "audio/x-speex",										musicCategoryData },
				/* movies */
				{ "video/x-msvideo",									movieCategoryData },
				{ "video/quicktime",									movieCategoryData },
				{ "video/avi",											movieCategoryData },
				{ "video/mp4",											movieCategoryData },
				{ "video/ogg",											movieCategoryData },
				{ "video/x-flv",										movieCategoryData },
				/* images */
				{ "image/jpeg",											imageCategoryData },
				{ "image/png",											imageCategoryData },
				{ "image/bmp",											imageCategoryData },
				{ "image/x-xpixmap",									imageCategoryData },
				{ "image/gif",											imageCategoryData },
				{ "image/tiff",											imageCategoryData },
				{ "image/x-pcx",										imageCategoryData },
				{ "image/x-xcf",										imageCategoryData },
				{ "image/x-psd",										imageCategoryData },
				{ "image/x-portable-bitmap",							imageCategoryData },
				{ "image/x-portable-anymap",							imageCategoryData },
				{ "image/svg+xml",										imageCategoryData },
				{ "image/x-ico",										imageCategoryData },
				{ "image/x-icns",										imageCategoryData },
				{ "image/x-panasonic-raw",								imageCategoryData },
				/* applications */
				{ "application/x-executable",							applicationCategoryData },
				{ "application/x-shellscript",							applicationCategoryData },
				{ "application/x-ms-dos-executable",					applicationCategoryData },
				/* archives */
				{ "application/x-tar",									archiveCategoryData },
				{ "application/zip",									archiveCategoryData },
				{ "application/x-rar",									archiveCategoryData },
				{ "application/x-bzip-compressed-tar",					archiveCategoryData },
				{ "application/x-compressed-tar",						archiveCategoryData },
				{ "application/x-gzip",									archiveCategoryData },
				{ "application/x-deb",									archiveCategoryData },
				{ "application/x-rpm",									archiveCategoryData },
				{ "application/x-java-archive",							archiveCategoryData },
				/* development */				
				{ "text/x-csrc",										developmentCategoryData },
				{ "text/x-c++src",										developmentCategoryData },
				{ "text/x-python",										developmentCategoryData },
				{ "text/x-csharp",										developmentCategoryData },
				{ "text/x-java",										developmentCategoryData },
				{ "text/x-sql",											developmentCategoryData },
			};
			
			return mapping;
		}
	}
}
