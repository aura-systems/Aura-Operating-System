using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.UDP.DNS
{
    class DNSCache
    {
        public static List<string> Name = new List<string>();
        public static List<Address> Address = new List<Address>();

        public static void AddEntry(string name, Address address)
        {
            Name.Add(name);
            Address.Add(address);
        }

        public static Address GetCache(string name)
        {
            if (Name.Contains(name))
            {
                return Address[Name.IndexOf(name)];
            }
            else
            {
                return null;
            }
        }

        public static string GetReverseCache(Address address)
        {
            if (Address.Contains(address))
            {
                return Name[Address.IndexOf(address)];
            }
            else
            {
                return null;
            }
        }

        public static void Clear()
        {
            Name.Clear();
            Address.Clear();
        }
    }
}
