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
    class DHCPCore
    {
        public static Address DHCPClientAddress;

        /// <summary>
        /// Get the IP address of the DHCP server
        /// </summary>
        /// <returns></returns>
        public static Address DHCPServerAddress()
        {
            Utils.Settings.LoadValues();
            return Address.Parse(Utils.Settings.GetValue("dhcp_server"));
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
            DHCPDiscover dhcp_discover = new DHCPDiscover();
            OutgoingBuffer.AddPacket(dhcp_discover);
            NetworkStack.Update();
        }

        /// <summary>
        /// Send a request to apply the new IP configuration
        /// </summary>
        public static void SendRequestPacket()
        {
            DHCPRequest dhcp_request = new DHCPRequest();
            OutgoingBuffer.AddPacket(dhcp_request);
            NetworkStack.Update();
        }
    }
}
