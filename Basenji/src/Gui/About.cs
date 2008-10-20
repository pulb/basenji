// About.cs
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
using System.Text;
using Gtk;
using Basenji.Gui.Base;

namespace Basenji.Gui
{
	public partial class About : WindowBase
	{
		public About() {
			BuildGui();
		}
		
		protected virtual void OnBtnCloseClicked(object sender, System.EventArgs e) {
			this.Destroy(); // TODO : not neccessary?
		}
	}
	
	// gui initialization
	public partial class About : WindowBase
	{
		private Button btnClose;
		
		protected override void BuildGui() {
			base.BuildGui();
			
			//general window settings
			this.Modal				= true;
			this.SkipTaskbarHint	= true;
			this.Resizable			= false;
			this.Title				= string.Format(S._("About {0}"), App.Name);
			this.Icon				= this.RenderIcon(Basenji.Icons.Icon.Stock_About, IconSize.Menu);
			
			// vbOuter			  
			VBox vbOuter = new VBox();
			vbOuter.Spacing = 24;
			
			Image img = new Image(new Gdk.Pixbuf("data/basenji.svg", 200, 200)); // TODO : fix path (e.g. /usr/share/icons)
			vbOuter.PackStart(img, true, true, 0);
			
			Label text = CreateLabel(GetText(), true);
			vbOuter.PackStart(text, false, false, 0);
			
			// hbuttonbox
			HButtonBox bbox = new HButtonBox();
			//bbox.Spacing = 6;
			bbox.LayoutStyle = ButtonBoxStyle.End;
			
			btnClose = CreateButton(Stock.Close, true, OnBtnCloseClicked);
			
			bbox.PackStart(btnClose, false, false, 0);
			vbOuter.PackStart(bbox, false, false, 0);	
			
			this.Add(vbOuter);
			ShowAll();
		}
		
		private string GetText() {
			StringBuilder sb = new StringBuilder();
		   
			sb.Append("<b><span size=\"xx-large\">").Append(App.Name).Append(" ").Append(App.Version).Append("</span></b>\n\n");
			sb.Append(S._("Copyright (c) ")).Append(App.Copyright).Append("\n");
			
			sb.AppendFormat(S._("Using VolumeDB v{0}."), Util.GetVolumeDBVersion());
			
			return sb.ToString();
		}
	}
	
}
