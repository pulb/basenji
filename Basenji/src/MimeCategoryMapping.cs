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
	public class MimeCategoryData<T>
	{
		public T DirectoryCategory		{ get; set; }
		public T TextCategory			{ get; set; }
		public T DocumentCategory		{ get; set; }
		public T MusicCategory			{ get; set; }
		public T MovieCategory			{ get; set; }
		public T ImageCategory			{ get; set; }
		public T ApplicationCategory	{ get; set; }
		public T ArchiveCategory		{ get; set; }
		public T DevelopmentCategory	{ get; set; }
	}
	
	// class that maps data items to mime cateogories (e.g. pictures)
	public static class MimeCategoryMapping
	{
		public static Dictionary<string, T> GetMapping<T>(MimeCategoryData<T> categoryData) {

			if (categoryData == null)
				throw new ArgumentNullException("categoryData");
			
			// mimetype -> category data mapping
			Dictionary<string, T> mapping = new Dictionary<string, T>() {
				/* directories */
				{ "x-directory/normal",									categoryData.DirectoryCategory },
				/* text */
				{ "text/plain",											categoryData.TextCategory },
				/* documents */
				{ "application/vnd.oasis.opendocument.text",			categoryData.DocumentCategory },
				{ "application/vnd.oasis.opendocument.spreadsheet",		categoryData.DocumentCategory },
				{ "application/vnd.oasis.opendocument.presentation",	categoryData.DocumentCategory },
				{ "application/rtf",									categoryData.DocumentCategory },
				{ "application/msword",									categoryData.DocumentCategory },
				{ "application/vnd.ms-excel",							categoryData.DocumentCategory },
				{ "application/pdf",									categoryData.DocumentCategory },
				{ "application/xml",									categoryData.DocumentCategory },
				{ "text/html",											categoryData.DocumentCategory },
				/* music */				
				{ "audio/mpeg",											categoryData.MusicCategory },
				{ "audio/mp4",											categoryData.MusicCategory },
				{ "audio/x-flac",										categoryData.MusicCategory },
				{ "application/ogg",									categoryData.MusicCategory },
				{ "audio/ogg",											categoryData.MusicCategory },
				{ "audio/x-wav",										categoryData.MusicCategory },
				{ "audio/x-speex",										categoryData.MusicCategory },
				/* movies */
				{ "video/x-msvideo",									categoryData.MovieCategory },
				{ "video/quicktime",									categoryData.MovieCategory },
				{ "video/mp4",											categoryData.MovieCategory },
				{ "video/ogg",											categoryData.MovieCategory },
				{ "video/x-flv",										categoryData.MovieCategory },
				/* images */
				{ "image/jpeg",											categoryData.ImageCategory },
				{ "image/png",											categoryData.ImageCategory },
				{ "image/bmp",											categoryData.ImageCategory },
				{ "image/x-xpixmap",									categoryData.ImageCategory },
				{ "image/gif",											categoryData.ImageCategory },
				{ "image/tiff",											categoryData.ImageCategory },
				{ "image/x-pcx",										categoryData.ImageCategory },
				{ "image/x-xcf",										categoryData.ImageCategory },
				{ "image/x-psd",										categoryData.ImageCategory },
				{ "image/x-portable-bitmap",							categoryData.ImageCategory },
				{ "image/x-portable-anymap",							categoryData.ImageCategory },
				{ "image/svg+xml",										categoryData.ImageCategory },
				{ "image/x-ico",										categoryData.ImageCategory },
				{ "image/x-icns",										categoryData.ImageCategory },
				{ "image/x-panasonic-raw",								categoryData.ImageCategory },
				/* applications */
				{ "application/x-executable",							categoryData.ApplicationCategory },
				{ "application/x-shellscript",							categoryData.ApplicationCategory },
				{ "application/x-ms-dos-executable",					categoryData.ApplicationCategory },
				/* archives */
				{ "application/x-tar",									categoryData.ArchiveCategory },
				{ "application/zip",									categoryData.ArchiveCategory },
				{ "application/x-rar",									categoryData.ArchiveCategory },
				{ "application/x-bzip-compressed-tar",					categoryData.ArchiveCategory },
				{ "application/x-compressed-tar",						categoryData.ArchiveCategory },
				{ "application/x-gzip",									categoryData.ArchiveCategory },
				{ "application/x-deb",									categoryData.ArchiveCategory },
				{ "application/x-rpm",									categoryData.ArchiveCategory },
				{ "application/x-java-archive",							categoryData.ArchiveCategory },
				/* development */				
				{ "text/x-csrc",										categoryData.DevelopmentCategory },
				{ "text/x-c++src",										categoryData.DevelopmentCategory },
				{ "text/x-python",										categoryData.DevelopmentCategory },
				{ "text/x-csharp",										categoryData.DevelopmentCategory },
				{ "text/x-java",										categoryData.DevelopmentCategory },
				{ "text/x-sql",											categoryData.DevelopmentCategory },
			};
			
			return mapping;
		}
	}
}
