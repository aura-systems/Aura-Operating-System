using Aura_OS.System.Network.IPV4;
using System;
using Aura_OS.HAL.Drivers.Network;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Core
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.IPV4.UDP.DHCP
{
    class DHCPClient
    {
        public static bool DHCPAsked = false;

        /// <summary>
        /// Get the IP address of the DHCP server
        /// </summary>
        /// <returns></returns>
        public static Address DHCPServerAddress(NetworkDevice networkDevice)
        {
            return NetworkConfig.Get(networkDevice).DefaultGateway;
        }

        /// <summary>
        /// Send a packet to the DHCP server to make the address available again
        /// </summary>
        public static void SendReleasePacket()
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                Address source = Config.FindNetwork(DHCPServerAddress(networkDevice));
                DHCPRelease dhcp_release = new DHCPRelease(source, DHCPServerAddress(networkDevice), networkDevice.MACAddress);

                OutgoingBuffer.AddPacket(dhcp_release);
                NetworkStack.Update();

                NetworkStack.RemoveAllConfigIP();

                NetworkInit.Enable(networkDevice, new Address(0, 0, 0, 0), new Address(0, 0, 0, 0), new Address(0, 0, 0, 0), new Address(0, 0, 0, 0));

                
                //Utils.Settings settings = new Utils.Settings(@"0:\System\" + networkDevice.Name + ".conf");
                //settings.EditValue("ipaddress", "0.0.0.0");
                //settings.EditValue("subnet", "0.0.0.0");
                //settings.EditValue("gateway", "0.0.0.0");
                //settings.EditValue("dns01", "0.0.0.0");
                //settings.PushValues();
            }            
        }

        /// <summary>
        /// Send a packet to find the DHCP server and tell that we want a new IP address
        /// </summary>
        public static void SendDiscoverPacket()
        {
            NetworkStack.RemoveAllConfigIP();

            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                NetworkInit.Enable(networkDevice, new Address(0, 0, 0, 0), new Address(0, 0, 0, 0), new Address(0, 0, 0, 0), new Address(0, 0, 0, 0));

                DHCPDiscover dhcp_discover = new DHCPDiscover(networkDevice.MACAddress);
                OutgoingBuffer.AddPacket(dhcp_discover);
                NetworkStack.Update();

                DHCPAsked = true;
            }
        }

        
        /// <summary>
        /// Send a request to apply the new IP configuration
        /// </summary>
        public static void SendRequestPacket(Address RequestedAddress, Address DHCPServerAddress)
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                DHCPRequest dhcp_request = new DHCPRequest(networkDevice.MACAddress, RequestedAddress, DHCPServerAddress);
                OutgoingBuffer.AddPacket(dhcp_request);
                NetworkStack.Update();
            }
        }

        /*
         * Method called to applied the differents options received in the DHCP packet ACK
         **/
        /// <summary>
        /// Apply the new IP configuration received.
        /// </summary>
        /// <param name="Options">DHCPOption class using the packetData from the received dhcp packet.</param>
        /// <param name="message">Enable/Disable the displaying of messages about DHCP applying and conf. Disabled by default.
        /// </param>
        public static void Apply(DHCPAck packet, bool message = false)
        {
            NetworkStack.RemoveAllConfigIP();

            //cf. Roadmap. (have to change this, because some network interfaces are not configured in dhcp mode) [have to be done in 0.5.x]
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                if (packet.Client.ToString() == null ||
                    packet.Client.ToString() == null ||
                    packet.Client.ToString() == null ||
                    packet.Client.ToString() == null)
                {
                    CustomConsole.WriteLineError("Parsing DHCP ACK Packet failed, can't apply network configuration.");
                }
                else
                {
                    if (message)
                    {
                        Console.WriteLine();
                        CustomConsole.WriteLineInfo("[DHCP ACK][" + networkDevice.Name + "] Packet received, applying IP configuration...");
                        CustomConsole.WriteLineInfo("   IP Address  : " + packet.Client.ToString());
                        CustomConsole.WriteLineInfo("   Subnet mask : " + packet.Subnet.ToString());
                        CustomConsole.WriteLineInfo("   Gateway     : " + packet.Server.ToString());
                        CustomConsole.WriteLineInfo("   DNS server  : " + packet.DNS.ToString());
                    }

                    Utils.Settings settings = new Utils.Settings(@"0:\System\" + networkDevice.Name + ".conf");
                    settings.EditValue("ipaddress", packet.Client.ToString());
                    settings.EditValue("subnet", packet.Subnet.ToString());
                    settings.EditValue("gateway", packet.Server.ToString());
                    settings.EditValue("dns01", packet.DNS.ToString());
                    settings.EditValue("dhcp_server", packet.Server.ToString());
                    settings.PushValues();

                    NetworkInit.Enable(networkDevice, packet.Client, packet.Subnet, packet.Server, packet.DNS);

                    if (message)
                    {
                        CustomConsole.WriteLineOK("[DHCP CONFIG][" + networkDevice.Name + "] IP configuration applied.");
                        Console.WriteLine();
                        DHCPAsked = false;
                    }
                }
            }

            Kernel.BeforeCommand();
        }
    }
}
