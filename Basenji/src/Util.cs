// Util.cs
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
using System.Runtime.InteropServices;
using System.Reflection;
using Platform.Common.Globalization;
using Platform.Common.Diagnostics;

namespace Basenji
{
	public static class Util
	{	
		// delegates for async method calls
		public delegate void Callback();
		public delegate void Callback<A>(A arg);
		public delegate R Callback<R, A>(A arg);
		
		public static string GetSizeStr(long size) {
			if (size < 1024)
				return string.Format(S._("{0} Bytes"), size);

			string[] units = { S._("Bytes"), S._("KB"), S._("MB"), S._("GB"), S._("TB") };
			double dblSize = size; // TODO : use float?
			int n = 0;

			while(dblSize >= 1024.0) {
				dblSize /= 1024.0;
				n++;
			}
			return string.Format("{0:N2} {1}", dblSize, units[n]);
		}
		
		public static string GetVolumeDBVersion() {
			Assembly asm = Assembly.GetAssembly(typeof(VolumeDB.VolumeDatabase));
			return asm.GetName().Version.ToString();		
		}
		
		public static void SetProcName(string name) {
			const int PR_SET_NAME = 0x0F;
			
			IntPtr arg = Marshal.StringToHGlobalAnsi(name);
			try {
				if (prctl(PR_SET_NAME, (ulong)arg.ToInt64(), 0, 0, 0) != 0)
					Debug.WriteLine("prctl() failed");
			} catch (EntryPointNotFoundException) {
				IntPtr fmt = Marshal.StringToHGlobalAnsi("%s");
				setproctitle(fmt, arg);
				Marshal.FreeHGlobal(fmt);
			}
			Marshal.FreeHGlobal(arg);
		}
		
		// linux
		[DllImport("libc")]
		private static extern int prctl (int option, ulong arg2, ulong arg3, ulong arg4, ulong arg5);
		
		// bsd
		[DllImport("libc")]
		private static extern void setproctitle(IntPtr format, IntPtr arg);
	}
}
