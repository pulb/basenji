// DialogBase.cs
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
using Gtk;

namespace Basenji.Gui.Base
{
	public abstract class DialogBase : Gtk.Dialog
	{		 
		public DialogBase() : base() {}

		protected virtual void BuildGui() {
			if ((WindowBase.MainWindow != null) && (this != WindowBase.MainWindow))
				this.TransientFor = WindowBase.MainWindow;
			
			this.BorderWidth		= 0;
			this.WindowPosition		= WindowPosition.CenterOnParent;
			this.Modal				= true;
			this.SkipTaskbarHint	= true;
			this.DestroyWithParent	= true;			
			this.HasSeparator		= false;
			this.Icon				= WindowBase.DEFAULT_ICON;
				
			// outer vbox			
			VBox vbOuter = this.VBox;
			vbOuter.Spacing = 0;
		}
		
		/*
		protected void ShowAll() {
			if (this.Child != null)
				this.Child.ShowAll();
			
			this.Show();		
		}*/
		
		protected void InitActionArea(out Button btnOk, out Button btnCancel, EventHandler btnOkClicked, EventHandler btnCancelClicked) {
			// buttonbox
			HButtonBox bbox = this.ActionArea;
			bbox.Spacing = 6;		// TODO : currently ignored? bug in gtk#?
			bbox.BorderWidth = 12;	// TODO : currently ignored? bug in gtk#?
			bbox.LayoutStyle = ButtonBoxStyle.End;
			
			// cancel button
			btnCancel = new Button();
			btnCancel.CanDefault = true;
			//btnCancel.CanFocus = true;
			btnCancel.UseUnderline = true;
			btnCancel.UseStock = true;
			btnCancel.Label = Stock.Cancel;
			if (btnCancelClicked != null)			 
				btnCancel.Clicked += btnCancelClicked;
			
			this.AddActionWidget(btnCancel, -6);
			
			// ok button
			btnOk = new Button();
			btnOk.CanDefault = true;
			//btnCancel.CanFocus = true;
			btnOk.UseUnderline = true;
			btnOk.UseStock = true;
			btnOk.Label = Stock.Ok;
			if (btnOkClicked != null)
				btnOk.Clicked += btnOkClicked;
			
			this.AddActionWidget(btnOk, -5);
		}
		
		protected Gdk.Pixbuf RenderIcon(Basenji.Icons.Icon icon, Gtk.IconSize size) {
			return icon.Render(this, size);
		}
	}
}
