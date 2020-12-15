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

        public static void DHCPHandler(byte[] packetData)
        {
            Kernel.debugger.Send("DHCP Handler called");
            DHCPPacket packet = new DHCPPacket(packetData);

            //TODO: check dhcp packet type
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

        internal DHCPPacket(byte[] rawData)
            : base(rawData)
        {
        }
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
            //TODO le reste
        }

        internal DHCPPacket(Address source, Address destination, MACAddress sourcemac, MACAddress destinationmac, byte sourceport, byte destinationport)
            : base(sourcemac, source, destination, sourceport, destinationport)
        {
            
            initFields();
        }

        //TODO Getter setter

    }

    // Other DHCP Packets that inherit DHCPPacket
}