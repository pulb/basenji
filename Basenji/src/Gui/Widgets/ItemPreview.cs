// ItemPreview.cs
// 
// Copyright (C) 2009 Patrick Ulbrich
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
using System.IO;
using Gdk;
using Gtk;
using Cairo;
using Basenji.Icons;
using VolumeDB;

namespace Basenji.Gui.Widgets
{
	public class ItemPreview : DrawingArea
	{
		// corner radius
		const double RADIUS = 6;
		// min len of a pixmap dimension required for rounded corners
		const int MIN_LEN_FOR_CORNERS = (int)(RADIUS * 6);
		
		private const IconSize	ICON_SIZE = IconSize.Dialog;
		private ItemIcons		itemIcons;
		private Pixbuf			pb;
		private bool			isIcon;
		
		public ItemPreview() {
			this.RoundedCorners		= true;
			this.EnableGenericIcons = true;
			
			this.itemIcons			= new ItemIcons(this);
			this.pb					= null;
			this.isIcon				= false;
		}
		
		public void Preview(VolumeItem item, VolumeDatabase db) {
			if (item == null)
				throw new ArgumentNullException("item");
			if (db == null)
				throw new ArgumentNullException("db");
			
			// free old pixbuf (but not a _cached_ icon!)
			if (!isIcon && (this.pb != null)) {
				this.pb.Dispose();
				this.pb = null;
			}
			
			string thumbName = System.IO.Path.Combine(
				DbData.GetVolumeDataThumbsPath(db, item.VolumeID), 
				string.Format("{0}.png", item.ItemID));
			
			if (File.Exists(thumbName)) {
				this.pb = new Gdk.Pixbuf(thumbName);
				this.isIcon = false;
			} else {
				if (EnableGenericIcons)
					this.pb = itemIcons.GetIconForItem(item, ICON_SIZE);
				else
					this.pb = null;
				this.isIcon = true;
			}
			
			QueueDraw();
		}
		
		public void Clear() {
			if (!isIcon && (this.pb != null)) {
				this.pb.Dispose();
				this.pb = null;
			}
			
			QueueDraw();
		}
		
		public bool RoundedCorners {
			get;
			set;
		}
		
		public bool EnableGenericIcons {
			get;
			set;
		}
		
		public bool IsThumbnailPreview {
			get {return !this.isIcon; }
		}
		
		protected override bool OnExposeEvent (EventExpose args) {
			if (pb == null)
				return true;
			
			double sf = 1.0; // pixbuf scale factor
			
			// if any image dimension > widget area => calc downscale factor
			if ((pb.Width > args.Area.Width) || (pb.Height > args.Area.Height)) {
				double sfWidth = (double)args.Area.Width / pb.Width;
				double sfHeight = (double)args.Area.Height / pb.Height;
				sf = Math.Min(sfWidth, sfHeight);
			} 
			
			// adjust selection area size to that of the pixbuf
			int width = (int)(pb.Width * sf);
			int height = (int)(pb.Height * sf);
			
			// center in the widget area
			double x = Math.Floor((args.Area.Width / 2.0) - (width / 2.0));
			double y = Math.Floor((args.Area.Height / 2.0) - (height / 2.0));
			
			using (Context cr = Gdk.CairoHelper.Create(args.Window)) {			
				cr.MoveTo(x, y);
				
				if (RoundedCorners && !isIcon && 
				    (width > MIN_LEN_FOR_CORNERS) && (height > MIN_LEN_FOR_CORNERS)) {
					
					cr.Arc(x + width - RADIUS, y + RADIUS, RADIUS, Math.PI * 1.5, Math.PI * 2);
					cr.Arc(x + width - RADIUS, y + height - RADIUS, RADIUS, 0, Math.PI * .5);
					cr.Arc(x + RADIUS, y + height - RADIUS, RADIUS, Math.PI * .5, Math.PI);
					cr.Arc(x + RADIUS, y + RADIUS, RADIUS, Math.PI, Math.PI * 1.5);
					
					cr.Clip();
					cr.NewPath();
				}
				
				// set pixbuf source downscale
				if (sf < 1.0)
					cr.Scale(sf, sf);
				
				// set pixbuf source
				CairoHelper.SetSourcePixbuf(cr, pb, Math.Floor(x / sf), Math.Floor(y / sf));				
				// paint pixbuf source
				cr.Paint();
			}
			
			return true;
		}
	}
}
