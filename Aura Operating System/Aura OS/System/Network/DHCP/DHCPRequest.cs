using System;
using Aura_OS.System.Network.IPV4;
using Aura_OS.HAL;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Request packet
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.DHCP
{
    public class DHCPRequest : DHCPPacket
    {
        protected int xID;

        public static int PacketSize { get; set; }

        public DHCPRequest(MACAddress mac_src, Address RequestedAddress, Address DHCPServerAddress)
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

            //SourceMAC mac
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

            //Request
            mRawData[dataOffset + 248] = 53;
            mRawData[dataOffset + 249] = 1;
            mRawData[dataOffset + 250] = 3;

            //Requested Address
            mRawData[dataOffset + 251] = 50;
            mRawData[dataOffset + 252] = 4;
            
            mRawData[dataOffset + 253] = RequestedAddress.address[0];
            mRawData[dataOffset + 254] = RequestedAddress.address[1];
            mRawData[dataOffset + 255] = RequestedAddress.address[2];
            mRawData[dataOffset + 256] = RequestedAddress.address[3];

            mRawData[dataOffset + 257] = 54;
            mRawData[dataOffset + 258] = 4;

            mRawData[dataOffset + 259] = DHCPServerAddress.address[0];
            mRawData[dataOffset + 260] = DHCPServerAddress.address[1];
            mRawData[dataOffset + 261] = DHCPServerAddress.address[2];
            mRawData[dataOffset + 262] = DHCPServerAddress.address[3];

            //Parameters start here
            mRawData[dataOffset + 263] = 0x37;
            mRawData[dataOffset + 264] = 4;

            //Parameters
            mRawData[dataOffset + 265] = 0x01;
            mRawData[dataOffset + 266] = 0x03;
            mRawData[dataOffset + 267] = 0x0f;
            mRawData[dataOffset + 268] = 0x06;

            mRawData[dataOffset + 269] = 0xff; //ENDMARK

            initFields();
        }
    }
}
