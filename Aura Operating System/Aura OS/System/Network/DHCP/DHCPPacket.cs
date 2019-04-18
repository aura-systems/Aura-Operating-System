using Aura_OS.HAL;
using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP Packet
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.DHCP
{
    public class DHCPPacket : IPPacket
    {
        public DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        public static void DHCPHandler(byte[] packetData)
        {            
            DHCPOption Options = new DHCPOption(packetData);

            if (Options.Type == 0x02)
            {
                //Offert packet received
                Core.SendRequestPacket(Options.Address(), Options.Server());
            }

            if (Options.Type == 0x05 || Options.Type == 0x06)
            {
                //ACK or NAK DHCP packet received
                if (Core.DHCPAsked)
                {
                    Core.Apply(Options, true);
                }
                else
                {
                    Core.Apply(Options);
                }
            }
        }

        protected override void initFields()
        {
            base.initFields();
        }

        public static int PacketSize { get; set; }

        public DHCPPacket(Address source, MACAddress mac)
            : base(mac, MACAddress.Broadcast, 300, 0x11, source, Address.Broadcast, 0x00)
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

        public override string ToString()
        {
            return "DHCP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", ";
        }
    }
}