// VolumeEditor.cs
// 
// Copyright (C) 2008, 2010 Patrick Ulbrich
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
using Gtk;
using Basenji;
using Basenji.Gui.Base;
using VolumeDB;
using VolumeDB.VolumeScanner;
using Platform.Common.Globalization;

namespace Basenji.Gui.Widgets.Editors
{
	public abstract partial class VolumeEditor : ObjectEditor<Volume>
	{
		private string	volumeType;
		
		private Label	lblVolumeType;
		private Label	lblHashed;
		private Label	lblAdded;
		
		private TreeIter customCategory;
		
		// TODO : 
		// - place "new" button next to the category combobox, 
		//   which will open a dialog to add/edit categories (e.g. add "Roms" category)
		// - suggest category depending on cd content
		public static readonly TranslatedStringTable categories = new TranslatedStringTable() {
			{ "Backup",		S._("Backup")		},
			{ "Documents",	S._("Documents")	},
			{ "Music",		S._("Music")		},
			{ "Movies",		S._("Movies")		},
			{ "Pictures",	S._("Pictures")		},
			{ "Misc",		S._("Misc")			},
			{ "Other",		S._("Other")		}
		};
		
		protected VolumeEditor(string volumeType) : base() {
			customCategory = TreeIter.Zero;
			this.volumeType = volumeType;			 
		}
		
		public static VolumeEditor CreateInstance(VolumeType volType) {
			switch (volType) {
				case VolumeType.FileSystemVolume:
					return new FileSystemVolumeEditor();
				case VolumeType.AudioCdVolume:
					return new AudioCdVolumeEditor();
				default:
					throw new NotImplementedException(string.Format("VolumeEditor widget for VolumeType {0} is not implemented", volType.ToString()));
			}
		}
		
		public Volume Volume { get { return Object; } }
		
		// used by the VolumeScanner window to update the info labels periodically
		public virtual void UpdateInfo(VolumeInfo vi) {			
			UpdateInfoLabels(vi.IsHashed, vi.Added);
		}

		protected override void ValidateForm() {
			// TODO : add further validation			
			if (!dcLoanedDate.IsEmpty && !dcLoanedDate.IsValid)
				throw new ValidationException("not a valid date",
				                              "Loaned date",
				                              dcLoanedDate.DatePattern);
			
			if (!dcReturnDate.IsEmpty && !dcReturnDate.IsValid)
				throw new ValidationException("not a valid date",
				                              "Return date",
				                              dcReturnDate.DatePattern);
		}
		
		protected override void SaveToObject(VolumeDB.Volume volume) {
			// save form
			volume.ArchiveNo = txtArchiveNo.Text.Trim();
			
			// if cmbCategory.ActiveText is empty, no category has been selected for a new volume
			// or the form has been loaded with an empty category string
			if (string.IsNullOrEmpty(cmbCategory.ActiveText)) {
				volume.Category = null;
			} else {
				string category;
				if (!categories.TryGetUntranslatedString(cmbCategory.ActiveText, out category))
					category = cmbCategory.ActiveText; // set user-specified custom category
				volume.Category	= category;
			}
			
			volume.Title		= txtTitle.Text.Trim();
			volume.Description	= tvDescription.Buffer.Text;
			volume.Keywords		= txtKeywords.Text.Trim();
			volume.LoanedTo		= txtLoanedTo.Text.Trim();
			volume.LoanedDate	= dcLoanedDate.IsEmpty ? DateTime.MinValue : dcLoanedDate.Date;
			volume.ReturnDate	= dcReturnDate.IsEmpty ? DateTime.MinValue : dcReturnDate.Date;
		
			volume.UpdateChanges();
		}
		
		protected override void LoadFromObject(VolumeDB.Volume volume) {
			//
			// form
			//
			txtArchiveNo.Text = volume.ArchiveNo;
			
			// remove user-specied custom category, 
			// that possibly has been appended on a previous load of another volume
			if (!customCategory.Equals(TreeIter.Zero)) {
				((ListStore)cmbCategory.Model).Remove(ref customCategory);
				customCategory = TreeIter.Zero;
			}
			
			// unselect category
			cmbCategory.Active = -1;
			if (volume.Category.Length > 0) {
				TreeModel model = cmbCategory.Model;
				TreeIter iter;
				bool selected = false;
				// select category
				for (int i = 0; i < categories.Count; i++) {
					if ((categories.GetUntranslatedString(i)) == volume.Category) {
						model.IterNthChild(out iter, i);
						cmbCategory.SetActiveIter(iter);
						selected = true;
						break;
					}					 
				}
				
				// volume.Category is a user-specified custom category -> append it to the combobox
				if(!selected) {
					cmbCategory.AppendText(volume.Category);
					model.IterNthChild(out customCategory, categories.Count);
					cmbCategory.SetActiveIter(customCategory);
				}
			}
			
			txtTitle.Text				= volume.Title;
			tvDescription.Buffer.Text	= volume.Description;
			txtKeywords.Text			= volume.Keywords; 
			txtLoanedTo.Text			= volume.LoanedTo;
			
			if (volume.LoanedDate != DateTime.MinValue)
				dcLoanedDate.Date		= volume.LoanedDate;
			else
				dcLoanedDate.Clear();
			
			if (volume.ReturnDate != DateTime.MinValue)
				dcReturnDate.Date		= volume.ReturnDate;
			else
				dcReturnDate.Clear();
			
			//
			// info labels
			//
			UpdateInfoLabels(volume.IsHashed, volume.Added);
		}
		
		private void UpdateInfoLabels(bool isHashed, DateTime added) {
			lblVolumeType.LabelProp = volumeType;
			lblHashed.LabelProp		= isHashed ? S._("Yes") : S._("No");
			lblAdded.LabelProp		= added.ToShortDateString();		
		}
		
		protected override void AddInfoLabels(List<InfoLabel> infoLabels) {
			lblVolumeType = WindowBase.CreateLabel();
			lblHashed = WindowBase.CreateLabel();
			lblAdded = WindowBase.CreateLabel();

			infoLabels.AddRange( new InfoLabel[] { 
				new InfoLabel(S._("Volume type:"), lblVolumeType),
				new InfoLabel(S._("Hashed:"), lblHashed),
				new InfoLabel(S._("Added:"), lblAdded)
			} );
		}
	}
	
	// gui initialization
	public abstract partial class VolumeEditor : ObjectEditor<Volume>
	{
		private Entry		txtArchiveNo;
		private ComboBox	cmbCategory;
		private Entry		txtTitle;
		private TextView	tvDescription;
		private Entry		txtKeywords;
		private Entry		txtLoanedTo;
		private Widgets.DateChooser dcLoanedDate;
		private Widgets.DateChooser dcReturnDate;
		
		protected override void CreateWidgetTbl(out Table tbl) {
			tbl = WindowBase.CreateTable(8, 2);

			// labels
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Archive No.:")),			0, 0);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Category:")),				0, 1);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Title:")),				0, 2);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Description:"), false,	0F, 0F),   0, 3);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Keywords:")),				0, 4);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Loaned to:")),			0, 5);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Loaned date:")),			0, 6);
			WindowBase.TblAttach(tbl, WindowBase.CreateLabel(S._("Return date:")),			0, 7);
			
			// widgets
			txtArchiveNo	= new Entry(Volume.MAX_ARCHIVE_NO_LENGTH);
			cmbCategory		= ComboBox.NewText();			 
			txtTitle		= new Entry(Volume.MAX_TITLE_LENGTH);
			ScrolledWindow swDescription = WindowBase.CreateScrolledTextView(out tvDescription, Volume.MAX_DESCRIPTION_LENGTH);
			txtKeywords		= new Entry(Volume.MAX_KEYWORDS_LENGTH);
			txtLoanedTo		= new Entry(Volume.MAX_LOANED_TO_LENGTH);
			dcLoanedDate	= new Widgets.DateChooser();
			dcReturnDate	= new Widgets.DateChooser();
			
			AttachOptions xAttachOpts = AttachOptions.Expand | AttachOptions.Fill | AttachOptions.Shrink;
			AttachOptions yAttachOpts = AttachOptions.Fill;
			
			WindowBase.TblAttach(tbl, txtArchiveNo,		1, 0, xAttachOpts, yAttachOpts);
			WindowBase.TblAttach(tbl, cmbCategory,		1, 1, xAttachOpts, yAttachOpts);
			WindowBase.TblAttach(tbl, txtTitle,			1, 2, xAttachOpts, yAttachOpts);
			WindowBase.TblAttach(tbl, swDescription,	1, 3, xAttachOpts, yAttachOpts);
			WindowBase.TblAttach(tbl, txtKeywords,		1, 4, xAttachOpts, yAttachOpts);
			WindowBase.TblAttach(tbl, txtLoanedTo,		1, 5, xAttachOpts, yAttachOpts);
			WindowBase.TblAttach(tbl, dcLoanedDate,		1, 6, xAttachOpts, yAttachOpts);
			WindowBase.TblAttach(tbl, dcReturnDate,		1, 7, xAttachOpts, yAttachOpts);
			
			// fill combobox
			foreach (string translated in categories.TranslatedStrings)
				cmbCategory.AppendText(translated);

			// events 
			txtArchiveNo.Changed			+= ChangedEventHandler;
			cmbCategory.Changed				+= ChangedEventHandler;
			txtTitle.Changed				+= ChangedEventHandler;
			tvDescription.Buffer.Changed	+= ChangedEventHandler;
			txtKeywords.Changed				+= ChangedEventHandler;
			txtLoanedTo.Changed				+= ChangedEventHandler;
			dcLoanedDate.Changed			+= ChangedEventHandler;
			dcReturnDate.Changed			+= ChangedEventHandler;
		}
	}
}
