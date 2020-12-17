using Aura_OS.HAL;
using Aura_OS.System.Network.IPV4;
using Aura_OS.System.Network.IPV4.UDP;
using System;
using System.Collections.Generic;
using System.Text;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP Packet
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.UDP.DHCP
{
    public class DHCPPacket : UDPPacket
    {

        protected byte messageType;
        protected Address yourClient;
        protected Address nextServer;

        int xID;

        public static void DHCPHandler(byte[] packetData)
        {
            DHCPPacket packet = new DHCPPacket(packetData);

            if (packet.messageType == 2) //Boot Reply
            {
                if (packet.RawData[284] == 0x02) //Offer packet received
                {
                    DHCPClient.SendRequestPacket(packet.yourClient, packet.nextServer);
                }
                else if (packet.RawData[284] == 0x05 || packet.RawData[284] == 0x06) //ACK or NAK DHCP packet received
                {
                    DHCPAck ack = new DHCPAck(packetData);
                    if (DHCPClient.DHCPAsked)
                    {
                        DHCPClient.Apply(ack, true);
                    }
                    else
                    {
                        DHCPClient.Apply(ack);
                    }
                }
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new DHCPPacket();
        }

        internal DHCPPacket() : base()
        { }

        internal DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        protected override void initFields()
        {
            base.initFields();
            messageType = mRawData[42];
            yourClient = new Address(mRawData, 58);
            nextServer = new Address(mRawData, 62);
        }

        internal DHCPPacket(MACAddress mac_src, ushort dhcpDataSize)
            : base(Address.Zero, Address.Broadcast, 68, 67, (ushort)(dhcpDataSize + 240), MACAddress.Broadcast)
        {
            //Request
            mRawData[42] = 0x01;

            //ethernet
            mRawData[43] = 0x01;

            //Length mac
            mRawData[44] = 0x06;

            //hops
            mRawData[45] = 0x00;

            Random rnd = new Random();
            xID = rnd.Next(0, Int32.MaxValue);
            mRawData[46] = (byte)((xID >> 24) & 0xFF);
            mRawData[47] = (byte)((xID >> 16) & 0xFF);
            mRawData[48] = (byte)((xID >> 8) & 0xFF);
            mRawData[49] = (byte)((xID >> 0) & 0xFF);

            //option bootp
            for (int i = 0; i < 20; i++)
            {
                mRawData[50 + i] = 0x00;
            }

            //Src mac
            mRawData[70] = mac_src.bytes[0];
            mRawData[71] = mac_src.bytes[1];
            mRawData[72] = mac_src.bytes[2];
            mRawData[73] = mac_src.bytes[3];
            mRawData[74] = mac_src.bytes[4];
            mRawData[75] = mac_src.bytes[5];

            //Fill 0
            for (int i = 0; i < 202; i++)
            {
                mRawData[76 + i] = 0x00;
            }

            //DHCP Magic cookie
            mRawData[278] = 0x63;
            mRawData[279] = 0x82;
            mRawData[280] = 0x53;
            mRawData[281] = 0x63;

            initFields();
        }

        internal byte MessageType
        {
            get { return this.messageType; }
        }

        internal Address Client
        {
            get { return this.yourClient; }
        }

        internal Address Server
        {
            get { return this.nextServer; }
        }

    }

    internal class DHCPDiscover : DHCPPacket
    {

        internal DHCPDiscover() : base()
        { }

        internal DHCPDiscover(byte[] rawData) : base(rawData)
        { }

        internal DHCPDiscover(MACAddress mac_src) : base(mac_src, 10) //discover packet size
        {
            //Discover
            mRawData[282] = 0x35;
            mRawData[283] = 0x01;
            mRawData[284] = 0x01;

            //Parameters start here
            mRawData[285] = 0x37;
            mRawData[286] = 4;

            //Parameters*
            mRawData[287] = 0x01;
            mRawData[288] = 0x03;
            mRawData[289] = 0x0f;
            mRawData[290] = 0x06;

            mRawData[291] = 0xff; //ENDMARK
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new DHCPDiscover();
        }

        protected override void initFields()
        {
            base.initFields();
        }

    }

    internal class DHCPRequest : DHCPPacket
    {

        internal DHCPRequest() : base()
        { }

        internal DHCPRequest(byte[] rawData) : base(rawData)
        { }

        internal DHCPRequest(MACAddress mac_src, Address RequestedAddress, Address DHCPServerAddress) : base(mac_src, 22)
        {
            //Request
            mRawData[282] = 53;
            mRawData[283] = 1;
            mRawData[284] = 3;

            //Requested Address
            mRawData[285] = 50;
            mRawData[286] = 4;

            mRawData[287] = RequestedAddress.address[0];
            mRawData[288] = RequestedAddress.address[1];
            mRawData[289] = RequestedAddress.address[2];
            mRawData[290] = RequestedAddress.address[3];

            mRawData[291] = 54;
            mRawData[292] = 4;

            mRawData[293] = DHCPServerAddress.address[0];
            mRawData[294] = DHCPServerAddress.address[1];
            mRawData[295] = DHCPServerAddress.address[2];
            mRawData[296] = DHCPServerAddress.address[3];

            //Parameters start here
            mRawData[297] = 0x37;
            mRawData[298] = 4;

            //Parameters
            mRawData[299] = 0x01;
            mRawData[300] = 0x03;
            mRawData[301] = 0x0f;
            mRawData[302] = 0x06;

            mRawData[303] = 0xff; //ENDMARK
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new DHCPRequest();
        }

        protected override void initFields()
        {
            base.initFields();
        }

    }

    internal class DHCPAck : DHCPPacket
    {

        protected Address subnetMask;
        protected Address domainNameServer;

        internal DHCPAck() : base()
        { }

        internal DHCPAck(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new DHCPRequest();
        }

        protected override void initFields()
        {
            base.initFields();
            subnetMask = new Address(mRawData, 311);
            domainNameServer = new Address(mRawData, 323);
        }

        internal Address Subnet
        {
            get { return this.subnetMask; }
        }

        internal Address DNS
        {
            get { return this.domainNameServer; }
        }

    }

    internal class DHCPRelease : DHCPPacket
    {

        internal DHCPRelease() : base()
        { }

        internal DHCPRelease(byte[] rawData) : base(rawData)
        { }

        internal DHCPRelease(Address source, Address dhcp_server) : base(MACAddress.None, 19)
        {
            //Release
            mRawData[282] = 0x35;
            mRawData[283] = 0x01;
            mRawData[284] = 0x07;

            //DHCP Server ID
            mRawData[285] = 0x36;
            mRawData[286] = 0x04;

            mRawData[287] = dhcp_server.address[0];
            mRawData[288] = dhcp_server.address[1];
            mRawData[289] = dhcp_server.address[2];
            mRawData[290] = dhcp_server.address[3];

            //Client ID
            mRawData[291] = 0x3d;
            mRawData[292] = 7;
            mRawData[293] = 1;

            mRawData[294] = SourceMAC.bytes[0];
            mRawData[295] = SourceMAC.bytes[1];
            mRawData[296] = SourceMAC.bytes[2];
            mRawData[297] = SourceMAC.bytes[3];
            mRawData[298] = SourceMAC.bytes[4];
            mRawData[299] = SourceMAC.bytes[5];

            mRawData[300] = 0xff; //ENDMARK
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new DHCPRelease();
        }

        protected override void initFields()
        {
            base.initFields();
        }

    }
}