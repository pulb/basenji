// WaitingDialog.cs
// 
// Copyright (C) 2012 Patrick Ulbrich
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
using System.Threading;
using Gtk;
using Gdk;
using Basenji.Gui.Base;

namespace Basenji.Gui
{
	public delegate bool WaitFunc<T>(out T value);
	
	public partial class WaitingDialog<T> : DialogBase
	{
		private WaitFunc<T> waitFunc;
		private string message;
		private volatile bool canceled;
		
		public WaitingDialog (WaitFunc<T> waitFunc, string message) {
			this.waitFunc = waitFunc;
			this.Value = default(T);
			this.message = message;
			this.canceled = false;
			
			BuildGui();
		}
		
		public new int Run() {
			BeginWaiting();
			return base.Run();
		}
		
		public T Value {
			get;
			private set;
		}
		
		private void BeginWaiting() {
			System.Action act = delegate {
				T tmp;
				while (!canceled && !waitFunc(out tmp))
					Thread.Sleep(1000);
				
				if (!canceled) {
					Application.Invoke(delegate {
						Value = tmp;
						Respond(ResponseType.Ok);
					});
				}
			};
			act.BeginInvoke(null, null);
		}
		
		private void OnBtnCancelClicked(object sender, System.EventArgs e) {
			// terminate waiting thread
			canceled = true;
			// return cancel state
			Respond(ResponseType.Cancel);
		}
	}
	
	// gui initialization
	public partial class WaitingDialog<T> : DialogBase
	{	
		Button btnCancel;
		
		protected override void BuildGui() {
			base.BuildGui();
			
			this.BorderWidth = 0 /* = 2 */; // TODO : somehow the dialog already has a 2 px border.. vbox? bug in gtk#?
			
			VBox vb = new VBox();
			vb.Spacing = 12;
			vb.BorderWidth = 12;
			
			vb.PackStart(WindowBase.CreateLabel(message, true), true, true, 0);
			
			btnCancel = WindowBase.CreateButton(Stock.Cancel, true, OnBtnCancelClicked);
			vb.PackStart(btnCancel, false, false, 0);
			
			VBox.PackStart(vb, true, true, 0);
			
			ShowAll();
		}
	}
}

