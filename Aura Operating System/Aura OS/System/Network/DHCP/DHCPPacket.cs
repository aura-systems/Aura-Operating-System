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

        public static int PacketSize { get; set; }

        public DHCPPacket(MACAddress src, Address source, Address requested)
            : base(src, MACAddress.Broadcast, 290, 0x11, source, Address.Broadcast, 0x00)
        {
            //UDP

            //Source Port 68
            mRawData[dataOffset + 0] = 0x00;
            mRawData[dataOffset + 1] = 0x44;

            //Destination Port 67
            mRawData[dataOffset + 2] = 0x00;
            mRawData[dataOffset + 3] = 0x43;

            //Length
            PacketSize = 248 + 8;
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

    public class DHCPDiscover : DHCPPacket
    {

        public DHCPDiscover()
            : base()
        { }

        public DHCPDiscover(byte[] rawData)
            : base(rawData)
        { }

        public static int PacketSize { get; set; }

        public DHCPDiscover(MACAddress src, Address source, Address requested_ip)
            : base(src, source, requested_ip)
        {
            //248
            //Request
            mRawData[dataOffset + 8] = 0x01;

            //ethernet
            mRawData[dataOffset + 9] = 0x01;

            //Length mac
            mRawData[dataOffset + 10] = 0x06;

            //option bootp
            for (int i = 11; i < 31; i++)
            {
                mRawData[dataOffset + i] = 0x00;
            }

            //Src mac
            mRawData[dataOffset + 32] = src.bytes[0];
            mRawData[dataOffset + 33] = src.bytes[1];
            mRawData[dataOffset + 34] = src.bytes[2];
            mRawData[dataOffset + 35] = src.bytes[3];
            mRawData[dataOffset + 36] = src.bytes[4];
            mRawData[dataOffset + 37] = src.bytes[5];

            //Fill 0
            for (int i = 38; i < 239; i++)
            {
                mRawData[dataOffset + i] = 0x00;
            }

            //DHCP Magic cookie
            mRawData[dataOffset + 240] = 0x63;
            mRawData[dataOffset + 241] = 0x82;
            mRawData[dataOffset + 242] = 0x53;
            mRawData[dataOffset + 243] = 0x63;

            //options

            //Discover
            mRawData[dataOffset + 244] = 0x35;
            mRawData[dataOffset + 245] = 0x01;
            mRawData[dataOffset + 246] = 0x01;

            //Requested IP Address
            mRawData[dataOffset + 247] = requested_ip.address[0];
            mRawData[dataOffset + 248] = requested_ip.address[1];
            mRawData[dataOffset + 249] = requested_ip.address[2];
            mRawData[dataOffset + 250] = requested_ip.address[3];

            //Parameters start here
            mRawData[dataOffset + 251] = 0x37;

            //Parameters length
            mRawData[dataOffset + 252] = 0x04;

            //Parameters
            mRawData[dataOffset + 253] = 0x01;
            mRawData[dataOffset + 254] = 0x03;
            mRawData[dataOffset + 255] = 0x15;
            mRawData[dataOffset + 256] = 0x06;

            mRawData[dataOffset + 257] = 0xff; //END

            initFields();
        }
    }
}