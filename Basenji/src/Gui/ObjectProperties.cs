// ObjectProperties.cs
// 
// Copyright (C) 2008 - 2012 Patrick Ulbrich
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
using Basenji.Gui.Base;
using Basenji.Gui.Widgets.Editors;
using VolumeDB;
using Gtk;

namespace Basenji.Gui
{	
	public abstract partial class ObjectProperties<T> : WindowBase
	{		 
		private string title;
		private int width, height;
		
		protected ObjectProperties(T obj,
		                           string title,
		                           ObjectEditor<T> editor,
		                           int width,
		                           int height) {
			this.title = title;
			this.objEditor = editor;
			
			if (width > 0)
				this.width = width;
			if (height > 0)
				this.height = height;
			
			BuildGui();
			objEditor.Load(obj);
		}
		
		private bool SaveAndClose() {
			try {
				objEditor.Save();
				this.Destroy();
			} catch (ValidationException e) {
				MsgDialog.ShowError(this, S._("Invalid data"), string.Format(S._("\"{0}\" is {1}.\n\nExpected format: {2}\nPlease correct or remove the data you entered.") , e.WidgetName, e.Message, e.ExpectedFormat));
				return false;			 
			}
			return true;
		}
		
		public event EventHandler<SavedEventArgs<T>> Saved;
		
		protected virtual void OnSaved(T obj) {
			if (Saved != null)
				Saved(this, new SavedEventArgs<T>(obj));
		}
		
		private void OnObjEditorSaved(object o, SavedEventArgs<T> args) {
			OnSaved(args.SavedObject);
		}
		
		private void OnBtnCloseClicked(object sender, System.EventArgs e) {
			SaveAndClose();
		}
		
		private void OnDeleteEvent(object o, Gtk.DeleteEventArgs args) {
			bool cancel = !SaveAndClose();
			args.RetVal = cancel;
		}
		
		[GLib.ConnectBefore()]
		private void OnWindowKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {
			if (args.Event.Key == Gdk.Key.Escape)
				SaveAndClose();
		}
	}
	
	// gui initialization
	public abstract partial class ObjectProperties<T> : WindowBase
	{
		private ObjectEditor<T> objEditor;
		private Button btnClose;
		
		protected override void BuildGui () {
			base.BuildGui();
			
			// general window settings
			SetModal();
			this.DefaultWidth		= this.width;
			this.DefaultHeight		= this.height;
			this.Title				= this.title;
			
			// vbOuter
			VBox vbOuter = new VBox();
			vbOuter.BorderWidth = 12;
			vbOuter.Spacing = 18;
			
			// objEditor
			vbOuter.PackStart(objEditor, true, true, 0);
			
			// button box
			HButtonBox bbox = new HButtonBox();
			bbox.LayoutStyle = ButtonBoxStyle.End;
			btnClose = CreateButton(Stock.Close, true, OnBtnCloseClicked);
			bbox.PackStart(btnClose, false, false, 0);			  
			vbOuter.PackStart(bbox, false, false, 0);
			
			this.Add(vbOuter);
			
			// events
			objEditor.Saved		+= OnObjEditorSaved;
			this.KeyPressEvent	+= OnWindowKeyPressEvent;
			this.DeleteEvent	+= OnDeleteEvent;
			
			this.ShowAll();
		}

	}
}
