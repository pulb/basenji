// AudioCdWin32.cs
//
// Copyright (c) 2008 Scott Peterson <lunchtimemama@gmail.com>
// Copyright (c) 2010 Patrick Ulbrich <zulu99@gmx.net>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

# if WIN32
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Platform.Common.IO
{
	// based on code of musicbrainz-sharp's DiscWin32
	internal static class AudioCdWin32
	{
		private delegate void MciCall(string result);
		
		public static int GetNumAudioTracks(string device) {
			
			int track_count = 0;
			
			if (device == null)
				throw new ArgumentNullException("device");
			
			if (device.Length == 0)
				throw new ArgumentException("Empty devicename");
			
			string device_string = string.Format("{0} type cdaudio", device);

            string alias = string.Format("platformio_cdio_{0}_{1}",
                Environment.TickCount, Thread.CurrentThread.ManagedThreadId);
			
//			MciClosure (
//                "sysinfo cdaudio quantity wait",
//                "Could not get the list of CD audio devices",
//                delegate(string result) {
//                    if (int.Parse(result.ToString ()) <= 0)
//                        throw new IOException ("No CD audio devices present.");
//            });
			
			MciClosure(
                string.Format("open {0} shareable alias {1} wait", device_string, alias),
                string.Format("Could not open device {0}", device),
                null);

            MciClosure(
                string.Format("status {0} number of tracks wait", alias),
                "Could not read number of tracks",
                delegate(string result) {
                    track_count = int.Parse(result);
                });
			
			MciClosure(
                string.Format("close {0} wait", alias),
                string.Format("Could not close device {0}", device),
                null);
			
			return track_count;
		}
		
		private static StringBuilder mci_result = new StringBuilder(128);
        private static StringBuilder mci_error = new StringBuilder(256);
        private static void MciClosure(string command, string failure_message, MciCall code) {
            int ret = mciSendString(command, mci_result, mci_result.Capacity, IntPtr.Zero);
            if (ret != 0) {
                mciGetErrorString(ret, mci_error, mci_error.Capacity);
                throw new IOException(string.Format("{0} : {1}", failure_message, mci_error.ToString()));
            } else if (code != null) {
				code(mci_result.ToString());
			}
        }
		
		[DllImport ("winmm")]
        private static extern Int32 mciSendString(
		                                   [MarshalAs(UnmanagedType.LPTStr)]
		                                   String command,
		                                   [MarshalAs(UnmanagedType.LPTStr)]
		                                   StringBuilder buffer,
		                                   Int32 bufferSize,
		                                   IntPtr hwndCallback
		                                   );
		
        [DllImport ("winmm")]
        private static extern Int32 mciGetErrorString(
		                                       Int32 errorCode,
		                                       [MarshalAs(UnmanagedType.LPTStr)]
		                                       StringBuilder errorText,
		                                       Int32 errorTextSize
		                                       );
        
	}
}
#endif