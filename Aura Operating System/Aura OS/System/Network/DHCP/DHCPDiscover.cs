using System;
using Aura_OS.System.Network.IPV4;
using Aura_OS.HAL;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Discover Packet
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.DHCP
{
    public class DHCPDiscover : DHCPPacket
    {
        protected int xID;

        public static int PacketSize { get; set; }

        public DHCPDiscover(MACAddress mac_src)
            : base(Address.Zero, mac_src)
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
            mRawData[dataOffset + 36] = mac_src.bytes[0];
            mRawData[dataOffset + 37] = mac_src.bytes[1];
            mRawData[dataOffset + 38] = mac_src.bytes[2];
            mRawData[dataOffset + 39] = mac_src.bytes[3];
            mRawData[dataOffset + 40] = mac_src.bytes[4];
            mRawData[dataOffset + 41] = mac_src.bytes[5];

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

            //Parameters start here
            mRawData[dataOffset + 251] = 0x37;
            mRawData[dataOffset + 252] = 4;

            //Parameters
            mRawData[dataOffset + 253] = 0x01;
            mRawData[dataOffset + 254] = 0x03;
            mRawData[dataOffset + 255] = 0x0f;
            mRawData[dataOffset + 256] = 0x06;

            mRawData[dataOffset + 257] = 0xff; //ENDMARK

            //Fill 0
            //for (int i = 258; i < 272; i++)
            //{
            //    mRawData[dataOffset + i] = 0x00;
            //}

            initFields();
        }
    }
}
