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
            MessageType = mRawData[this.dataOffset];
            HardwareType = mRawData[this.dataOffset + 1];
            HardwareAddressLength = mRawData[this.dataOffset + 2];
            Hops = mRawData[this.dataOffset + 3];
            TransactionID = (uint)((mRawData[4] << 24) | (mRawData[5] << 16) | (mRawData[6] << 8) | mRawData[7]);
            SecondsElapsed = (ushort)((mRawData[this.dataOffset + 8] << 8) | mRawData[this.dataOffset + 9]);
            BootpFlags = (ushort)((mRawData[this.dataOffset + 10] << 8) | mRawData[this.dataOffset + 11]);
            ClientIp = new Address((byte)(mRawData[12] << 24), (byte)(mRawData[13] << 16), (byte)(mRawData[14] << 8), (byte)(mRawData[15]));
            dataOffset += 240;
            //TODO: le reste
        }

        internal DHCPPacket(MACAddress mac_src, UInt16 icmpDataSize)
            : base(Address.Zero, Address.Broadcast, 68, 67, icmpDataSize, MACAddress.Broadcast)
        {
            //Request
            mRawData[dataOffset] = 0x01;

            //ethernet
            mRawData[dataOffset + 1] = 0x01;

            //Length mac
            mRawData[dataOffset + 2] = 0x06;

            //hops
            mRawData[dataOffset + 3] = 0x00;

            Random rnd = new Random();
            xID = rnd.Next(0, Int32.MaxValue);
            mRawData[dataOffset + 4] = (byte)((xID >> 24) & 0xFF);
            mRawData[dataOffset + 5] = (byte)((xID >> 16) & 0xFF);
            mRawData[dataOffset + 6] = (byte)((xID >> 8) & 0xFF);
            mRawData[dataOffset + 7] = (byte)((xID >> 0) & 0xFF);

            //option bootp
            for (int i = 8; i < 27; i++)
            {
                mRawData[dataOffset + i] = 0x00;
            }

            //Src mac
            mRawData[dataOffset + 28] = mac_src.bytes[0];
            mRawData[dataOffset + 29] = mac_src.bytes[1];
            mRawData[dataOffset + 31] = mac_src.bytes[2];
            mRawData[dataOffset + 32] = mac_src.bytes[3];
            mRawData[dataOffset + 33] = mac_src.bytes[4];
            mRawData[dataOffset + 34] = mac_src.bytes[5];

            //Fill 0
            for (int i = 35; i < 236; i++)
            {
                mRawData[dataOffset + i] = 0x00;
            }

            //DHCP Magic cookie
            mRawData[dataOffset + 237] = 0x63;
            mRawData[dataOffset + 238] = 0x82;
            mRawData[dataOffset + 239] = 0x53;
            mRawData[dataOffset + 240] = 0x63;

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

        internal DHCPDiscover(MACAddress mac_src) : base(mac_src, 300) //discover packet size
        {
            //Discover
            mRawData[dataOffset] = 0x35;
            mRawData[dataOffset + 1] = 0x01;
            mRawData[dataOffset + 2] = 0x01;

            //Parameters start here
            mRawData[dataOffset + 3] = 0x37;
            mRawData[dataOffset + 4] = 4;

            //Parameters
            mRawData[dataOffset + 5] = 0x01;
            mRawData[dataOffset + 6] = 0x03;
            mRawData[dataOffset + 7] = 0x0f;
            mRawData[dataOffset + 8] = 0x06;

            mRawData[dataOffset + 9] = 0xff; //ENDMARK
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
            dataOffset += 10;
        }

    }

    // Other DHCP Packets that inherit DHCPPacket
}