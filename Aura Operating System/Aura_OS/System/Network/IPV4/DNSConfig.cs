using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4
{
    class DNSConfig
    {
        public static List<Address> DNSNameservers = new List<Address>();

        public static void Add(Address nameserver)
        {
            if (!DNSNameservers.Contains(nameserver))
            {
                DNSNameservers.Add(nameserver);
            }         
        }

        public static void Remove(Address nameserver)
        {
            if (DNSNameservers.Contains(nameserver))
            {
                DNSNameservers.Remove(nameserver);
            }
        }

        /// <summary>
        /// Call this to get your adress to request your DNS server
        /// </summary>
        /// <param name="index">Which server you want to get</param>
        /// <returns>DNS Server</returns>
        public static Address Server(int index)
        {
            return DNSNameservers[index];
        }
    }
}
