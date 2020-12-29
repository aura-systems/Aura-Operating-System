/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List of all IPs / Utils
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Collections.Generic;
using System;

namespace Aura_OS.System.Network.IPV4
{
    /// <summary>
    /// Contains a IPv4 configuration
    /// </summary>
    public class Config
    {
        internal static List<Config> ipConfigs;

        static Config()
        {
            ipConfigs = new List<Config>();
        }

        internal static void Add(Config config)
        {
            ipConfigs.Add(config);
        }

        internal static void Remove(Config config)
        {
            int counter = 0;

            foreach (var ipconfig in ipConfigs)
            {
                if (ipconfig == config)
                {
                    ipConfigs.RemoveAt(counter);
                    return;
                }
                counter++;
            }
        }

        internal static void RemoveAll()
        {
            ipConfigs.Clear();
        }

        internal static Address FindNetwork(Address destIP)
        {
            Address default_gw = null;

            foreach (Config ipConfig in ipConfigs)
            {
                if ((ipConfig.IPAddress.Hash & ipConfig.SubnetMask.Hash) ==
                    (destIP.Hash & ipConfig.SubnetMask.Hash))
                {
                    return ipConfig.IPAddress;
                }
                if ((default_gw == null) && (ipConfig.DefaultGateway.CompareTo(Address.Zero) != 0))
                {
                    default_gw = ipConfig.IPAddress;
                }

                if (!IsLocalAddress(destIP))
                {
                    return ipConfig.IPAddress;
                }
            }

            return default_gw;
        }

        internal static bool IsLocalAddress(Address destIP)
        {
            for (int c = 0; c < ipConfigs.Count; c++)
            {
                if ((ipConfigs[c].IPAddress.Hash & ipConfigs[c].SubnetMask.Hash) ==
                    (destIP.Hash & ipConfigs[c].SubnetMask.Hash))
                {
                    return true;
                }
            }

            return false;
        }

        internal static HAL.Drivers.Network.NetworkDevice FindInterface(Address sourceIP)
        {
            return NetworkStack.AddressMap[sourceIP.Hash];
        }

        internal static Address FindRoute(Address destIP)
        {
            for (int c = 0; c < ipConfigs.Count; c++)
            {
                //if (ipConfigs[c].DefaultGateway.CompareTo(Address.Zero) != 0)
                //{
                //    return ipConfigs[c].DefaultGateway;
                //}
                return ipConfigs[c].DefaultGateway;
            }

            return null;
        }

        protected Address address;
        protected Address defaultGateway;
        protected Address subnetMask;
        protected Address defaultDns;

        /// <summary>
        /// Create a IPv4 Configuration with no default gateway
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnet">Subnet Mask</param>
        public Config(Address ip, Address subnet)
            : this(ip, subnet, Address.Zero)
        { }

        /// <summary>
        /// Create a IPv4 Configuration
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnet">Subnet Mask</param>
        /// <param name="gw">Default gateway</param>
        public Config(Address ip, Address subnet, Address gw)
        {
            this.address = ip;
            this.subnetMask = subnet;
            this.defaultGateway = gw;
        }

        public Address IPAddress
        {
            get { return this.address; }
            set { this.address = value; }
        }
        public Address SubnetMask
        {
            get { return this.subnetMask; }
            set { this.subnetMask = value; }
        }
        public Address DefaultGateway
        {
            get { return this.defaultGateway; }
            set { this.defaultGateway = value; }
        }
        public Address DefaultDNSServer
        {
            get { return this.defaultDns; }
            set { this.defaultDns = value; }
        }

        public override string ToString()
        {
            return "IPAddress=" + IPAddress + ", SubnetMask=" + SubnetMask + ", DefaultGateway=" + DefaultGateway;
        }
    }
}
