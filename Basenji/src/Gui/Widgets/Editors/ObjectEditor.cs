// ObjectEditor.cs
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
using System.Collections.Generic;
using Gtk;
using Basenji.Gui.Base;
using Platform.Common.Diagnostics;

namespace Basenji.Gui.Widgets.Editors
{
	public abstract partial class ObjectEditor<T> : BinBase
	{
		private List<InfoLabel> infoLabels;
		
		protected ObjectEditor()
		{
			this.DataChanged = false;
			this.Object = default(T);
			this.infoLabels = new List<InfoLabel>();
			
			AddInfoLabels(infoLabels);
			BuildGui();
		}
		
		public bool DataChanged {
			get;
			private set;
		}
		
		protected T Object {
			get;
			private set;
		}
		
		public new bool Sensitive {
			get { 
				// just test the first widget
				return tblWidgets.Children[0].Sensitive;
			}
			set {
				tblWidgets.Foreach(w => {
					if (!(w is Label))
						w.Sensitive = value;
				});
			}
		}
		
		public void Load(T obj) {
			LoadFromObject(obj); // may throw a ArgumentException
			this.Object = obj;
			// changed flag was set to true since the input fields were loaded
			// but we will keep track of changes made by the user only.
			DataChanged = false;	
		}
		
		public void Save() {
			if (this.Object == null)
				throw new InvalidOperationException("No object loaded");

			if (!DataChanged) {
#if DEBUG
				Debug.WriteLine("not saving, nothing changed.");
#endif
				return;
			} else {
#if DEBUG
				Debug.WriteLine("saving form.");
#endif
			}
			
			ValidateForm(); // may throw a ValidationException
			SaveToObject(this.Object);
			OnSaved();
		}
		
		protected abstract void LoadFromObject(T obj);
		protected abstract void SaveToObject(T obj);
	    protected abstract void ValidateForm();
		protected abstract void AddInfoLabels(List<InfoLabel> infoLabels);
		
		protected void ChangedEventHandler(object sender, EventArgs args) {
			DataChanged = true;
		}
		
		public event EventHandler<SavedEventArgs<T>> Saved;
		
		protected virtual void OnSaved() {
			if (Saved != null)
				Saved(this, new SavedEventArgs<T>(this.Object));
		}
		
		protected class InfoLabel
		{
			public InfoLabel(string caption, Label label) {
				this.Caption = caption;
				this.Label = label;
			}
			
			public string Caption {
				get;
				set;
			}
			
			public Label Label {
				get;
				set;
			}
		}
	}
	
	// gui initialization
	public abstract partial class ObjectEditor<T> : BinBase
	{
		private Table tblWidgets;
		
		protected override void BuildGui() {
			// hbox			   
			HBox hbox = new HBox();
			hbox.Spacing = 18;
			
			CreateWidgetTbl(out tblWidgets);
			
			hbox.PackStart(tblWidgets, true, true, 0);
			hbox.PackStart(new VSeparator(), false, false, 0);
			hbox.PackStart(CreateInfoTbl(), false, false, 0);
			
			this.Add(hbox);
		}
		
		protected abstract void CreateWidgetTbl(out Table tbl);
		
		private Table CreateInfoTbl() {
			Table tbl = WindowBase.CreateTable(infoLabels.Count, 2);
			
			for (int i = 0; i < infoLabels.Count; i++) {
				string caption = string.Format("<i>{0}</i>", Util.Escape(infoLabels[i].Caption));
				
				WindowBase.TblAttach(tbl, WindowBase.CreateLabel(caption, true), 0, i);								   
				WindowBase.TblAttach(tbl, infoLabels[i].Label, 1, i);
				infoLabels[i].Label.LabelProp = "-";
			}

			return tbl;
		}
	}
}
