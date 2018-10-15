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
            foreach (HAL.Drivers.Network.NetworkDevice networkDevice in HAL.Drivers.Network.NetworkDevice.Devices)
            {
                DHCPDiscover dhcp_discover = new DHCPDiscover(networkDevice.MACAddress);
                OutgoingBuffer.AddPacket(dhcp_discover);
                NetworkStack.Update();
                CustomConsole.WriteLineInfo("Discovering Packet sent");
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
                CustomConsole.WriteLineInfo("Requesting Packet sent");
            }
        }

        public static void Apply(DHCPOption Options)
        {
            NetworkStack.RemoveAllConfigIP();

            Utils.Settings.LoadValues();
            Utils.Settings.EditValue("ipaddress", Options.Address().ToString());
            Utils.Settings.EditValue("subnet", Options.Subnet().ToString());
            Utils.Settings.EditValue("gateway", Options.Gateway().ToString());
            Utils.Settings.EditValue("dns01", Options.DNS01().ToString());
            Utils.Settings.Reload();

            NetworkInit.Init(false);
            NetworkInit.Enable();

            Apps.System.Debugger.debugger.Send("New DHCP configuration applied!");
            CustomConsole.WriteLineOK("New DHCP configuration applied!");
        }
    }
}
