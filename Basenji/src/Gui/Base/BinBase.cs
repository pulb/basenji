// BinBase.cs
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
	public abstract class BinBase : Gtk.Bin
	{
		private Widget child;
		
		public BinBase() : base() {
			
			this.SizeRequested += delegate(object sender, SizeRequestedArgs args) {
				if (this.child != null)
					args.Requisition = this.child.SizeRequest();
			};

			this.SizeAllocated += delegate(object sender, SizeAllocatedArgs args) {
				if (this.child != null)
					this.child.Allocation = args.Allocation;		 
			};
			
			this.Added += delegate(object sender, AddedArgs args) {
				this.child = args.Widget;
			};
		}
		
		protected abstract void BuildGui();
		
		protected Gdk.Pixbuf RenderIcon(Basenji.Icons.Icon icon, Gtk.IconSize size) {
			return icon.Render(this, size);
		}
	}
}
