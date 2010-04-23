// Led.cs
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
using Gdk;

namespace Basenji.Gui.Widgets
{
	public class Led : Base.BinBase
	{
		private Gtk.Image	image;
		private Gdk.Pixbuf	pixbufLedOn;
		private Gdk.Pixbuf	pixbufLedOff;
		private bool		state; // on / off
		
		public Led() : this(false) { }		  
		public Led(bool initialState) {
			BuildGui();
			
			this.pixbufLedOn   = Pixbuf.LoadFromResource("Basenji.images.LED_On.png");
			this.pixbufLedOff  = Pixbuf.LoadFromResource("Basenji.images.LED_Off.png");

			LedState = initialState;
		}
		
		public bool LedState {
			get { return state; }
			set {
				state = value;
				image.Pixbuf = state ? pixbufLedOn : pixbufLedOff;
			}
		}
		
		protected override void BuildGui() {
			this.image = new Gtk.Image();
			this.Add(image);
		}
	}
}
