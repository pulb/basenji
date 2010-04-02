// FilesystemScannerOptions.cs
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

namespace VolumeDB.VolumeScanner
{
	public class FilesystemScannerOptions : ScannerOptions
	{
		public FilesystemScannerOptions() : base() {
			DiscardSymLinks = false;
			GenerateThumbnails = false;
			ExtractMetaData = false;
			ExtractionBlacklist = null;
			DbDataPath = null;
		}
		
		public bool DiscardSymLinks {
			get; set;
		}
		
		public bool GenerateThumbnails {
			get; set;
		}
		
		public bool ExtractMetaData {
			get; set;
		}
		
		public string[] ExtractionBlacklist {
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
			tmp.ExtractMetaData = this.ExtractMetaData;
			tmp.ExtractionBlacklist = this.ExtractionBlacklist;
			tmp.DbDataPath = this.DbDataPath;
		}
	}
}
