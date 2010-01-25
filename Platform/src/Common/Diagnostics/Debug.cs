// Debug.cs
// 
// Copyright (C) 2008 - 2010 Patrick Ulbrich
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
		public static void Assert(bool condition, string msg) {
			if (!condition)
				throw new AssertionFailedException("Assertion failed: " + msg);
		}
		
		public static void WriteLine(string message) {
			Assembly asm = Assembly.GetCallingAssembly();
			WriteLine(asm, message, new object[] {});
		}
		
		public static void WriteLine(string message, params object[] args) {
			Assembly asm = Assembly.GetCallingAssembly();
			WriteLine(asm, message, args);
		}
		
		private static void WriteLine(Assembly asm, string message, params object[] args) {
			if (args.Length > 0)
				message = string.Format(message, args);
			
			string appName = asm.GetName().Name;
			Console.WriteLine("[{0} DBG]: {1}", appName, message);
			//System.Diagnostics.Debug.WriteLine(message);
		}
	}
}
