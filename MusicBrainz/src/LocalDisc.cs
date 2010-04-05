// LocalDisc.cs
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
using System.Security.Cryptography;
using System.Text;

namespace MusicBrainz
{
    public abstract class LocalDisc : Disc
    {
        static Uri submission_service_url = new Uri("http://mm.musicbrainz.org/bare/cdlookup.html");
        public static Uri SubmissionServiceUrl {
            get { return submission_service_url; }
            set {
                if (value == null) throw new ArgumentNullException ("value");
                submission_service_url = value;
            }
        }

        internal byte first_track;
        internal byte last_track;
        internal int [] track_offsets = new int [100];
        TimeSpan [] track_durations;

        internal LocalDisc()
        {
        }

        internal void Init ()
        {
            track_durations = new TimeSpan [last_track];
            for (int i = 1; i <= last_track; i++) {
                track_durations [i - 1] = TimeSpan.FromSeconds (
                    ((i < last_track ? track_offsets [i + 1] : track_offsets [0]) - track_offsets [i]) / 75); // 75 frames in a second
            }
            GenerateId ();
        }

        void GenerateId ()
        {
            StringBuilder input_builder = new StringBuilder (804);

            input_builder.AppendFormat ("{0:X2}", first_track);
            input_builder.AppendFormat ("{0:X2}", last_track);

            for (int i = 0; i < track_offsets.Length; i++)
                input_builder.AppendFormat ("{0:X8}", track_offsets[i]);

            // MB uses a slightly modified RFC822 for reasons of URL happiness.
            string base64 = Convert.ToBase64String (SHA1.Create ().
                ComputeHash (Encoding.ASCII.GetBytes (input_builder.ToString ())));
            StringBuilder hash_builder = new StringBuilder (base64.Length);

            foreach (char c in base64)
                switch (c) {
                case '+':
                    hash_builder.Append ('.');
                    break;
                case '/':
                    hash_builder.Append ('_');
                    break;
                case '=':
                    hash_builder.Append ('-');
                    break;
                default:
                    hash_builder.Append (c);
                    break;
                }

            Id = hash_builder.ToString ();
        }

        public TimeSpan [] GetTrackDurations ()
        {
            return (TimeSpan []) track_durations.Clone ();
        }

        Uri submission_url;
        public Uri SubmissionUrl {
            get {
                if (submission_url == null) {
                    submission_url = BuildSubmissionUrl ();
                }
                return submission_url; }
        }

        Uri BuildSubmissionUrl ()
        {
            StringBuilder builder = new StringBuilder ();
            builder.Append (SubmissionServiceUrl.AbsoluteUri);
            builder.Append ("?id=");
            builder.Append (Id);
            builder.Append ("&tracks=");
            builder.Append (last_track);
            builder.Append ("&toc=");
            builder.Append (first_track);
            builder.Append ('+');
            builder.Append (last_track);
            builder.Append ('+');
            builder.Append (track_offsets [0]);
            for (int i = first_track; i <= last_track; i++) {
                builder.Append ('+');
                builder.Append (track_offsets [i]);
            }
            return new Uri(builder.ToString ());
        }

        public static LocalDisc GetFromDevice (string device)
        {
            if (device == null) throw new ArgumentNullException ("device");
            try {
                switch (Environment.OSVersion.Platform){
                case PlatformID.Unix:
                    return new DiscLinux (device);
                //case PlatformID.Win32NT:
                    //return new DiscWin32NT (device);
                default:
                    return new DiscWin32 (device);
                }
            } catch (Exception exception) {
                throw new LocalDiscException (exception);
            }
        }
    }

    public class LocalDiscException : Exception
    {
        public LocalDiscException (string message) : base (message)
        {
        }
        public LocalDiscException (Exception exception) : base ("Could not load local disc from device.", exception)
        {
        }
    }
}
