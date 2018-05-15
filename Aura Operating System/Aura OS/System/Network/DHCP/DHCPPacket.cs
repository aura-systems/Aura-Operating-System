using Aura_OS.HAL;
using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.DHCP
{
    public class DHCPPacket : IPPacket
    {
        protected int Identification;
        Random rnd = new Random();        

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
            Identification = rnd.Next(0, Int32.MaxValue);
            //290
            //20 bytes Internet Protocol version 4 Src: 0.0.0.0 Dst: 255.255.255.255

            //IPv4
            mRawData[14] = 0x45;

            //Services Field
            mRawData[15] = 0x00;

            //Total Length
            PacketSize = 248 + 8 + 20 + 14;
            mRawData[16] = (byte)((PacketSize >> 8) & 0xFF);
            mRawData[17] = (byte)((PacketSize >> 0) & 0xFF);

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
        
        public override string ToString()
        {
            return "DHCP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", ";
        }
    }

    public class DHCPRequest : DHCPPacket
    {
        public DHCPRequest()
            : base()
        { }

        public DHCPRequest(byte[] rawData)
            : base(rawData)
        { }

        public DHCPRequest(MACAddress src, Address source, Address requested)
            : base(src, source, requested)
        {
            //UDP

            //Source Port 68
            mRawData[36] = 0x00;
            mRawData[36] = 0x44;

            //Destination Port 67
            mRawData[36] = 0x00;
            mRawData[36] = 0x43;

            //Length
            PacketSize = 248 + 8;
            mRawData[37] = (byte)((PacketSize >> 8) & 0xFF);
            mRawData[38] = (byte)((PacketSize >> 0) & 0xFF);

            //Checksum
            mRawData[39] = 0x00;
            mRawData[40] = 0x00;

            initFields();
        }
    }

    public class DHCPDiscover : DHCPRequest
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
            mRawData[41] = 0x01;

            //ethernet
            mRawData[42] = 0x01;

            //Length mac
            mRawData[43] = 0x06;

            //option bootp
            mRawData[44] = 0x00;
            mRawData[45] = 0x00;
            mRawData[46] = 0x00;
            mRawData[48] = 0x00;
            mRawData[49] = 0x00;
            mRawData[50] = 0x00;
            mRawData[51] = 0x00;
            mRawData[52] = 0x00;
            mRawData[53] = 0x00;
            mRawData[54] = 0x00;
            mRawData[55] = 0x00;
            mRawData[56] = 0x00;
            mRawData[57] = 0x00;
            mRawData[58] = 0x00;
            mRawData[59] = 0x00;
            mRawData[60] = 0x00;
            mRawData[61] = 0x00;
            mRawData[62] = 0x00;
            mRawData[63] = 0x00;
            mRawData[64] = 0x00;

            //Src mac
            mRawData[65] = src.bytes[0];
            mRawData[66] = src.bytes[1];
            mRawData[67] = src.bytes[2];
            mRawData[68] = src.bytes[3];
            mRawData[69] = src.bytes[4];
            mRawData[70] = src.bytes[5];

            //Fill 0
            for (int i = 71; i < 272; i++)
            {
                mRawData[i] = 0x00;
            }

            //DHCP Magic cookie
            mRawData[273] = 0x63;
            mRawData[274] = 0x82;
            mRawData[275] = 0x53;
            mRawData[276] = 0x63;

            //options

            //Discover
            mRawData[277] = 0x35;
            mRawData[278] = 0x01;
            mRawData[279] = 0x01;

            //Requested IP Address
            mRawData[280] = requested_ip.address[0];
            mRawData[281] = requested_ip.address[1];
            mRawData[282] = requested_ip.address[2];
            mRawData[283] = requested_ip.address[3];

            //Parameters start here
            mRawData[284] = 0x37;

            //Parameters length
            mRawData[285] = 0x04;

            //Parameters
            mRawData[286] = 0x01;
            mRawData[287] = 0x03;
            mRawData[288] = 0x15;
            mRawData[289] = 0x06;

            mRawData[290] = 0xff; //END

            initFields();
        }
    }
}