using System;
using System.Text;
using Sys = System;

namespace Aura_OS.System.Network.IPV4.TCP
{
    public class TCPPacket : IPPacket
    {
        protected UInt16 sourcePort;
        protected UInt16 destPort;
        protected UInt16 tcpLen;
        protected UInt16 tcpCRC;
        UInt16 sequencenumber; UInt16 acknowledgmentnb; UInt16 Headerlenght; UInt16 Flags; UInt16 WSValue; UInt16 Checksum; UInt16 UrgentPointer;

        internal static void TCPHandler(byte[] packetData)
        {
            TCPPacket tcp_packet = new TCPPacket(packetData);
            Kernel.debugger.Send("Received TCP packet from " + tcp_packet.SourceIP.ToString() + ":" + tcp_packet.SourcePort.ToString());

            Console.WriteLine("Source port: 0x" + Utils.Conversion.DecToHex(tcp_packet.sourcePort));
            Console.WriteLine("destPort: 0x" + Utils.Conversion.DecToHex(tcp_packet.destPort));
            Console.WriteLine("sequencenumber: 0x" + Utils.Conversion.DecToHex(tcp_packet.sequencenumber));
            Console.WriteLine("acknowledgmentnb: 0x" + Utils.Conversion.DecToHex(tcp_packet.acknowledgmentnb));
            Console.WriteLine("Headerlenght: 0x" + Utils.Conversion.DecToHex(tcp_packet.Headerlenght));
            Console.WriteLine("Flags: 0x" + Utils.Conversion.DecToHex(tcp_packet.Flags));
            Console.WriteLine("WSValue: 0x" + Utils.Conversion.DecToHex(tcp_packet.WSValue));
            Console.WriteLine("Checksum: 0x" + Utils.Conversion.DecToHex(tcp_packet.Checksum));
            Console.WriteLine("UrgentPointer: 0x" + Utils.Conversion.DecToHex(tcp_packet.UrgentPointer));

            //Kernel.debugger.Send("Content: " + Encoding.ASCII.GetString(tcp_packet.TCP_Data));
            TCPClient receiver = TCPClient.Client(tcp_packet.DestinationPort);
            if (receiver != null)
            {
                Kernel.debugger.Send("TCP Packet is for registered client");
                receiver.receiveData(tcp_packet);
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

        public TCPPacket(byte[] rawData)
            : base(rawData)
        {}

        public TCPPacket(Address source, Address dest, UInt16 srcPort, UInt16 destPort, byte[] data, UInt32 sequencenumber, UInt32 acknowledgmentnb, UInt16 Headerlenght, UInt16 Flags, UInt16 WSValue, UInt16 Checksum, UInt16 UrgentPointer)
            : base((UInt16)(data.Length + 20), 17, source, dest)
        {
            mRawData[this.dataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);

            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);

            tcpLen = (UInt16)(data.Length + 20);

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
            mRawData[this.dataOffset + 16] = (byte)((Checksum >> 8) & 0xFF);
            mRawData[this.dataOffset + 17] = (byte)((Checksum >> 0) & 0xFF);

            //Urgent Pointer
            mRawData[this.dataOffset + 18] = (byte)((UrgentPointer >> 8) & 0xFF);
            mRawData[this.dataOffset + 19] = (byte)((UrgentPointer >> 0) & 0xFF);


            for (int b = 0; b < data.Length; b++)
            {
                mRawData[this.dataOffset + 20 + b] = data[b];
            }

            initFields();
        }

        protected override void initFields()
        {
            base.initFields();

            sourcePort = (UInt16)((mRawData[this.dataOffset] << 8) | mRawData[this.dataOffset + 1]);
            destPort = (UInt16)((mRawData[this.dataOffset + 2] << 8) | mRawData[this.dataOffset + 3]);
            sequencenumber = (UInt16)((mRawData[this.dataOffset + 4] << 24) | (mRawData[this.dataOffset + 5] << 16) | (mRawData[this.dataOffset + 6] << 8) | mRawData[this.dataOffset + 7]);
            acknowledgmentnb = (UInt16)((mRawData[this.dataOffset + 8] << 24) | (mRawData[this.dataOffset + 9] << 16) | (mRawData[this.dataOffset + 10] << 8) | mRawData[this.dataOffset + 11]);
            Headerlenght = (UInt16)mRawData[this.dataOffset + 12];
            Flags = (UInt16)mRawData[this.dataOffset + 13];
            WSValue = (UInt16)((mRawData[this.dataOffset + 14] << 8) | mRawData[this.dataOffset + 15]);
            Checksum = (UInt16)((mRawData[this.dataOffset + 16] << 8) | mRawData[this.dataOffset + 17]);
            UrgentPointer = (UInt16)((mRawData[this.dataOffset + 18] << 8) | mRawData[this.dataOffset + 19]);
        }

        internal UInt16 DestinationPort
        {
            get { return this.destPort; }
        }
        internal UInt16 SourcePort
        {
            get { return this.sourcePort; }
        }
        internal UInt16 TCP_Length
        {
            get { return this.tcpLen; }
        }
        internal UInt16 TCP_DataLength
        {
            get { return (UInt16)(this.tcpLen - 20); }
        }
        internal byte[] TCP_Data
        {
            get
            {
                byte[] data = new byte[this.tcpLen - 20];

                for (int b = 0; b < data.Length; b++)
                {
                    data[b] = this.mRawData[this.dataOffset + 20 + b];
                }

                return data;
            }
        }

        public override string ToString()
        {
            return "TCP Packet Src=" + sourceIP + ":" + sourcePort + ", Dest=" + destIP + ":" + destPort + ", DataLen=" + TCP_DataLength;
        }
    }
}
