//
// Manager.cs
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

// 2008-09-28, patrick ulbrich: added InitBusG()

using System;
using System.Collections;
using System.Collections.Generic;

using NDesk.DBus;

namespace Hal
{
    internal delegate void DBusDeviceAddedHandler(string udi);
    internal delegate void DBusDeviceRemovedHandler(string udi);
    internal delegate void DBusNewCapabilityHandler(string udi, string capability);
    
    [Interface("org.freedesktop.Hal.Manager")]
    internal interface IManager 
    {
        event DBusDeviceAddedHandler DeviceAdded;
        event DBusDeviceRemovedHandler DeviceRemoved;
        event DBusNewCapabilityHandler NewCapability;
    
        string [] GetAllDevices();
        bool DeviceExists(string udi);
        string [] FindDeviceStringMatch(string key, string value);
        string [] FindDeviceByCapability(string capability);
    }
    
    public class DeviceArgs : EventArgs
    {
        private string udi;
        
        public DeviceArgs(string udi)
        {
            this.udi = udi;
        }
        
        public string Udi {
            get { return udi; }
        }
    }
    
    public class DeviceAddedArgs : DeviceArgs
    {
        private Device device;
        
        public DeviceAddedArgs(string udi) : base(udi)
        {
        }
        
        public Device Device {
            get { 
                if(device == null) {
                    device = new Device(Udi);
                }
                
                return device;
            }
        }
    }
    
    public class DeviceRemovedArgs : DeviceArgs
    {
        public DeviceRemovedArgs(string udi) : base(udi)
        {
        }
    }
    
    public class NewCapabilityArgs : DeviceArgs
    {
        private string capability;
        
        public NewCapabilityArgs(string udi, string capability) : base(udi)
        {
            this.capability = capability;
        }
        
        public string Capability {
            get { return capability; }
        }
    }
    
    public delegate void DeviceAddedHandler(object o, DeviceAddedArgs args);
    public delegate void DeviceRemovedHandler(object o, DeviceRemovedArgs args);
    public delegate void NewCapabilityHandler(object o, NewCapabilityArgs args);
    
    public class Manager : IEnumerable<string>
    {
        private IManager manager;
        
        public event DeviceAddedHandler DeviceAdded;
        public event DeviceRemovedHandler DeviceRemoved;
        public event NewCapabilityHandler NewCapability;
        
        public Manager()
        {
            if(!Bus.System.NameHasOwner("org.freedesktop.Hal")) {
                throw new ApplicationException("Could not find org.freedesktop.Hal");
            }
            
            manager = Bus.System.GetObject<IManager>("org.freedesktop.Hal",
                new ObjectPath("/org/freedesktop/Hal/Manager"));
            
            if(manager == null) {
                throw new ApplicationException("The /org/freedesktop/Hal/Manager object could not be located on the DBUs interface org.freedesktop.Hal");
            }

            manager.DeviceAdded += OnDeviceAdded;
            manager.DeviceRemoved += OnDeviceRemoved;
            manager.NewCapability += OnNewCapability;
        }
        
		// required by gtk applications to avoid multithreading issues
		public static void InitBusG() {
			NDesk.DBus.BusG.Init();
		}
		
        protected virtual void OnDeviceAdded(string udi)
        {
            if(DeviceAdded != null)
                DeviceAdded(this, new DeviceAddedArgs(udi));
        }
        
        protected virtual void OnDeviceRemoved(string udi)
        {
            if(DeviceRemoved != null)
                DeviceRemoved(this, new DeviceRemovedArgs(udi));
        }
        
        protected virtual void OnNewCapability(string udi, string capability)
        {
            if(NewCapability != null)
                NewCapability(this, new NewCapabilityArgs(udi, capability));
        }
        
        public bool DeviceExists(string udi)
        {
            return manager.DeviceExists(udi);
        }
        
        public string [] FindDeviceByStringMatch(string key, string value)
        {
            return manager.FindDeviceStringMatch(key, value);
        }
        
        public string [] FindDeviceByCapability(string capability)
        {
            return manager.FindDeviceByCapability(capability);
        }
        
        public Device [] FindDeviceByCapabilityAsDevice(string capability)
        {
            return Device.UdisToDevices(FindDeviceByCapability(capability));
        }
        
        public Device [] FindDeviceByStringMatchAsDevice(string key, string value)
        {
            return Device.UdisToDevices(FindDeviceByStringMatch(key, value));
        }
        
        public string [] GetAllDevices()
        {
            return manager.GetAllDevices();
        }
        
        public IEnumerator<string> GetEnumerator()
        {
            foreach(string device in GetAllDevices()) {
                yield return device;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
