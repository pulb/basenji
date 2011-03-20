// FilesystemScannerOptions.cs
//
// Copyright (C) 2010, 2011 Patrick Ulbrich
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
using VolumeDB.Metadata;

namespace VolumeDB.VolumeScanner
{
	public class FilesystemScannerOptions : ScannerOptions
	{
		public FilesystemScannerOptions() : base() {
			DiscardSymLinks = false;
			GenerateThumbnails = false;
			MetadataProvider = null;
			DbDataPath = null;
		}
		
		public bool DiscardSymLinks {
			get; set;
		}
		
		public bool GenerateThumbnails {
			get; set;
		}
		
		public MetadataProvider MetadataProvider {
			get; set;
		}
		
		public string DbDataPath {
			get; set;
		}
		
		protected override void CopyOptions(ScannerOptions opts) {
			
			if (!(opts is FilesystemScannerOptions))
				return;
			
			FilesystemScannerOptions tmp = opts as FilesystemScannerOptions;
			
			tmp.DiscardSymLinks = this.DiscardSymLinks;
			tmp.GenerateThumbnails = this.GenerateThumbnails;
			
			// don't deep-copy the metadata provider, it's too expensive
			// (e.g. native libextractor instantiation and initialization)
			tmp.MetadataProvider = this.MetadataProvider;
			tmp.DbDataPath = this.DbDataPath;
			
///*			
//			if (this.MetadataProvider != null) {
//				// the target may have a different metadataprovider assigned, 
//				// so assign a flat copy of this objects provider
//				tmp.MetadataProvider = (MetadataProvider)this.MetadataProvider.MemberwiseClone();
//
//				this.MetadataProvider.CopyTo(tmp.MetadataProvider);
//			} else {
//				tmp.MetadataProvider = null;
//			}*/
			
			
		}
	}
}
