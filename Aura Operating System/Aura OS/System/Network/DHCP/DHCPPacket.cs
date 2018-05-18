using Aura_OS.HAL;
using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.DHCP
{
    public class DHCPPacket : IPPacket
    {  
        public static void VMTInclude()
        {
            new DHCPPacket();
        }

        public DHCPPacket()
            : base()
        { }

        public DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        public static void DHCPHandler(byte[] packetData)
        {
            DHCPPacket dhcp_packet = new DHCPPacket(packetData);

            Kernel.debugger.Send("Received DHCP packet from " + dhcp_packet.SourceIP.ToString());
            Console.WriteLine("DHCP Offer received!");
        }

        public static int PacketSize { get; set; }

        public DHCPPacket(MACAddress src, Address source, Address requested)
            : base(src, MACAddress.Broadcast, 300, 0x11, source, Address.Broadcast, 0x00)
        {
            //UDP

            //Source Port 68
            mRawData[dataOffset + 0] = 0x00;
            mRawData[dataOffset + 1] = 0x44;

            //Destination Port 67
            mRawData[dataOffset + 2] = 0x00;
            mRawData[dataOffset + 3] = 0x43;

            //Length
            PacketSize = 271;
            mRawData[dataOffset + 4] = (byte)((PacketSize >> 8) & 0xFF);
            mRawData[dataOffset + 5] = (byte)((PacketSize >> 0) & 0xFF);

            //Checksum
            mRawData[dataOffset + 6] = 0x00;
            mRawData[dataOffset + 7] = 0x00;

            initFields();
        }

        public static bool IsDHCPPacket(byte[] dhcpPacket)
        {
            if ((dhcpPacket[278] == 0x63) && (dhcpPacket[279] == 0x82) && (dhcpPacket[280] == 0x53) && (dhcpPacket[281] == 0x63)) //Magic cookie: DHCP
            {
                Console.WriteLine("IsDHCPPacket: Magic cookie detected");
                return true;
            }
            return false;
        }
        
        public override string ToString()
        {
            return "DHCP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", ";
        }
    }
}