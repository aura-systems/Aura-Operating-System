using System;
using Aura_OS.System.Network.IPV4;
using Aura_OS.HAL;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP Discovering
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.DHCP
{
    public class DHCPDiscover : DHCPPacket
    {
        protected int xID;

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
            //Request
            mRawData[dataOffset + 8] = 0x01;

            //ethernet
            mRawData[dataOffset + 9] = 0x01;

            //Length mac
            mRawData[dataOffset + 10] = 0x06;

            //hops
            mRawData[dataOffset + 11] = 0x00;


            Random rnd = new Random();
            xID = rnd.Next(0, Int32.MaxValue);
            mRawData[dataOffset + 12] = (byte)((xID >> 24) & 0xFF);
            mRawData[dataOffset + 13] = (byte)((xID >> 16) & 0xFF);
            mRawData[dataOffset + 14] = (byte)((xID >> 8) & 0xFF);
            mRawData[dataOffset + 15] = (byte)((xID >> 0) & 0xFF);


            //option bootp
            for (int i = 16; i < 35; i++)
            {
                mRawData[dataOffset + i] = 0x00;
            }

            //Src mac
            mRawData[dataOffset + 36] = src.bytes[0];
            mRawData[dataOffset + 37] = src.bytes[1];
            mRawData[dataOffset + 38] = src.bytes[2];
            mRawData[dataOffset + 39] = src.bytes[3];
            mRawData[dataOffset + 40] = src.bytes[4];
            mRawData[dataOffset + 41] = src.bytes[5];

            //Fill 0
            for (int i = 42; i < 243; i++)
            {
                mRawData[dataOffset + i] = 0x00;
            }

            //DHCP Magic cookie
            mRawData[dataOffset + 244] = 0x63;
            mRawData[dataOffset + 245] = 0x82;
            mRawData[dataOffset + 246] = 0x53;
            mRawData[dataOffset + 247] = 0x63;

            //options

            //Discover
            mRawData[dataOffset + 248] = 0x35;
            mRawData[dataOffset + 249] = 0x01;
            mRawData[dataOffset + 250] = 0x01;

            //Requested IP Address
            mRawData[dataOffset + 251] = 0x32;
            mRawData[dataOffset + 252] = 4;

            mRawData[dataOffset + 253] = requested_ip.address[0];
            mRawData[dataOffset + 254] = requested_ip.address[1];
            mRawData[dataOffset + 255] = requested_ip.address[2];
            mRawData[dataOffset + 256] = requested_ip.address[3];

            //Parameters start here
            mRawData[dataOffset + 257] = 0x37;
            mRawData[dataOffset + 258] = 4;

            //Parameters
            mRawData[dataOffset + 259] = 0x01;
            mRawData[dataOffset + 260] = 0x03;
            mRawData[dataOffset + 261] = 0x0f;
            mRawData[dataOffset + 262] = 0x06;

            mRawData[dataOffset + 263] = 0xff; //ENDMARK

            //Fill 0
            for (int i = 264; i < 272; i++)
            {
                mRawData[dataOffset + i] = 0x00;
            }

            initFields();
        }
    }
}
