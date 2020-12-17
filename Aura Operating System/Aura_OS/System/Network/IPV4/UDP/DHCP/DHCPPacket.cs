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

        protected byte MessageType;
        protected byte HardwareType;
        protected byte HardwareAddressLength;
        protected byte Hops;
        protected uint TransactionID;
        protected ushort SecondsElapsed;
        protected ushort BootpFlags;
        protected Address ClientIp;
        protected Address YourClient;
        protected Address NextServer;
        protected Address RelayAgent;
        protected MACAddress Client;
        protected uint MagicCookie;
        int xID;

        public static void DHCPHandler(byte[] packetData)
        {
            Kernel.debugger.Send("DHCP Handler called");
            DHCPPacket packet = new DHCPPacket(packetData);

            if (packet.MessageType == 0x02)
            {
                Console.WriteLine("Offert packet received");
            }

            if (packet.MessageType == 0x05 || packet.MessageType == 0x06)
            {
                Console.WriteLine("ACK or NAK DHCP packet received");
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
            MessageType = mRawData[42];
            /*HardwareType = mRawData[this.dataOffset + 1];
            HardwareAddressLength = mRawData[this.dataOffset + 2];
            Hops = mRawData[this.dataOffset + 3];
            TransactionID = (uint)((mRawData[4] << 24) | (mRawData[5] << 16) | (mRawData[6] << 8) | mRawData[7]);
            SecondsElapsed = (ushort)((mRawData[this.dataOffset + 8] << 8) | mRawData[this.dataOffset + 9]);
            BootpFlags = (ushort)((mRawData[this.dataOffset + 10] << 8) | mRawData[this.dataOffset + 11]);
            ClientIp = new Address((byte)(mRawData[12] << 24), (byte)(mRawData[13] << 16), (byte)(mRawData[14] << 8), (byte)(mRawData[15]));*/
            //TODO: le reste
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

        //TODO Getter setter

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

    // Other DHCP Packets that inherit DHCPPacket
}