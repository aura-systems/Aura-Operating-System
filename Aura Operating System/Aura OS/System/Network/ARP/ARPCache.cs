/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ARP Cache (Contains MAC/IP)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using Aura_OS.HAL;

namespace Aura_OS.System.Network.ARP
{
    internal static class ARPCache
    {
        private static TempDictionary<MACAddress> cache;

        private static void ensureCacheExists()
        {
            if (cache == null)
            {
                cache = new TempDictionary<MACAddress>();
            }
        }

        internal static bool Contains(IPV4.Address ipAddress)
        {
            ensureCacheExists();
            return cache.ContainsKey(ipAddress.Hash);
        }

        internal static void Update(IPV4.Address ipAddress, MACAddress macAddress)
        {
            ensureCacheExists();
            if (ipAddress == null)
            {
              global::System.Console.Write("");
            }
            UInt32 ip_hash = ipAddress.Hash;
            if (ip_hash == 0)
            {
                return;
            }

            if (cache.ContainsKey(ip_hash) == false)
            {
                cache.Add(ip_hash, macAddress);
            }
            else
            {
                cache[ip_hash] = macAddress;
            }
        }

        internal static MACAddress Resolve(IPV4.Address ipAddress)
        {
            ensureCacheExists();
            if (cache.ContainsKey(ipAddress.Hash) == false)
            {
                return null;
            }

            return cache[ipAddress.Hash];
        }
    }
}
