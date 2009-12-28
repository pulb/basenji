// S.cs
// 
// Copyright (C) 2008, 2009 Patrick Ulbrich
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
using System.Globalization;
using System.Reflection;
using Platform.Common.Globalization;

namespace Basenji
{
	// shorthand to Catalog.GetString()
	internal static class S
	{
		private static Catalog c;
		
		static S() {
			string ns = String.Format("{0}.{1}",
			                          Assembly.GetCallingAssembly().GetName().Name,
			                          "po");
			
			c = Catalog.GetCatalogForCulture(CultureInfo.CurrentUICulture, ns, true);
		}
		
		public static string _(string msgid) {
			return c.GetString(msgid);
		}
	}
}
