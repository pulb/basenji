// FileVolumeItem.cs
// 
// Copyright (C) 2008 Patrick Ulbrich
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

namespace VolumeDB
{
	public sealed class FileVolumeItem : FileSystemVolumeItem, IHashable
	{
		//private string  filename;
		//private string  extension;
		private long	size;
		private string	hash;
		
		internal FileVolumeItem(VolumeDatabase database)
			: base(database, VolumeItemType.FileVolumeItem)
		{
			//filename	= null;
			//extension = null;
			size	  = 0L;
			hash	  = null;
		}
		
		/// <summary>
		/// <para>Required by internal factory methods like VolumeScannerBase.GetNewVolumeItem<TVolumeItem>()</para>
		/// <para>Purpose :</para>
		/// <para>
		/// - guarantee that _all_ fields of this type are initialized by the caller 
		///  (in contrast to property initialization, which easily makes you miss a property [in particular if a new one was added..])
		/// </para>
		/// <para>
		/// - seperate fields of a type from fields of its base type (e.g. GetNewVolumeItem<VolumeItem>() initializes all fields of a the VolumeItem base type. 
		/// Caller code only needs to initialize fields of the derived <TVolumeItem> type)
		/// </para>
		/// </summary>
		internal void SetFileVolumeItemFields(/*string extension,*/ long size, string hash) {
			//ValidateFilename(filename);

			//this.filename  = filename;
			//this.extension = extension;
			this.size	   = size;
			this.hash	   = hash;
		}
		
		// FileVolumeItem specific implementation of IChildItem.GetParent()
		public DirectoryVolumeItem GetDirectory() {
			return (DirectoryVolumeItem) ((IChildItem)this).GetParent();
		}
		
		 //private static void ValidateFilename(string filename)
		//{
		//	  if (filename == null)
		//		  throw new ArgumentNullException("filename");

		//	  if (filename.Length == 0)
		//		  throw new ArgumentException("filename is empty");
		//}
		
		internal override void ReadFromVolumeDBRecord(IRecordData recordData) {
			base.ReadFromVolumeDBRecord(recordData);

			//filename	=	(string)				recordData["Filename"];
			//extension =	ReplaceDBNull<string>(	recordData["Extension"],  null);
			size	  =   (long)				  recordData["Size"];
			hash	  =   ReplaceDBNull<string>(  recordData["Hash"],		null);
		}

		internal override void WriteToVolumeDBRecord(IRecordData recordData) {
			base.WriteToVolumeDBRecord(recordData);
			
			//recordData.AddField("Filename",  filename);
			//recordData.AddField("Extension", extension);
			recordData.AddField("Size",		 size);
			recordData.AddField("Hash",		 hash);
		}
		
		#region read-only properties
		
		//public string Filename
		//{
		//	  get { return filename ?? string.Empty }
		//	  internal set
		//	  {
		//		  //ValidateFilename(value);
		//		  filename = value;
		//	  }
		//}

		//public string Extension {
		//	get				{ return extension ?? string.Empty; }
		//	  internal set	  { extension = value; }
		//}

		public long Size {
			get				{ return size; }
			internal set	{ size = value; }
		}

		public string Hash {
			get				{ return hash ?? string.Empty; }
			internal set	{ hash = value; }
		}
		
		#endregion
		
	}
}
