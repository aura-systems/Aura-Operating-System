/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Aura_OS.HAL;
using System;
using System.Text;

namespace Aura_OS.System.Network.IPV4.TCP
{
    public class TCPPacket : IPPacket
    {
        protected ushort sourcePort;
        protected ushort destinationPort;
        protected ushort tcpLen;
        protected ulong sequenceNumber;
        protected ulong ackNumber;
        protected int headerLenght;
        protected int flags;
        protected int wsValue;
        protected int checksum;
        protected int urgentPointer;

        internal static void TCPHandler(byte[] packetData)
        {
            TCPPacket packet = new TCPPacket(packetData);

            Kernel.debugger.Send("[Received] TCP packet from " + packet.SourceIP.ToString() + ":" + packet.SourcePort.ToString());

            TcpClient receiver = TcpClient.Client(packet.DestinationPort);
            if (receiver != null)
            {
                receiver.receiveData(packet);
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new TCPPacket();
        }

        internal TCPPacket()
            : base()
        { }

        internal TCPPacket(byte[] rawData)
            : base(rawData)
        { }

        public TCPPacket(Address source, Address dest, UInt16 srcport, UInt16 destport, UInt16 datalength)
            : base((ushort)(datalength + 8), 17, source, dest, 0x00)
        {
            MakePacket(srcport, destport, datalength);
            initFields();
        }

        public TCPPacket(Address source, Address dest, UInt16 srcport, UInt16 destport, byte[] data)
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
            destinationPort = 0;
            sourcePort = 0;
        }

        protected override void initFields()
        {
            base.initFields();
            sourcePort = (ushort)((mRawData[dataOffset] << 8) | mRawData[dataOffset + 1]);
            destinationPort = (ushort)((mRawData[dataOffset + 2] << 8) | mRawData[dataOffset + 3]);
            sequenceNumber = (uint)((mRawData[dataOffset + 4] << 24) | (mRawData[dataOffset + 5] << 16) | (mRawData[dataOffset + 6] << 8) | mRawData[dataOffset + 7]);
            ackNumber = (uint)((mRawData[dataOffset + 8] << 24) | (mRawData[dataOffset + 9] << 16) | (mRawData[dataOffset + 10] << 8) | mRawData[dataOffset + 11]);
            headerLenght = mRawData[dataOffset + 12];
            flags = mRawData[dataOffset + 13];
            wsValue = (ushort)((mRawData[dataOffset + 14] << 8) | mRawData[dataOffset + 15]);
            checksum = (ushort)((mRawData[dataOffset + 16] << 8) | mRawData[dataOffset + 17]);
            urgentPointer = (ushort)((mRawData[dataOffset + 18] << 8) | mRawData[dataOffset + 19]);
            tcpLen = (ushort)(mRawData.Length - (34));
        }

        internal UInt16 DestinationPort
        {
            get { return this.destinationPort; }
        }
        internal UInt16 SourcePort
        {
            get { return this.sourcePort; }
        }

        public override string ToString()
        {
            return "TCP Packet Src=" + sourceIP + ":" + sourcePort + ", Dest=" + destIP + ":" + destinationPort;
        }
    }
}