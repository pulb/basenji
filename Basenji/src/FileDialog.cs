// FileDialog.cs
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
using Basenji.Gui.Base;

namespace Basenji
{
	public static class FileDialog
	{
		public static ResponseType Show(FileChooserAction action, Window parent, string title, out string filename) {
			FileChooserDialog fc = null;
			switch(action) {
				case FileChooserAction.Open:
					fc = new FileChooserDialog(title, parent, FileChooserAction.Open, Stock.Cancel, ResponseType.Cancel, Stock.Open, ResponseType.Ok);
					break;
				case FileChooserAction.Save:
					fc = new FileChooserDialog(title, parent, FileChooserAction.Save, Stock.Cancel, ResponseType.Cancel, Stock.Save, ResponseType.Ok);
					break;
				case FileChooserAction.CreateFolder:
					throw new NotImplementedException();
					break;
				case FileChooserAction.SelectFolder:
					throw new NotImplementedException();
					break;
			}

			fc.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			
			fc.Modal				= true;
			fc.DestroyWithParent	= true;
			fc.SkipTaskbarHint		= true;
			fc.Icon					= WindowBase.DEFAULT_ICON;

			FileFilter ff;
			ff = new FileFilter();
			ff.Name = S._("VolumeDatabase files");
			ff.AddPattern("*.vdb");
			fc.AddFilter(ff);
			
			ff = new FileFilter();
			ff.Name = S._("All files");
			ff.AddPattern("*.*");
			fc.AddFilter(ff);
			
			ResponseType r = (ResponseType)fc.Run();
			filename = fc.Filename;
			fc.Destroy();
			return r;
		}
	}
}
