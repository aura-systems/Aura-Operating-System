/*
* PROJECT:          Aura Operating System Development
* CONTENT:          UDP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Aura_OS.HAL;
using System;
using System.Text;

namespace Aura_OS.System.Network.IPV4.UDP
{
    public class UDPPacket : IPPacket
    {
        protected UInt16 sourcePort;
        protected UInt16 destPort;
        protected UInt16 udpLen;
        protected UInt16 udpCRC;

        internal static void UDPHandler(byte[] packetData)
        {
            UDPPacket udp_packet = new UDPPacket(packetData);

            Kernel.debugger.Send("[Received] UDP packet from " + udp_packet.SourceIP.ToString() + ":" + udp_packet.SourcePort.ToString());

            if (udp_packet.SourcePort == 67)
            {
                Network.UDP.DHCP.DHCPPacket.DHCPHandler(packetData);
                return;
            }
            else if (udp_packet.SourcePort == 53)
            {
                //DNS.DNSPacket.DNSHandler(packetData);
                return;
            }

            UdpClient receiver = UdpClient.Client(udp_packet.DestinationPort);
            if (receiver != null)
            {
                receiver.receiveData(udp_packet);
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new UDPPacket();
        }

        internal UDPPacket()
            : base()
        { }

        internal UDPPacket(byte[] rawData)
            : base(rawData)
        {}

        public UDPPacket(Address source, Address dest, UInt16 srcport, UInt16 destport, UInt16 datalength, MACAddress destmac)
            : base((ushort)(datalength + 8), 17, source, dest, 0x00, destmac)
        {
            MakePacket(srcport, destport, datalength);
            initFields();
        }

        public UDPPacket(Address source, Address dest, UInt16 srcport, UInt16 destport, byte[] data)
            : base((ushort)(data.Length + 8), 17, source, dest, 0x00)
        {
            MakePacket(srcport, destport, (ushort)data.Length);

            for (int b = 0; b < data.Length; b++)
            {
                mRawData[this.dataOffset + 8 + b] = data[b];
            }

            initFields();
        }

        private void MakePacket(ushort srcport, ushort destport, ushort length)
        {
            mRawData[this.dataOffset + 0] = (byte)((srcport >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((srcport >> 0) & 0xFF);
            mRawData[this.dataOffset + 2] = (byte)((destport >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destport >> 0) & 0xFF);
            udpLen = (ushort)(length + 8);

            mRawData[this.dataOffset + 4] = (byte)((udpLen >> 8) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((udpLen >> 0) & 0xFF);

            mRawData[this.dataOffset + 6] = (byte)((0 >> 8) & 0xFF);
            mRawData[this.dataOffset + 7] = (byte)((0 >> 0) & 0xFF);
        }

        protected override void initFields()
        {
            base.initFields();
            sourcePort = (UInt16)((mRawData[this.dataOffset] << 8) | mRawData[this.dataOffset + 1]);
            destPort = (UInt16)((mRawData[this.dataOffset + 2] << 8) | mRawData[this.dataOffset + 3]);
            udpLen = (UInt16)((mRawData[this.dataOffset + 4] << 8) | mRawData[this.dataOffset + 5]);
            udpCRC = (UInt16)((mRawData[this.dataOffset + 6] << 8) | mRawData[this.dataOffset + 7]);
        }

        internal UInt16 DestinationPort
        {
            get { return this.destPort; }
        }
        internal UInt16 SourcePort
        {
            get { return this.sourcePort; }
        }
        internal UInt16 UDP_Length
        {
            get { return this.udpLen; }
        }
        internal UInt16 UDP_DataLength
        {
            get { return (UInt16)(this.udpLen - 8); }
        }
        internal byte[] UDP_Data
        {
            get
            {
                byte[] data = new byte[this.udpLen - 8];

                for (int b = 0; b < data.Length; b++)
                {
                    data[b] = this.mRawData[this.dataOffset + 8 + b];
                }

                return data;
            }
        }

        public override string ToString()
        {
            return "UDP Packet Src=" + sourceIP + ":" + sourcePort + ", Dest=" + destIP + ":" + destPort + ", DataLen=" + UDP_DataLength;
        }
    }
}
