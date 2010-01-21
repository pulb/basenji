// Debug.cs
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
using System.Reflection;

namespace Platform.Common.Diagnostics
{	
	public static class Debug
	{
		public static void WriteLine(string message) {
#if DEBUG
			Assembly asm = Assembly.GetCallingAssembly();
			WriteLine(asm, message, new object[] {});
#endif
		}
		
		public static void WriteLine(string message, params object[] args) {
#if DEBUG
			Assembly asm = Assembly.GetCallingAssembly();
			WriteLine(asm, message, args);
#endif
		}
		
		private static void WriteLine(Assembly asm, string message, params object[] args) {
			object[] args2;
			if (args.Length == 0) {
				message = message.Replace("{", "{{").Replace("}", "}}");
				args2 = args;
			} else {
				args2 = new object[args.Length];
				args.CopyTo(args2, 0);
				
				for (int i = 0; i < args.Length; i++) {
					string s;
					if ((s = args[i] as string) != null)
						args2[i] = s.Replace("{", "{{").Replace("}", "}}");
					else
						args2[i] = args[i];
				}
			}
			
			message = string.Format(message, args2);
			string appName = asm.GetName().Name;
			Console.WriteLine("[{0} DBG]: {1}", appName, message);
			//System.Diagnostics.Debug.WriteLine(message);
		}
	}
}
