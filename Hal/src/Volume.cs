//
// Volume.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2006-2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;

using NDesk.DBus;

namespace Hal
{
    [Interface("org.freedesktop.Hal.Device.Volume")]
    internal interface IVolume
    {
        void Mount(string [] args);
        void Unmount(string [] args);
        void Eject(string [] args);
    }
    
    public class Volume : Device
    {
        public Volume(string udi) : base(udi)
        {
        }

        public void Mount()
        {
            Mount(new string [] { String.Empty });
        }
        
        public void Mount(params string [] args)
        {
            CastDevice<IVolume>().Mount(args);
        }
        
        public void Unmount()
        {
            Unmount(new string [] { String.Empty });
        }
        
        public void Unmount(params string [] args)
        {
            CastDevice<IVolume>().Unmount(args);
        }
        
        public void Eject()
        {
            Eject(new string [] { String.Empty });
        }
        
        public void Eject(params string [] args)
        {
            CastDevice<IVolume>().Eject(args);
        }
    }
}
