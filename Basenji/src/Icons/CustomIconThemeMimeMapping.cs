// CustomIconThemeMimeMapping.cs
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
using Gdk;

namespace Basenji.Icons
{
	// basic support for custom mimetype icons
	public class CustomIconThemeMimeMapping
	{ 
		// mimetype -> custom mime icon mapping
		// (maps mimetypes to icons that are actually available in the custom icon theme)
		private readonly Dictionary<string, Icon> MIME_MAPPING = MimeCategoryMapping
			.GetMapping<Icon>(new MimeCategoryData<Icon>() {
				DirectoryCategory	= Icon.Stock_Directory,
				TextCategory		= Icon.Category_Texts,
				DocumentCategory	= Icon.Category_Documents,
				MusicCategory		= Icon.Category_Music,
				MovieCategory		= Icon.Category_Movies ,
				ImageCategory		= Icon.Category_Images,
				ApplicationCategory	= Icon.Category_Applications,
				ArchiveCategory		= Icon.Category_Archives,
				DevelopmentCategory	= Icon.Category_Texts
			});
		
//		// mimetype -> custom mime icon mapping
//		// (maps mimetypes to icons that are actually available in the custom icon theme)
//		// Note: the mapping differs from the mapping of the CategoryView widget (e.g. the development icon), 
//		// though additions to the mapping of the CategoryView widget might be useful here as well.
//		private readonly Dictionary<string, Icon> MIME_MAPPING = new Dictionary<string, Icon>() {
//			/* directories */
//			{ "x-directory/normal",									Icon.Stock_Directory },
//			/* text */
//			{ "text/plain",											Icon.Category_Texts },
//			/* documents */
//			{ "application/vnd.oasis.opendocument.text",			Icon.Category_Documents },
//			{ "application/vnd.oasis.opendocument.spreadsheet",		Icon.Category_Documents },
//			{ "application/vnd.oasis.opendocument.presentation",	Icon.Category_Documents },
//			{ "application/rtf",									Icon.Category_Documents },
//			{ "application/msword",									Icon.Category_Documents },
//			{ "application/vnd.ms-excel",							Icon.Category_Documents },
//			{ "application/pdf",									Icon.Category_Documents },
//			{ "application/xml",									Icon.Category_Documents },
//			{ "text/html",											Icon.Category_Documents },
//			/* music */				
//			{ "audio/mpeg",											Icon.Category_Music },
//			{ "audio/mp4",											Icon.Category_Music },
//			{ "audio/x-flac",										Icon.Category_Music },
//			{ "application/ogg",									Icon.Category_Music },
//			{ "audio/ogg",											Icon.Category_Music },
//			{ "audio/x-wav",										Icon.Category_Music },
//			{ "audio/x-speex",										Icon.Category_Music },
//			/* movies */
//			{ "video/x-msvideo",									Icon.Category_Movies },
//			{ "video/quicktime",									Icon.Category_Movies },
//			{ "video/mp4",											Icon.Category_Movies },
//			{ "video/ogg",											Icon.Category_Movies },
//			{ "video/x-flv",										Icon.Category_Movies },
//			/* images */
//			{ "image/jpeg",											Icon.Category_Images },
//			{ "image/png",											Icon.Category_Images },
//			{ "image/bmp",											Icon.Category_Images },
//			{ "image/x-xpixmap",									Icon.Category_Images },
//			{ "image/gif",											Icon.Category_Images },
//			{ "image/tiff",											Icon.Category_Images },
//			{ "image/x-pcx",										Icon.Category_Images },
//			{ "image/x-xcf",										Icon.Category_Images },
//			{ "image/x-psd",										Icon.Category_Images },
//			{ "image/x-portable-bitmap",							Icon.Category_Images },
//			{ "image/x-portable-anymap",							Icon.Category_Images },
//			{ "image/svg+xml",										Icon.Category_Images },
//			{ "image/x-ico",										Icon.Category_Images },
//			{ "image/x-icns",										Icon.Category_Images },
//			{ "image/x-panasonic-raw",								Icon.Category_Images },
//			/* applications */
//			{ "application/x-executable",							Icon.Category_Applications },
//			{ "application/x-shellscript",							Icon.Category_Applications },
//			{ "application/x-ms-dos-executable",					Icon.Category_Applications },
//			/* archives */
//			{ "application/x-tar",									Icon.Category_Archives },
//			{ "application/zip",									Icon.Category_Archives },
//			{ "application/x-rar",									Icon.Category_Archives },
//			{ "application/x-bzip-compressed-tar",					Icon.Category_Archives },
//			{ "application/x-compressed-tar",						Icon.Category_Archives },
//			{ "application/x-gzip",									Icon.Category_Archives },
//			{ "application/x-deb",									Icon.Category_Archives },
//			{ "application/x-rpm",									Icon.Category_Archives },
//			{ "application/x-java-archive",							Icon.Category_Archives },
//			/* development */				
//			{ "text/x-csrc",										Icon.Category_Texts },
//			{ "text/x-c++src",										Icon.Category_Texts },
//			{ "text/x-python",										Icon.Category_Texts },
//			{ "text/x-csharp",										Icon.Category_Texts },
//			{ "text/x-java",										Icon.Category_Texts },
//			{ "text/x-sql",											Icon.Category_Texts },
//		};
		
		public CustomIconThemeMimeMapping() {
			DefaultIcon = Icon.Stock_File;
		}
		
		// returned by GetIconNameForMimeType() 
		// if no appropriate icon can be found
		public Icon DefaultIcon { get; set; }
		
		public Icon GetIconForMimeType(string mimeType) {
			if (mimeType == null)
				throw new ArgumentNullException("mimeType");
			
			if (mimeType.Length == 0)
				throw new ArgumentException("Argument is emtpy", "mimeType");
			
			Icon icon;
			if (MIME_MAPPING.TryGetValue(mimeType, out icon))
				return icon;
			
			return DefaultIcon;
		}
	}
}
