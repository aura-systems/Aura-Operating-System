using Aura_OS.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.DHCP
{
    internal class DHCPPacket : EthernetPacket
    {
        protected int Identification;
        Random rnd = new Random();        

        public static void VMTInclude()
        {
            new DHCPPacket();
        }

        internal DHCPPacket()
            : base()
        { }

        internal DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        protected DHCPPacket(MACAddress src, int packet_size)
            : base(MACAddress.Broadcast, src, 0x0800, packet_size)
        {
            Identification = rnd.Next(0, Int32.MaxValue);

            //20 bytes Internet Protocol version 4 Src: 0.0.0.0 Dst: 255.255.255.255

            //IPv4
            mRawData[14] = 0x45;

            //Services Field
            mRawData[15] = 0x00;

            //Total Length
            mRawData[16] = (byte)((packet_size >> 8) & 0xFF);
            mRawData[17] = (byte)((packet_size >> 0) & 0xFF);

            //Identification
            mRawData[18] = (byte)((Identification >> 24) & 0xFF);
            mRawData[19] = (byte)((Identification >> 16) & 0xFF);
            mRawData[20] = (byte)((Identification >> 8) & 0xFF);
            mRawData[21] = (byte)((Identification >> 0) & 0xFF);

            //Flags
            mRawData[22] = 0x00;
            mRawData[23] = 0x00;

            //Time to live
            mRawData[24] = 0x80;

            //UDP
            mRawData[25] = 0x11;
            
            //Header Checksum
            mRawData[26] = 0x00;
            mRawData[27] = 0x00;
            
            //Source IP
            mRawData[28] = 0x00;
            mRawData[29] = 0x00;
            mRawData[30] = 0x00;
            mRawData[31] = 0x00;
            
            //Destination IP (Broadcast)
            mRawData[32] = 0xff;
            mRawData[33] = 0xff;
            mRawData[34] = 0xff;
            mRawData[35] = 0xff;

            initFields();
        }
    }
}
