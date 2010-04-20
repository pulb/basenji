// DiscWin32NT.cs
//
// Copyright (c) 2008 Scott Peterson <lunchtimemama@gmail.com>
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

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MusicBrainz
{
    internal sealed class DiscWin32NT : LocalDisc
    {
        [DllImport ("kernel32.dll")]
        static extern bool DeviceIoControl (SafeFileHandle hDevice, uint dwIoControlCode,
                                            IntPtr lpInBuffer, uint nInBufferSize,
                                            IntPtr lpOutBuffer, uint nOutBufferSize,
                                            out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport ("kernel32.dll")]
        static extern SafeFileHandle CreateFile (string lpFileName, uint dwDesiredAccess,
                                                 uint dwShareMode, IntPtr SecurityAttributes,
                                                 uint dwCreationDisposition, uint dwFlagsAndAttributes,
                                                 IntPtr hTemplateFile);

        const uint GENERIC_READ = 0x80000000;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;

        const uint OPEN_EXISTING = 3;

        const uint IOCTL_CDROM_READ_TOC = 0x24000;
        const uint IOCTL_CDROM_GET_LAST_SESSION = 0x24038;

        const int XA_INTERVAL = ((60 + 90 + 2) * 75);

        [StructLayout (LayoutKind.Sequential)]
        struct TRACK_DATA
        {
            public byte Reserved;
            public byte ControlAndAdr;
            public byte TrackNumber;
            public byte Reserved1;
            public byte Address0;
            public byte Address1;
            public byte Address2;
            public byte Address3;

            public int GetSectors ()
            {
                return Address1 * 4500 + Address2 * 75 + Address3;
            }
        }

        const int MAX_TRACK_NUMBER = 100;

        [StructLayout (LayoutKind.Sequential)]
        class CDROM_TOC
        {
            public ushort Length;
            public byte FirstTrack;
            public byte LastTrack;
            public TRACK_DATA_Array TrackData = new TRACK_DATA_Array();
            
            public CDROM_TOC ()
            {
                Length = (ushort)Marshal.SizeOf (this);
            }
        }

        [StructLayout (LayoutKind.Sequential)]
        class TRACK_DATA_Array
        {
            [MarshalAs (UnmanagedType.ByValArray, SizeConst = MAX_TRACK_NUMBER * 8)]
            byte[] TrackData = new byte[MAX_TRACK_NUMBER * Marshal.SizeOf (typeof (TRACK_DATA))];
            public unsafe TRACK_DATA this[int index] {
                get {
                    if (index < 0 || index >= MAX_TRACK_NUMBER)
                        throw new IndexOutOfRangeException ();

                    fixed (byte* b = TrackData) {
                        TRACK_DATA* td = (TRACK_DATA*)b;
                        td += index;
                        return *td;
                    }
                }
            }
        }

        [StructLayout (LayoutKind.Sequential)]
        class CDROM_TOC_SESSION_DATA
        {
            public ushort Length;
            public byte FirstCompleteSession;
            public byte LastCompleteSession;
            public TRACK_DATA TrackData;

            public CDROM_TOC_SESSION_DATA ()
            {
                Length = (ushort)Marshal.SizeOf (this);
            }
        }

        internal DiscWin32NT (string device)
        {
            int colon = device.IndexOf (':');
            if (colon == -1) {
                throw new ArgumentException ("The device path is not valid.", "device");
            }

            string filename = string.Concat (@"\\.\", device.Substring (0, colon + 1));

            CDROM_TOC_SESSION_DATA session = new CDROM_TOC_SESSION_DATA ();
            CDROM_TOC toc = new CDROM_TOC();

            using (SafeFileHandle file = CreateFile (filename, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero)) {

                if (file.IsInvalid) {
                    throw new LocalDiscException ("Could not open the CD device.");
                }

                uint returned;

                GCHandle session_handle = GCHandle.Alloc (session, GCHandleType.Pinned);
                try {
                    bool result = DeviceIoControl (file, IOCTL_CDROM_GET_LAST_SESSION, IntPtr.Zero, 0,
                        session_handle.AddrOfPinnedObject (), (uint)Marshal.SizeOf (session), out returned, IntPtr.Zero);
                    if (!result) {
                        throw new LocalDiscException ("There was a problem while reading the disc TOC.");
                    }
                } finally {
                    session_handle.Free ();
                }

                IntPtr toc_ptr = Marshal.AllocHGlobal (Marshal.SizeOf (toc));
                Marshal.StructureToPtr (toc, toc_ptr, false);
                try {
                    bool result = DeviceIoControl (file, IOCTL_CDROM_READ_TOC, IntPtr.Zero, 0,
                        toc_ptr, (uint)Marshal.SizeOf (toc), out returned, IntPtr.Zero);
                    if (!result) {
                        throw new LocalDiscException ("There was a problem while reading the disc TOC.");
                    }
                    Marshal.PtrToStructure (toc_ptr, toc);
                } finally {
                    Marshal.FreeHGlobal (toc_ptr);
                }
            }

            first_track = toc.FirstTrack;

            if (session.FirstCompleteSession != session.LastCompleteSession) {
                last_track = (byte)(session.TrackData.TrackNumber - 1);
                track_offsets[0] = toc.TrackData[last_track].GetSectors () - XA_INTERVAL;
            } else {
                last_track = toc.LastTrack;
                track_offsets[0] = toc.TrackData[last_track].GetSectors ();
            }

            for (int i = first_track; i <= last_track; i++) {
                track_offsets[i] = toc.TrackData[i - 1].GetSectors ();
            }

            Init ();
        }
    }
}