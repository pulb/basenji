// ViewBase.cs
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
//using System.Threading;
//using System.Runtime.Remoting.Messaging;
using Gtk;
using VolumeDB;

namespace Basenji.Gui.Base
{
	public abstract class ViewBase : TreeView
	{
		public ViewBase() {
			HeadersVisible = false;
		}
		
		// bug (?) workaround:
		// sometimes, if a new store is assigned to the model,
		// the vertical scrollposition of a parent scrolledwindow
		// does not seem to be reset to 0.
		public new TreeModel Model {
			get {
				return base.Model;
			}
			set {				
				base.Model = value;
				Vadjustment.Value = 0;
			}
		}
//		  protected abstract void FillingThread(VolumeDatabase database);
//		  
//		  public IAsyncResult BeginFill(VolumeDatabase database, AsyncCallback callback, object state) {
//			  if (database == null)
//				  throw new ArgumentNullException("database");
//			  
//			  // anonymous fill method executed on a new thread
//			  Util.AsyncMethodInvoker ami = delegate {
//				  FillingThread(database);
//			  };
//			  
//			  return ami.BeginInvoke(callback, state);
//		  }
//		  
//		  public void EndFill(IAsyncResult asyncResult) {
//			  if (asyncResult == null)
//				  throw new ArgumentNullException("asyncResult");
//			  
//			  Util.AsyncMethodInvoker d = (Util.AsyncMethodInvoker) ((AsyncResult)asyncResult).AsyncDelegate;
//			  d.EndInvoke(asyncResult);
//		  }
		
		public bool GetSelectedIter(out TreeIter iter) {
			TreeModel model;
			
			if (Selection.GetSelected(out model, out iter))
				return true;

			return false;
		}
		
		protected void Remove(TreeIter iter) {
			// select prev/next row				   
			ListStore store = (ListStore)Model; 
			TreePath p = store.GetPath(iter);
			if (!p.Prev())
				p.Next();
			Selection.SelectPath(p);
			// remove selected row
			store.Remove(ref iter);		   
		}
		
		protected void ResetView() {
			foreach (TreeViewColumn c in Columns)
				RemoveColumn(c);		
		}
		
		protected Gdk.Pixbuf RenderIcon(Basenji.Icons.Icon icon, Gtk.IconSize size) {
			return icon.Render(this, size);
		}
	}
}
