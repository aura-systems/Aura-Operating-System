/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Aura_OS.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.TCP
{

    /// <summary>
    /// DHCP Option
    /// </summary>
    public class TCPOption
    {
        public byte Kind { get; set; }
        public byte Length { get; set; }
        public byte[] Data { get; set; }
    }

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

        protected List<TCPOption> options = null;

        public bool SYN;
        public bool ACK;
        public bool FIN;
        public bool PSH;
        public bool RST;

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

        public TCPPacket(Address source, Address dest, ushort srcPort, ushort destPort,
            ulong sequencenumber, ulong acknowledgmentnb, ushort Headerlenght, ushort Flags,
            ushort WSValue, ushort UrgentPointer, ushort len)
            : base((ushort)(20 + len), 6, source, dest, 0x40)
        {
            //ports
            mRawData[this.dataOffset + 0] = (byte)((sourcePort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);

            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);

            //sequencenumber
            mRawData[this.dataOffset + 4] = (byte)((sequencenumber >> 24) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((sequencenumber >> 16) & 0xFF);
            mRawData[this.dataOffset + 6] = (byte)((sequencenumber >> 8) & 0xFF);
            mRawData[this.dataOffset + 7] = (byte)((sequencenumber >> 0) & 0xFF);

            //Acknowledgment number
            mRawData[this.dataOffset + 8] = (byte)((acknowledgmentnb >> 24) & 0xFF);
            mRawData[this.dataOffset + 9] = (byte)((acknowledgmentnb >> 16) & 0xFF);
            mRawData[this.dataOffset + 10] = (byte)((acknowledgmentnb >> 8) & 0xFF);
            mRawData[this.dataOffset + 11] = (byte)((acknowledgmentnb >> 0) & 0xFF);

            //Header lenght
            mRawData[this.dataOffset + 12] = (byte)((Headerlenght >> 0) & 0xFF);

            //Flags
            mRawData[this.dataOffset + 13] = (byte)((Flags >> 0) & 0xFF);

            //Window size value
            mRawData[this.dataOffset + 14] = (byte)((WSValue >> 8) & 0xFF);
            mRawData[this.dataOffset + 15] = (byte)((WSValue >> 0) & 0xFF);

            //Checksum
            mRawData[this.dataOffset + 16] = 0x00;
            mRawData[this.dataOffset + 17] = 0x00;

            //Urgent Pointer
            mRawData[this.dataOffset + 18] = (byte)((UrgentPointer >> 8) & 0xFF);
            mRawData[this.dataOffset + 19] = (byte)((UrgentPointer >> 0) & 0xFF);

            initFields();
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

            SYN = (mRawData[47] & (1 << 1)) != 0;
            ACK = (mRawData[47] & (1 << 4)) != 0;
            FIN = (mRawData[47] & (1 << 0)) != 0;
            PSH = (mRawData[47] & (1 << 3)) != 0;
            RST = (mRawData[47] & (1 << 2)) != 0;
        }

        internal ushort DestinationPort
        {
            get { return destinationPort; }
        }
        internal ushort SourcePort
        {
            get { return sourcePort; }
        }
        internal ulong SequenceNumber
        {
            get { return sequenceNumber; }
        }

        public override string ToString()
        {
            return "TCP Packet Src=" + sourceIP + ":" + sourcePort + ", Dest=" + destIP + ":" + destinationPort;
        }
    }

    public class TCPacketSyn : TCPPacket
    {
        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new TCPacketSyn();
        }

        internal TCPacketSyn()
            : base()
        { }

        public TCPacketSyn(Address Source, Address Destination, ushort SourcePort,
            ushort DestinationPort, ulong SequenceNumber, ulong ACKNumber,
            ushort Flags, ushort WSValue)
            : base(Source, Destination, SourcePort, DestinationPort, SequenceNumber,
                  ACKNumber, 0x50, Flags, WSValue, 0x0000, 0)
        {
            
        }

        protected override void initFields()
        {
            base.initFields();
        }
    }
}