//
// Device.cs
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
    public struct PropertyModification
    {
        public string Key;
        public bool Added;
        public bool Removed;
    }

    internal delegate void DBusPropertyModifiedHandler(int modificationsLength, 
        PropertyModification [] modifications);
    
    [Interface("org.freedesktop.Hal.Device")]
    internal interface IDevice
    {
        // TODO:
        // Need to support the Condition event, but it has a
        // variable number of arguments, not currently supported
        
        event DBusPropertyModifiedHandler PropertyModified;
    
        void SetPropertyString(string key, string value);
        void SetPropertyInteger(string key, int value);
        void SetPropertyBoolean(string key, bool value);
        void SetPropertyDouble(string key, double value);
        void SetPropertyStringList(string key, string [] value);
        
        void SetProperty(string key, ulong value);
        ulong GetProperty(string key); // nasty hack to get around the fact
                                       // that HAL doesn't actually send this
                                       // in a variant, nor does it have a 
                                       // GetPropertyUInt64
                                       // should be object GetProperty(string key)

        void StringListPrepend(string key, string value);
        void StringListAppend(string key, string value);
        void StringListRemove(string key, string value);
        
        string GetPropertyString(string key);
        int GetPropertyInteger(string key);
        bool GetPropertyBoolean(string key);
        double GetPropertyDouble(string key);
        string [] GetPropertyStringList(string key);
        
        IDictionary<string, object> GetAllProperties();
        void RemoveProperty(string key);
        PropertyType GetPropertyType(string key);
        bool PropertyExists(string key);
        
        void AddCapability(string capability);
        bool QueryCapability(string capability);
        void Lock(string reason);
        void Unlock();
    }
    
    internal enum DType : byte
    {
      Invalid = (byte)'\0',
      Byte = (byte)'y',
      Boolean = (byte)'b',
      Int16 = (byte)'n',
      UInt16 = (byte)'q',
      Int32 = (byte)'i',
      UInt32 = (byte)'u',
      Int64 = (byte)'x',
      UInt64 = (byte)'t',
      Single = (byte)'f',
      Double = (byte)'d',
      String = (byte)'s',
      ObjectPath = (byte)'o',
      Signature = (byte)'g',
      Array = (byte)'a',
      Struct = (byte)'r',
      DictEntry = (byte)'e',
      Variant = (byte)'v',
      StructBegin = (byte)'(',
      StructEnd = (byte)')',
      DictEntryBegin = (byte)'{',
      DictEntryEnd = (byte)'}',
    }
    
    public enum PropertyType
    {
        Invalid = DType.Invalid,
        Int32 = DType.Int32,
        UInt64 = DType.UInt64,
        Double = DType.Double,
        Boolean = DType.Boolean,
        String = DType.String,
        StrList = ((int)(DType.String << 8) + ('l')) 
    }

    public class PropertyModifiedArgs : EventArgs
    {
        private PropertyModification [] modifications;
        
        public PropertyModifiedArgs(PropertyModification [] modifications)
        {
            this.modifications = modifications;
        }
        
        public PropertyModification [] Modifications {
            get { return modifications; }
        }
    }

    public delegate void PropertyModifiedHandler(object o, PropertyModifiedArgs args);

    public class Device : IEnumerable<KeyValuePair<string, object>>, IEqualityComparer<Device>,
        IEquatable<Device>, IComparer<Device>, IComparable<Device>
    {
        private string udi;
        private IDevice device;
        
        public event PropertyModifiedHandler PropertyModified;
        
        public Device(string udi)
        {
            this.udi = udi;
            
            device = CastDevice<IDevice>();
            device.PropertyModified += OnPropertyModified;
        }
        
        public static Device [] UdisToDevices(string [] udis)
        {
            if(udis == null || udis.Length == 0) {
                return new Device[0];
            }
            
            Device [] devices = new Device[udis.Length];
            for(int i = 0; i < udis.Length; i++) {
                devices[i] = new Device(udis[i]);
            }
            
            return devices;
        }
        
        protected virtual void OnPropertyModified(int modificationsLength, PropertyModification [] modifications)
        {
            if(modifications.Length != modificationsLength) {
                throw new ApplicationException("Number of modified properties does not match");
            }
        
            PropertyModifiedHandler handler = PropertyModified;
            if(handler != null) {
                handler(this, new PropertyModifiedArgs(modifications));   
            }
        }
        
        public string [] GetChildren(Manager manager)
        {
            return manager.FindDeviceByStringMatch("info.parent", Udi);
        }
        
        public Device [] GetChildrenAsDevice(Manager manager)
        {
            return manager.FindDeviceByStringMatchAsDevice("info.parent", Udi);
        }
        
        public void Lock(string reason)
        {
            device.Lock(reason);
        }
        
        public void Unlock()
        {
            device.Unlock();
        }

        public string GetPropertyString(string key)
        {
            return device.GetPropertyString(key);
        }

        public int GetPropertyInteger(string key)
        {
            return device.GetPropertyInteger(key);
        }
        
        public ulong GetPropertyUInt64(string key)
        {
            return device.GetProperty(key);
        }

        public double GetPropertyDouble(string key)
        {
            return device.GetPropertyDouble(key);
        }

        public bool GetPropertyBoolean(string key)
        {
            return device.GetPropertyBoolean(key);
        }

        public string [] GetPropertyStringList(string key)
        {
            return device.GetPropertyStringList(key);
        }

        public PropertyType GetPropertyType(string key)
        {
            return PropertyExists(key) ? device.GetPropertyType(key) : PropertyType.Invalid;
        }
        
        public void StringListPrepend(string key, string value)
        {
            device.SetPropertyString(key, value);
        }
        
        public void StringListAppend(string key, string value)
        {
            device.StringListAppend(key, value);
        }
        
        public void StringListRemove(string key, string value)
        {
            device.StringListRemove(key, value);
        }
        
        public void SetPropertyString(string key, string value)
        {
            device.SetPropertyString(key, value);
        }
        
        public void SetPropertyUInt64(string key, ulong value)
        {
            device.SetProperty(key, value);
        }

        public void SetPropertyInteger(string key, int value)
        {
            device.SetPropertyInteger(key, value);
        }

        public void SetPropertyDouble(string key, double value)
        {
            device.SetPropertyDouble(key, value);
        }

        public void SetPropertyBoolean(string key, bool value)
        {
            device.SetPropertyBoolean(key, value);
        }
        
        public void SetPropertyStringList(string key, string [] value)
        {
            device.SetPropertyStringList(key, value);
        }
        
        public void RemoveProperty(string key)
        {
            device.RemoveProperty(key);
        }
        
        public bool PropertyExists(string key)
        {
            return device.PropertyExists(key);
        }
        
        public void AddCapability(string capability)
        {
            device.AddCapability(capability);
        }
        
        public bool QueryCapability(string capability)
        {
            return device.QueryCapability(capability);
        }
        
        public T CastDevice<T>()
        {
            if(!Bus.System.NameHasOwner("org.freedesktop.Hal")) {
                throw new ApplicationException("Could not find org.freedesktop.Hal");
            }
            
            return Bus.System.GetObject<T>("org.freedesktop.Hal", new ObjectPath(Udi));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return device.GetAllProperties().GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return device.GetAllProperties().GetEnumerator();
        }
        
        public bool Equals(Device other)
        {
            return Udi.Equals(other.Udi);
        }
        
        public bool Equals(Device a, Device b)
        {
            return a.Udi.Equals(b.Udi);
        }
        
        public int CompareTo(Device other)
        {
            return Udi.CompareTo(other.Udi);
        }
        
        public int Compare(Device a, Device b)
        {
            return a.Udi.CompareTo(b.Udi);
        }
        
        public int GetHashCode(Device a)
        {
            return a.Udi.GetHashCode();
        }
        
        public override int GetHashCode()
        {
            return Udi.GetHashCode();
        }
        
        public override string ToString()
        {
            return udi;
        }
        
        public string this[string property] {
            get { return PropertyExists(property) ? GetPropertyString(property) : null; }
            set { SetPropertyString(property, value); }
        }
        
        public string Udi {
            get { return udi; }
        }
        
        public bool IsVolume {
            get {
                if(!PropertyExists("info.interfaces")) {
                    return false;
                }
                
                foreach(string @interface in GetPropertyStringList("info.interfaces")) {
                    if(@interface == "org.freedesktop.Hal.Device.Volume") {
                       return true;
                    }
                }
                
                return false;
            }
        }
        
        public Volume Volume {
            get { return new Volume(Udi); }
        }
        
        public Device Parent {
            get {
                if(PropertyExists("info.parent")) {
                    return new Device(this["info.parent"]);
                }
                
                return null;
            }
        }
    }
}
