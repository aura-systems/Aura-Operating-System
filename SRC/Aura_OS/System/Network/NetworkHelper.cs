/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Network helpers (gen2 ConfigEmpty / CurrentAddress / NameID / GetDeviceByName / DNS)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.DNS;

namespace Aura_OS.System.Network
{
    public static class NetworkHelper
    {
        /// <summary>
        /// gen2 !ConfigEmpty(). A 0.0.0.0 config (left by DHCP timeout/release) counts as none.
        /// GEN3-GAP(dhcp)
        /// </summary>
        public static bool IsConfigured
        {
            get
            {
                for (int i = 0; i < NetworkManager.DeviceCount; i++)
                {
                    IPConfig config = NetworkManager.GetAdapter(i).IPConfig;
                    if (config is not null && !config.Address.IsZero)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// gen2 NetworkConfiguration.CurrentAddress: the primary adapter's address, null when unconfigured or 0.0.0.0.
        /// </summary>
        public static Address CurrentAddress
        {
            get
            {
                IPConfig config = NetworkManager.Primary.IPConfig;
                return config is null || config.Address.IsZero ? null : config.Address;
            }
        }

        /// <summary>
        /// gen2 NetworkDevice.NameID. GEN3-GAP(nic-names): gen3 has no stable interface names.
        /// </summary>
        public static string AliasOf(NetworkAdapter adapter) => "eth" + adapter.Index.ToString();

        /// <summary>
        /// gen2 NetworkDevice.GetDeviceByName: accepts "eth0", "0" or the driver name.
        /// Returns default (IsValid == false) when none matches.
        /// </summary>
        public static NetworkAdapter FindAdapter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return default;
            }

            for (int i = 0; i < NetworkManager.DeviceCount; i++)
            {
                NetworkAdapter adapter = NetworkManager.GetAdapter(i);
                if (name == AliasOf(adapter) || name == i.ToString()
                    || string.Equals(name, adapter.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return adapter;
                }
            }

            return default; // IsValid == false
        }

        /// <summary>
        /// IP literal or DNS name -> Cosmos Address (A record). Null on failure (bad name, no reply,
        /// no DNS server, or no configured interface reaching it).
        /// </summary>
        public static Address Resolve(string hostOrIp, Address server = null, int timeoutMs = 5000)
        {
            if (string.IsNullOrEmpty(hostOrIp))
            {
                return null;
            }

            Address literal = Address.Parse(hostOrIp);
            if (literal is not null)
            {
                return literal;
            }

            server ??= DnsConfig.Nameservers.Count > 0 ? DnsConfig.Nameservers[0] : null;
            if (server is null)
            {
                return null;
            }

            // gen3: finally/using do not run on the exception path, dispose explicitly.
            DnsClient dns = null;
            Address result = null;

            try
            {
                dns = new DnsClient();
                dns.Connect(server);
                dns.SendQuery(hostOrIp);   // InvalidOperationException: no configured interface reaches the server
                result = dns.Receive(timeoutMs);
            }
            catch (Exception)
            {
                result = null;
            }

            if (dns is not null)
            {
                dns.Dispose();
            }

            return result;
        }
    }
}
