/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP (For local network discovering)
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.DHCP
{
    internal class DHCPPacket: IPV4.IPPacket
    {
        protected byte dhcpType;
        protected UInt16 dhcpLen;
        protected int xID;

        internal static void DHCPHandler(byte[] packetData)
        {
            DHCPPacket dhcp_packet = new DHCPPacket(packetData);
            Console.WriteLine("Received DHCP packet from " + dhcp_packet.SourceIP.ToString());
            Console.WriteLine("Content: " + Encoding.ASCII.GetString(dhcp_packet.mRawData));

            //Request or reply ?
            switch (dhcp_packet.DHCP_Type)
            {
                case 0x01:
                    //Boot Request (1)

                    break;

                case 0x02:
                    //Boot Reply (2)

                    break;
            }
        }

        internal byte DHCP_Type
        {
            get { return this.dhcpType; }
        }

        internal int Transaction_ID
        {
            get { return this.xID; }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new DHCPPacket();
        }

        internal DHCPPacket()
            : base()
        { }

        public DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        public DHCPPacket(IPV4.Address source, IPV4.Address dest, UInt16 srcPort, UInt16 destPort, byte[] data)
            : base((UInt16)(data.Length + 8), 2, source, dest)
        {
            //User Datagram Protocol
            //Source port
            mRawData[this.dataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);

            //Destination port
            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);
            
            //Length
            dhcpLen = (UInt16)(data.Length + 8);
            mRawData[this.dataOffset + 4] = (byte)((dhcpLen >> 8) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((dhcpLen >> 0) & 0xFF);
            
            //Checksum
            mRawData[this.dataOffset + 6] = 0;
            mRawData[this.dataOffset + 7] = 0;

            for (int b = 0; b < data.Length; b++)
            {
                mRawData[this.dataOffset + 8 + b] = data[b];
            }

            initFields();
        }

        protected override void initFields()
        {
            base.initFields();
            dhcpType = mRawData[dataOffset + 8]; //DHCP TYPE = Request or Reply
        }

        public override string ToString()
        {
            return "DHCP Packet Src=" + sourceIP + ", Dest=" + destIP + ", Type=" + dhcpType;
        }
    }

    internal class DHCPDiscoverRequest : DHCPPacket
    {
        public DHCPDiscoverRequest(IPV4.Address source, IPV4.Address dest, UInt16 srcPort, UInt16 destPort, byte[] data)
            : base(new IPV4.Address(0,0,0,0), new IPV4.Address(255, 255, 255, 255), 68, 67, data)
        {
            source = new IPV4.Address(0, 0, 0, 0);
            //Bootstrap Protocol
            //Message type
            mRawData[this.dataOffset + 8] = 0x01;

            mRawData[this.dataOffset + 9] = 0x01;
            mRawData[this.dataOffset + 10] = 6;
            mRawData[this.dataOffset + 11] = 0;

            //Transaction ID (xID)
            Random rnd = new Random();
            xID = rnd.Next(0, Int32.MaxValue);
            mRawData[this.dataOffset + 12] = (byte)((xID >> 24) & 0xFF);
            mRawData[this.dataOffset + 13] = (byte)((xID >> 16) & 0xFF);
            mRawData[this.dataOffset + 14] = (byte)((xID >> 8) & 0xFF);
            mRawData[this.dataOffset + 15] = (byte)((xID >> 0) & 0xFF);

            //Seconds elapsed
            mRawData[this.dataOffset + 16] = 0;
            mRawData[this.dataOffset + 17] = 0;

            //Bootp flags
            mRawData[this.dataOffset + 18] = 0;
            mRawData[this.dataOffset + 19] = 0;

            //Client IP address 0.0.0.0
            mRawData[this.dataOffset + 20] = source.ToByteArray()[0];
            mRawData[this.dataOffset + 21] = source.ToByteArray()[1];
            mRawData[this.dataOffset + 22] = source.ToByteArray()[2];
            mRawData[this.dataOffset + 23] = source.ToByteArray()[3];

            //Your (client) IP address 0.0.0.0
            mRawData[this.dataOffset + 24] = source.ToByteArray()[0];
            mRawData[this.dataOffset + 25] = source.ToByteArray()[1];
            mRawData[this.dataOffset + 26] = source.ToByteArray()[2];
            mRawData[this.dataOffset + 27] = source.ToByteArray()[3];

            //Next server IP address 0.0.0.0
            mRawData[this.dataOffset + 28] = 0;
            mRawData[this.dataOffset + 29] = 0;
            mRawData[this.dataOffset + 30] = 0;
            mRawData[this.dataOffset + 31] = 0;

            //Relay agent IP address 0.0.0.0
            mRawData[this.dataOffset + 32] = 0;
            mRawData[this.dataOffset + 33] = 0;
            mRawData[this.dataOffset + 34] = 0;
            mRawData[this.dataOffset + 35] = 0;

            //Client MAC address
            mRawData[this.dataOffset + 36] = srcMAC.bytes[0];
            mRawData[this.dataOffset + 37] = srcMAC.bytes[1];
            mRawData[this.dataOffset + 38] = srcMAC.bytes[2];
            mRawData[this.dataOffset + 39] = srcMAC.bytes[3];
            mRawData[this.dataOffset + 40] = srcMAC.bytes[4];
            mRawData[this.dataOffset + 41] = srcMAC.bytes[5];

            //Client hardware address padding & Server hostname & Boot file name
            for (int i = 42; i < 244; i++)
            {
                mRawData[this.dataOffset + i] = 0; //only 0 because its not given
            }

            //Magic cookie: DHCP
            mRawData[this.dataOffset + 245] = 0x63;
            mRawData[this.dataOffset + 246] = 0x82;
            mRawData[this.dataOffset + 247] = 0x53;
            mRawData[this.dataOffset + 248] = 0x63;

            //Option (53) DHCP Message Type
            mRawData[this.dataOffset + 249] = 0x35; //35 = 53
            mRawData[this.dataOffset + 250] = 1; //Length
            mRawData[this.dataOffset + 251] = 1; //DHCP Discover

            //Option (61) Client identifier
            mRawData[this.dataOffset + 252] = 7;
            mRawData[this.dataOffset + 253] = 1;
            mRawData[this.dataOffset + 254] = srcMAC.bytes[0];
            mRawData[this.dataOffset + 255] = srcMAC.bytes[1];
            mRawData[this.dataOffset + 256] = srcMAC.bytes[2];
            mRawData[this.dataOffset + 257] = srcMAC.bytes[3];
            mRawData[this.dataOffset + 258] = srcMAC.bytes[4];
            mRawData[this.dataOffset + 259] = srcMAC.bytes[5];

            //Option (50) Requested IP Address            
            mRawData[this.dataOffset + 260] = 4;
            mRawData[this.dataOffset + 261] = source.ToByteArray()[0];
            mRawData[this.dataOffset + 262] = source.ToByteArray()[1];
            mRawData[this.dataOffset + 263] = source.ToByteArray()[2];
            mRawData[this.dataOffset + 264] = source.ToByteArray()[3];

            //Option (12) Host name            
            mRawData[this.dataOffset + 265] = (byte) Computer.Info.HostnameLength();
            int a = 0;
            for (int i = 266; i < (266 + Computer.Info.HostnameLength() - 1); i++)
            {
                mRawData[this.dataOffset + i] = Computer.Info.getHostname()[a];
                a++;
            }
            


            for (int b = 0; b < data.Length; b++)
            {
                mRawData[this.dataOffset + 8 + b] = data[b];
            }

            initFields();
        }


    }
}
