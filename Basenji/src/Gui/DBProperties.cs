// DBProperties.cs
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

// #pragma warning disable 649

using System;
using VolumeDB;
using Gtk;
using Basenji.Gui.Base;

namespace Basenji.Gui
{	
	public partial class DBProperties : WindowBase
	{
		private VolumeDatabase		db;
		private DatabaseProperties	props;
		
		public DBProperties(VolumeDatabase db) {
			BuildGui();
			
			this.db = db;
			props	= db.GetDBProperties();
			
			entName.Text				= props.Name;
			txtDescription.Buffer.Text	= props.Description;
			entCreated.Text				= props.Created.ToString();
		}
		
		private void Save() {
			props.Name = entName.Text;
			props.Description = txtDescription.Buffer.Text;

			db.UpdateDBProperties(props);
		}
		
		protected virtual void OnBtnCloseClicked(object sender, System.EventArgs e) {
			Save();
			this.Destroy(); // TODO : not neccessary?
		}
		
		protected virtual void OnDeleteEvent(object o, Gtk.DeleteEventArgs args) {
			Save();
			this.Destroy(); // TODO : not neccessary?
		}
	}
	
	// gui initialization
	public partial class DBProperties : WindowBase
	{
		private Entry		entName;
		private TextView	txtDescription;
		private Entry		entCreated;
		private Button		btnClose;
		
		protected override void BuildGui() {
			base.BuildGui();
			
			//general window settings
			SetDialogStyle();
			this.DefaultWidth		= 400;
			this.DefaultHeight		= 240;
			this.Title				= S._("Database Properties");
			this.Icon = this.RenderIcon(Basenji.Icons.Icon.Stock_Properties, IconSize.Menu);
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.BorderWidth = 12;
			vbOuter.Spacing = 18;
			
			// table
			Table tbl = CreateTable(3, 2);
			
			// labels
			TblAttach(tbl, CreateLabel(S._("Name:")), 0, 0);
			TblAttach(tbl, CreateLabel(S._("Description:"), false, 0.0f, 0.0f), 0, 1);
			TblAttach(tbl, CreateLabel(S._("Created:")), 0, 2);
			
			// name entry
			entName = new Entry(DatabaseProperties.MAX_NAME_LENGTH);
			TblAttach(tbl, entName, 1, 0);
			
			// description textview
			ScrolledWindow sw = WindowBase.CreateScrolledTextView(out txtDescription, DatabaseProperties.MAX_DESCRIPTION_LENGTH);
			TblAttach(tbl, sw, 1, 1, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill);
			
			// created entry
			entCreated = new Entry();
			entCreated.Sensitive = false;
			TblAttach(tbl, entCreated, 1, 2);
			
			vbOuter.PackStart(tbl, true, true, 0);
			
			// hbuttonbox
			HButtonBox bbox = new HButtonBox();
			//bbox.Spacing = 6;
			bbox.LayoutStyle = ButtonBoxStyle.End;
			
			btnClose = CreateButton(Stock.Close, true, OnBtnCloseClicked);
			
			bbox.PackStart(btnClose, false, false, 0);
			vbOuter.PackStart(bbox, false, false, 0);			 
			
			this.Add(vbOuter);			  
			
			this.DeleteEvent += OnDeleteEvent;
			
			ShowAll();
		}
	}
}
