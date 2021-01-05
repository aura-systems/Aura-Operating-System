using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.Config
{
    class DNSConfig
    {
        public static List<Address> DNSNameservers = new List<Address>();

        public static void Add(Address nameserver)
        {
            foreach (var ns in DNSNameservers)
            {
                if (ns.address.ToString() == nameserver.address.ToString())
                {
                    return;
                }
            }
            DNSNameservers.Add(nameserver);
        }

        public static void Remove(Address nameserver)
        {
            int counter = 0;

            foreach (var ns in DNSNameservers)
            {
                if (ns == nameserver)
                {
                    DNSNameservers.RemoveAt(counter);
                }
                counter++;
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
