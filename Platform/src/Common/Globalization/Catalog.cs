// Catalog.cs
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

// TODO : write an extension method 
// public static string _(this object o, string msgid)
// when mono 2.0 is included in major distros

using System;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace Platform.Common.Globalization
{
	// shorthand to Catalog.GetString()
	public static class S
	{
		public static string _(string msgid) {
			return Catalog.GetString(msgid);
		}
	}
		
	public static class Catalog
	{
		private static ResourceManager rm;
		private static Assembly asm;
		
		static Catalog() {
			asm = Assembly.GetCallingAssembly();
			
			CultureInfo c = CultureInfo.CurrentUICulture;				
			// try to set sub-language, e.g. "de_CH"
			if (!SetCulture(c)) {
				if (c.Parent != null) {
					// try to set parent language, e.g. "de"
					SetCulture(c.Parent);
				}
			}
		}
		
		public static bool SetCulture(CultureInfo c) {
			rm = null;
			
			string cultureName = c.Name.Replace('-', '_'); // fix minus in sub-languages
			if (asm.GetManifestResourceInfo(cultureName + ".resources") != null) {
				rm = new ResourceManager(cultureName, asm);
				return true;
			}
			return false;
		}
		
		public static string GetString(string msgid) {
			if (rm == null)
				return msgid;
				
			string translated = rm.GetString(msgid);
			return translated ?? msgid;
		}
	}
}
