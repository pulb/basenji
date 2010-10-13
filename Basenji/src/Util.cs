// Util.cs
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
using System.Runtime.InteropServices;
using System.Reflection;
using Platform.Common.Diagnostics;

namespace Basenji
{
	public static class Util
	{		
		public static string GetSizeStr(long size) {
			if (size < 1024)
				return string.Format(S._("{0} Bytes"), size);

			string[] units = { S._("Bytes"), S._("KB"), S._("MB"), S._("GB"), S._("TB") };
			double dblSize = size;
			int n = 0;

			while (dblSize > 1023.995 /* dblSize >= 1024.0 */) {
				dblSize /= 1024.0;
				n++;
			}
			
			// rounds up starting at .995
			return string.Format("{0:N2} {1}", dblSize, units[n]);
		}

		public static string Escape(string str) {
			// Note : String.Replace() is better suited than
			// Stringbuilder.Replace() in this case, 
			// as String.Replace() returns the same string instance 
			// if oldValue can't be found (which applies in the majority of cases).
			str = str.Replace("&", "&amp;");
			str = str.Replace("<", "&lt;");
			str = str.Replace(">", "&gt;");
			str = str.Replace("\"", "&quot;");
			str = str.Replace("'", "&apos;");			
			
			return str;
		}
		
		// Copied from Banshee.Hyena.Gui.GtkUtilities
		// Copyright (C) 2007 Aaron Bockover <abockover@novell.com>
		public static Gdk.Color ColorBlend(Gdk.Color a, Gdk.Color b) {
			// at some point, might be nice to allow any blend?
			double blend = 0.5;
			
			if (blend < 0.0 || blend > 1.0) {
				throw new ApplicationException ("blend < 0.0 || blend > 1.0");
			}
			
			double blendRatio = 1.0 - blend;
			
			int aR = a.Red >> 8;
			int aG = a.Green >> 8;
			int aB = a.Blue >> 8;
			
			int bR = b.Red >> 8;
			int bG = b.Green >> 8;
			int bB = b.Blue >> 8;
			
			double mR = aR + bR;
			double mG = aG + bG;
			double mB = aB + bB;
			
			double blR = mR * blendRatio;
			double blG = mG * blendRatio;
			double blB = mB * blendRatio;
			
			Gdk.Color color = new Gdk.Color ((byte)blR, (byte)blG, (byte)blB);
			Gdk.Colormap.System.AllocColor (ref color, true, true);
			
			return color;
        }
		
		public static string FormatExceptionMsg(Exception e) {
			string msg = e.Message;
			int breakPos = msg.IndexOfAny(Environment.NewLine.ToCharArray());
			if (breakPos > -1)
				msg = msg.Substring(0, breakPos);
			return msg + ".";
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
