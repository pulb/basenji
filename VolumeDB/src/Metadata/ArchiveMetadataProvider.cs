// ArchiveMetadataProvider.cs
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
using System.Collections.Generic;
using System.IO;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;
using Platform.Common.Diagnostics;

namespace VolumeDB.Metadata
{
	public sealed class ArchiveMetadataProvider : MetadataProvider
	{	
		// enable detailed debugging messages
		private const bool VERBOSE = true;
		
		private delegate void ArchiveMetadataDelegate(string filename, List<MetadataItem> metadata);
		
		// TODO : add support for more archive formats, e.g. RAR
		private readonly Dictionary<string, ArchiveMetadataDelegate> supportedArchiveTypes
		= new Dictionary<string, ArchiveMetadataDelegate>() {
			{ "application/zip",					GetZipMetadata },
			{ "application/x-zip-compressed",		GetZipMetadata },
			{ "application/x-java-archive",			GetZipMetadata },
			{ "application/x-tar",					GetUncompressedTarMetadata },
			{ "application/x-compressed-tar",		GetCompressedTarMetadata },
			{ "application/x-bzip-compressed-tar",	GetBZipCompressedTarMetadata }
		};
		
		public ArchiveMetadataProvider () {
			
		}
		
		public override IEnumerable<MetadataItem> GetMetadata(string filename, string mimetype) {
			EnsureNotDisposed();
			
			ArchiveMetadataDelegate GetMetadata;
			
			if ((mimetype == null) || !supportedArchiveTypes.TryGetValue(mimetype, out GetMetadata)) {
				if (VERBOSE && Global.EnableDebugging) {
					if (mimetype == null)
						Debug.WriteLine("ArchiveMetadataProvider got a Mimetype nullptr");
					else
						Debug.WriteLine("ArchiveMetadataProvider does not like files of type " + mimetype);
				}
				return null;
			}
			
			// only instanciate the list after ensuring that the mimetype is actually supported.
			// save resourecs -> GetMetadata() is called on every single file.
			List<MetadataItem> metadata = new List<MetadataItem>();
			
			GetMetadata(filename, metadata);
			
			if (metadata.Count == 0)
				return null;
			
			return metadata;
		}
		
		// may throw ZipException, e.g. on password protected files
		private static void GetZipMetadata(string filename, List<MetadataItem> metadata) {
			using (ZipInputStream s = new ZipInputStream(File.OpenRead(filename))) {
				ZipEntry e;
				
				while ((e = s.GetNextEntry()) != null) {
					if (e.IsFile || e.IsDirectory) {
						// Note: directories can be identified by a ending slash
						metadata.Add(new MetadataItem(MetadataType.FILENAME, e.Name));
					}
				}
			}
		}
		
		private static void GetUncompressedTarMetadata(string filename, List<MetadataItem> metadata) {
			GetTarMetadata(File.OpenRead(filename), metadata);
		}
		
		private static void GetCompressedTarMetadata(string filename, List<MetadataItem> metadata) {
			GetTarMetadata(new GZipInputStream(File.OpenRead(filename)), metadata);
		}
		
		private static void GetBZipCompressedTarMetadata(string filename, List<MetadataItem> metadata) {
			GetTarMetadata(new BZip2InputStream(File.OpenRead(filename)), metadata);
		}
		
		private static void GetTarMetadata(Stream s, List<MetadataItem> metadata) {
			using (TarInputStream s2 = new TarInputStream(s)) {
				TarEntry e;
				
				while ((e = s2.GetNextEntry()) != null) {
					// Note: directories can be identified by a ending slash
					metadata.Add(new MetadataItem(MetadataType.FILENAME, e.Name));
				}
			}
		}
	}
}
