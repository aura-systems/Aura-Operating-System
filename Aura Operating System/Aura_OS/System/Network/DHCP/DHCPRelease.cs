using System;
using Aura_OS.System.Network.IPV4;
using Aura_OS.HAL;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Release packet
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.DHCP
{
    public class DHCPRelease : DHCPPacket
    {
        protected int xID;

        public static int PacketSize { get; set; }

        public DHCPRelease(Address source, Address dhcp_server)
            : base(source, MACAddress.None)
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

            //sec elapsed & bootp
            mRawData[dataOffset + 16] = 0x00;
            mRawData[dataOffset + 17] = 0x00;
            mRawData[dataOffset + 18] = 0x00;
            mRawData[dataOffset + 19] = 0x00;

            //client IP addr
            mRawData[dataOffset + 20] = source.address[0];
            mRawData[dataOffset + 21] = source.address[1];
            mRawData[dataOffset + 22] = source.address[2];
            mRawData[dataOffset + 23] = source.address[3];

            //option bootp
            for (int i = 24; i < 35; i++)
            {
                mRawData[dataOffset + i] = 0x00;
            }

            //SourceMAC mac
            mRawData[dataOffset + 36] = SourceMAC.bytes[0];
            mRawData[dataOffset + 37] = SourceMAC.bytes[1];
            mRawData[dataOffset + 38] = SourceMAC.bytes[2];
            mRawData[dataOffset + 39] = SourceMAC.bytes[3];
            mRawData[dataOffset + 40] = SourceMAC.bytes[4];
            mRawData[dataOffset + 41] = SourceMAC.bytes[5];

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

            //Release
            mRawData[dataOffset + 248] = 0x35;
            mRawData[dataOffset + 249] = 0x01;
            mRawData[dataOffset + 250] = 0x07;

            //DHCP Server ID
            mRawData[dataOffset + 251] = 0x36;
            mRawData[dataOffset + 252] = 0x04;

            mRawData[dataOffset + 253] = dhcp_server.address[0];
            mRawData[dataOffset + 254] = dhcp_server.address[1];
            mRawData[dataOffset + 255] = dhcp_server.address[2];
            mRawData[dataOffset + 256] = dhcp_server.address[3];

            //Client ID
            mRawData[dataOffset + 257] = 0x3d;
            mRawData[dataOffset + 258] = 7;
            mRawData[dataOffset + 259] = 1;

            mRawData[dataOffset + 260] = SourceMAC.bytes[0];
            mRawData[dataOffset + 261] = SourceMAC.bytes[1];
            mRawData[dataOffset + 262] = SourceMAC.bytes[2];
            mRawData[dataOffset + 263] = SourceMAC.bytes[3];
            mRawData[dataOffset + 264] = SourceMAC.bytes[4];
            mRawData[dataOffset + 265] = SourceMAC.bytes[5];

            mRawData[dataOffset + 266] = 0xff; //ENDMARK

            //Fill 0
            //for (int i = 264; i < 272; i++)
            //{
            //    mRawData[dataOffset + i] = 0x00;
            //}

            initFields();
        }
    }
}
