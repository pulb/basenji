// DiscLinux.cs
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

namespace MusicBrainz
{
    internal sealed class DiscLinux : LocalDisc
    {
        const int O_RDONLY = 0x0;
        const int O_NONBLOCK = 0x4000;
        const int CDROMREADTOCHDR = 0x5305;
        const int CDROMREADTOCENTRY = 0x5306;
        const int CDROMMULTISESSION = 0x5310;
        const int CDROM_LBA = 0x01;
        const int CDROM_LEADOUT = 0xAA;
        const int CD_FRAMES = 75;
        const int XA_INTERVAL = ((60 + 90 + 2) * CD_FRAMES);

        [DllImport ("libc", CharSet = CharSet.Auto)]
        static extern int open (string path, int flags);

        [DllImport ("libc")]
        static extern int close (int fd);

        [DllImport ("libc", EntryPoint = "ioctl")]
        static extern int read_toc_header (int fd, int request, ref cdrom_tochdr header);
        static int read_toc_header (int fd, ref cdrom_tochdr header)
        {
            return read_toc_header (fd, CDROMREADTOCHDR, ref header);
        }

        [DllImport ("libc", EntryPoint = "ioctl")]
        static extern int read_multisession (int fd, int request, ref cdrom_multisession multisession);
        static int read_multisession (int fd, ref cdrom_multisession multisession)
        {
            return read_multisession (fd, CDROMMULTISESSION, ref multisession);
        }

        [DllImport ("libc", EntryPoint = "ioctl")]
        static extern int read_toc_entry (int fd, int request, ref cdrom_tocentry entry);
        static int read_toc_entry (int fd, ref cdrom_tocentry entry)
        {
            return read_toc_entry (fd, CDROMREADTOCENTRY, ref entry);
        }

        struct cdrom_tochdr
        {
            public byte cdth_trk0;
            public byte cdth_trk1;
        }

        struct cdrom_tocentry
        {
            public byte cdte_track;
            public byte adr_ctrl;
            public byte cdte_format;
            public int lba;
            public byte cdte_datamode;
        }

        struct cdrom_multisession
        {
            public int lba;
            public byte xa_flag;
            public byte addr_format;
        }

        int ReadTocHeader (int fd)
        {
            cdrom_tochdr th = new cdrom_tochdr ();
            cdrom_multisession ms = new cdrom_multisession ();

            int ret = read_toc_header (fd, ref th);

            if (ret < 0) return ret;

            first_track = th.cdth_trk0;
            last_track = th.cdth_trk1;

            ms.addr_format = CDROM_LBA;
            ret = read_multisession (fd, ref ms);

            if(ms.xa_flag != 0) last_track--;

            return ret;
        }

        static int ReadTocEntry (int fd, byte track_number, ref ulong lba)
        {
            cdrom_tocentry te = new cdrom_tocentry ();
            te.cdte_track = track_number;
            te.cdte_format = CDROM_LBA;

            int ret = read_toc_entry (fd, ref te);

            if(ret == 0) lba = (ulong)te.lba;

            return ret;
        }

        int ReadLeadout (int fd, ref ulong lba)
        {
            cdrom_multisession ms = new cdrom_multisession ();
            ms.addr_format = CDROM_LBA;

            int ret = read_multisession (fd, ref ms);

            if (ms.xa_flag != 0) {
                lba = (ulong)(ms.lba - XA_INTERVAL);
                return ret;
            }

            return ReadTocEntry (fd, CDROM_LEADOUT, ref lba);
        }

        internal DiscLinux (string device)
        {
            int fd = open (device, O_RDONLY | O_NONBLOCK);

            if (fd < 0) throw new LocalDiscException (String.Format ("Cannot open device '{0}'", device));

            try {
                if (ReadTocHeader (fd) < 0) throw new LocalDiscException ("Cannot read table of contents");
                if (last_track == 0) throw new LocalDiscException ("This disc has no tracks");

                ulong lba = 0;
                ReadLeadout (fd, ref lba);
                track_offsets [0] = (int)lba + 150;

                for (byte i = first_track; i <= last_track; i++) {
                    ReadTocEntry (fd, i, ref lba);
                    track_offsets[i] = (int)lba + 150;
                }
            } finally {
                close (fd);
            }

            Init ();
        }
    }
}
