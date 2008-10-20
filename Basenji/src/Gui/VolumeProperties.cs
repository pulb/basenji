// VolumeProperties.cs
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
using Basenji.Gui.Base;
using Basenji.Gui.Widgets;
using VolumeDB;
using Gtk;

namespace Basenji.Gui
{	
	public partial class VolumeProperties : WindowBase
	{		 
		private Volume volume;
		public VolumeProperties(Volume volume) {
			this.volume = volume;
			BuildGui();
			volEdit.Load(volume);
		}
		
		private bool SaveAndClose() {
			try {
				volEdit.Save();
				this.Destroy();
			} catch (Widgets.VolumeEdit.ValidationException e) {
				MsgDialog.ShowError(this, S._("Invalid data"), string.Format(S._("\"{0}\" is {1}.\n\nExpected format: {2}\nPlease correct or remove the data you entered.") , e.WidgetName, e.Message, e.ExpectedFormat));
				return false;			 
			}
			return true;
		}
		
		public event SavedEventHandler Saved;
		
		protected virtual void OnSaved(Volume volume) {
			if (Saved != null)
				Saved(this, new Widgets.SavedEventArgs(volume));
		}
		
		private void OnVolEditSaved(object o, SavedEventArgs args) {
			OnSaved(args.Volume);		 
		}
		
		private void OnBtnCloseClicked(object sender, System.EventArgs e) {
			SaveAndClose();
		}
		
		private void OnDeleteEvent(object o, Gtk.DeleteEventArgs args) {
			bool cancel = !SaveAndClose();
			args.RetVal = cancel;
		}
	}
	
	// gui initialization
	public partial class VolumeProperties : WindowBase
	{
		private VolumeEdit volEdit;
		private Button btnClose;
		
		protected override void BuildGui () {
			base.BuildGui();
			
			// general window settings
			//this.BorderWidth = 2;
			this.DefaultWidth		= 580;
			this.DefaultHeight		= 400;
			this.Modal				= true;
			this.SkipTaskbarHint	= true;
			this.Title				= S._("Volume Properties");
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.Spacing = 24;
			
			// volEdit
			volEdit = VolumeEdit.CreateInstance(volume.GetVolumeType());
			vbOuter.PackStart(volEdit, true, true, 0);
			
			// button box
			HButtonBox bbox = new HButtonBox();
			bbox.LayoutStyle = ButtonBoxStyle.End;
			btnClose = CreateButton(Stock.Close, true, OnBtnCloseClicked);
			bbox.PackStart(btnClose, false, false, 0);			  
			vbOuter.PackStart(bbox, false, false, 0);
			
			this.Add(vbOuter);
			
			// events
			volEdit.Saved		+= OnVolEditSaved;
			this.DeleteEvent	+= OnDeleteEvent;
			
			this.ShowAll();
		}

	}
}
