using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Core
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.DHCP
{
    class Core
    {
        /// <summary>
        /// Get the IP address of the DHCP server
        /// </summary>
        /// <returns></returns>
        public static Address DHCPServerAddress()
        {            
            return Address.Parse(Kernel.settings.Get("dhcp_server"));
        }

        /// <summary>
        /// Send a packet to the DHCP server to make the address available again
        /// </summary>
        public static void SendReleasePacket()
        {
            Address source = Config.FindNetwork(DHCPServerAddress());
            DHCPRelease dhcp_release = new DHCPRelease(source, DHCPServerAddress());
            OutgoingBuffer.AddPacket(dhcp_release);
            NetworkStack.Update();
        }

        /// <summary>
        /// Send a packet to find the DHCP server and tell that we want a new IP address
        /// </summary>
        public static void SendDiscoverPacket()
        {
            foreach (HAL.Drivers.Network.NetworkDevice networkDevice in HAL.Drivers.Network.NetworkDevice.Devices)
            {
                DHCPDiscover dhcp_discover = new DHCPDiscover(networkDevice.MACAddress);
                OutgoingBuffer.AddPacket(dhcp_discover);
                NetworkStack.Update();
            }            
        }

        /// <summary>
        /// Send a request to apply the new IP configuration
        /// </summary>
        public static void SendRequestPacket(Address RequestedAddress, Address DHCPServerAddress)
        {
            foreach (HAL.Drivers.Network.NetworkDevice networkDevice in HAL.Drivers.Network.NetworkDevice.Devices)
            {
                DHCPRequest dhcp_request = new DHCPRequest(networkDevice.MACAddress, RequestedAddress, DHCPServerAddress);
                OutgoingBuffer.AddPacket(dhcp_request);
                NetworkStack.Update();
            }
        }

        public static void Apply(DHCPOption Options)
        {
            NetworkStack.RemoveAllConfigIP();

            foreach (HAL.Drivers.Network.NetworkDevice networkDevice in HAL.Drivers.Network.NetworkDevice.Devices)
            {
                NetworkInit.NetworkSettings(networkDevice).Edit("ipaddress", Options.Address().ToString());
                NetworkInit.NetworkSettings(networkDevice).Edit("subnet", Options.Subnet().ToString());
                NetworkInit.NetworkSettings(networkDevice).Edit("gateway", Options.Gateway().ToString());
                NetworkInit.NetworkSettings(networkDevice).Push();
            }

            NetworkInit.Init(false);
            NetworkInit.Enable();
        }
    }
}
