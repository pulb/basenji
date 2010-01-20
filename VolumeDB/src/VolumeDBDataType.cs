// VolumeDBDataType.cs
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
using System.Collections;
using System.Collections.Generic;

namespace VolumeDB
{	
	/// <summary>
	/// Baseclass of all types that represent a datatype of the VolumeDatabase.
	/// </summary>
	public abstract class VolumeDBDataType : IVolumeDBRecord
	{
		//private bool	  isInserted;
		private string		tableName;
		private string[]	primarykeyFields;
		private bool		isNew;
		
		internal VolumeDBDataType(string tableName, string[] primarykeyFields) {
			if (tableName == null)
				throw new ArgumentNullException("tableName");

			// don't check primarykeyFields for null.
			// primarykeyFields can be null for tables without primarykey and just one record.
			
			this.tableName			= tableName;
			this.primarykeyFields	= primarykeyFields;
			this.isNew				= true;
		}
		
		#region IVolumeDBRecord Members
		
		string IVolumeDBRecord.TableName {
			get { return tableName; }
		}

		string[] IVolumeDBRecord.PrimaryKeyFields {
			get {
				if ((primarykeyFields != null) && (primarykeyFields.Length > 0)) {				  
					// pass a copy of the primarykeyFields array, 
					// the string elements themselves are immutable and don't need to be copied.
					string[] copy = new string[primarykeyFields.Length];
					Array.Copy(primarykeyFields, copy, primarykeyFields.Length);
					return copy;
				} else {
					return new string[0];				 
				}
			}
		}

		bool IVolumeDBRecord.IsNew {
			get { return isNew; }
			set { isNew = value; }
		}

		IRecordData IVolumeDBRecord.GetRecordData() {
			IRecordData recordData = new __RecordData_Dictionary_Impl();
			WriteToVolumeDBRecord(recordData);
			return recordData;
		}

		void IVolumeDBRecord.SetRecordData(IRecordData recordData) {
			ReadFromVolumeDBRecord(recordData);
		}

		#endregion
		
		// TODO : make this member internally protected in case this language feature has become real
		// see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=33c53cf6-2709-4cc9-a408-6cafee4313ef
		//protected
		internal
		abstract void ReadFromVolumeDBRecord(IRecordData recordData);

		// TODO : make this member internally protected in case this language feature has become real
		// see http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=33c53cf6-2709-4cc9-a408-6cafee4313ef
		//protected
		internal
		abstract void WriteToVolumeDBRecord(IRecordData recordData);

		/// <summary>
		/// Creates a new record from the object in the associated VolumeDatabase object.
		/// Publicly inaccessible.
		/// </summary>
		internal abstract void InsertIntoDB();

		/// <summary>
		/// Saves back changes to the associated VolumeDatabase object.
		/// </summary>
		public abstract void UpdateChanges();
		
		/// <summary>
		/// Indicates whether the object has been inserted into the associated VolumeDatabase object.
		/// </summary>
		internal bool IsInserted {
			get { return !((IVolumeDBRecord)this).IsNew; }
		}
		
		protected static void EnsurePropertyLength(string val, int maxLen) {
			if (val != null && val.Length > maxLen)
				throw new ArgumentException(string.Format("The length of this propertys value must be <= {0}", maxLen));
		}
		
		#region private class __RecordData_Dictionary_Impl
		private class __RecordData_Dictionary_Impl : Dictionary<string, object>, IRecordData {
			public __RecordData_Dictionary_Impl() : base() { }

			#region IRecordData Members

			public new object this[string fieldName] {
				get { return base[fieldName]; }
			}

			public object GetValue(string fieldName) {
				return this[fieldName];
			}

			public void AddField(string fieldName, object value) {
				this.Add(fieldName, value);
			}

			#endregion
			
			#region IEnumerable<FieldnameValuePair> Members

			IEnumerator<FieldnameValuePair> IEnumerable<FieldnameValuePair>.GetEnumerator() {
				foreach (KeyValuePair<string, object> pair in ((IEnumerable<KeyValuePair<string, object>>)this))
					yield return new FieldnameValuePair(pair.Key, pair.Value);
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator() {
				return ((IEnumerable<FieldnameValuePair>)this).GetEnumerator();
			}

			#endregion
		}
		#endregion
		
	}
}
