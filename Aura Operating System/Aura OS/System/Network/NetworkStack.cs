using System;
using Sys = System;
using Aura_OS.HAL.Drivers.Network;
using Aura_OS.System.Network.ARP;
using Aura_OS.System.Network.IPV4;

namespace Aura_OS.System.Network
{
    /// <summary>
    /// Implement a Network Stack for all network devices and protocols
    /// </summary>
    public static class NetworkStack
    {
        internal static TempDictionary<HAL.Drivers.Network.NetworkDevice> AddressMap { get; private set; }

        /// <summary>
        /// Initialize the Network Stack to prepare it for operation
        /// </summary>
        public static void Init()
        {
            AddressMap = new TempDictionary<HAL.Drivers.Network.NetworkDevice>();

            // VMT Scanner issue workaround
            ARPPacket.VMTInclude();
            ARPPacket_Ethernet.VMTInclude();
            ARPReply_Ethernet.VMTInclude();
            ARPRequest_Ethernet.VMTInclude();
            ICMPPacket.VMTInclude();
            ICMPEchoReply.VMTInclude();
            ICMPEchoRequest.VMTInclude();
            IPV4.UDP.UDPPacket.VMTInclude();
        }

        /// <summary>
        /// Configure a IP configuration on the given network device.
        /// <remarks>Multiple IP Configurations can be made, like *nix environments</remarks>
        /// </summary>
        /// <param name="nic"><see cref="NetworkDevice"/> that will have the assigned configuration</param>
        /// <param name="config"><see cref="IPV4.Config"/> instance that defines the IP Address, Subnet
        /// Mask and Default Gateway for the device</param>
        public static void ConfigIP(NetworkDevice nic, IPV4.Config config)
        {
            AddressMap.Add(config.IPAddress.Hash, nic);
            IPV4.Config.Add(config);
            nic.DataReceived = HandlePacket;
        }

        internal static void HandlePacket(byte[] packetData)
        {
            Console.Write("Packet Received Length=");
            if (packetData == null)
            {
                Sys.Console.WriteLine("null");
                return;
            }
            Sys.Console.WriteLine(packetData.Length);
            //Sys.Console.WriteLine(BitConverter.ToString(packetData));

            UInt16 etherType = (UInt16)((packetData[12] << 8) | packetData[13]);
            switch (etherType)
            {
                case 0x0806:
                    ARPPacket.ARPHandler(packetData);
                    break;
                case 0x0800:
                    IPV4.IPPacket.IPv4Handler(packetData);
                    break;
            }
        }

        /// <summary>
        /// Called continously to keep the Network Stack going.
        /// </summary>
        public static void Update()
        {
            IPV4.OutgoingBuffer.Send();
        }
    }
}
