/*
* PROJECT:          Aura Operating System Development
* CONTENT:          UDP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

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

            Kernel.debugger.Send("Received UDP packet from " + udp_packet.SourceIP.ToString() + ":" + udp_packet.SourcePort.ToString());

            if (CheckCRC(udp_packet.udpCRC, udp_packet))
            {
                Kernel.debugger.Send("Content: " + Encoding.ASCII.GetString(udp_packet.UDP_Data));
                UdpClient receiver = UdpClient.Client(udp_packet.DestinationPort);
                if (receiver != null)
                {
                    Kernel.debugger.Send("UDP Packet is for registered client");
                    receiver.receiveData(udp_packet);
                }
            }
            else
            {
                Kernel.debugger.Send("But checksum Incorrect... Packet Passed.");
            }
        }

        public static bool CheckCRC(ushort udpCRC, UDPPacket packet)
        {
            byte[] header = new byte[18 + packet.UDP_Data.Length];

            header[0] = packet.sourceIP.address[0];
            header[1] = packet.sourceIP.address[1];
            header[2] = packet.sourceIP.address[2];
            header[3] = packet.sourceIP.address[3];

            header[4] = packet.destIP.address[0];
            header[5] = packet.destIP.address[1];
            header[6] = packet.destIP.address[2];
            header[7] = packet.destIP.address[3];

            header[8] = 0x00;

            header[9] = 0x11;

            header[10] = (byte)((packet.udpLen >> 8) & 0xFF);
            header[11] = (byte)((packet.udpLen >> 0) & 0xFF);

            header[12] = (byte)((packet.sourcePort >> 8) & 0xFF);
            header[13] = (byte)((packet.sourcePort >> 0) & 0xFF);

            header[14] = (byte)((packet.destPort >> 8) & 0xFF);
            header[15] = (byte)((packet.destPort >> 0) & 0xFF);

            header[16] = (byte)((packet.udpLen >> 8) & 0xFF);
            header[17] = (byte)((packet.udpLen >> 0) & 0xFF);

            for (int i = 0; i < packet.UDP_Data.Length; i++)
            {
                header[18 + i] = packet.UDP_Data[i];
            }

            UInt16 calculatedcrc = Check(header, 0, header.Length);
            Kernel.debugger.Send("Calculated: 0x" + Utils.Conversion.DecToHex(calculatedcrc));
            Kernel.debugger.Send("Received:  0x" + Utils.Conversion.DecToHex(packet.udpCRC));
            if (calculatedcrc == packet.udpCRC)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected static UInt16 Check(byte[] buffer, UInt16 offset, int length)
        {
            UInt32 crc = 0;

            for (UInt16 w = offset; w < offset + length; w += 2)
            {
                crc += (UInt16)((buffer[w] << 8) | buffer[w + 1]);
            }

            crc = (~((crc & 0xFFFF) + (crc >> 16)));
            return (UInt16)crc;
            
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

        public UDPPacket(byte[] rawData)
            : base(rawData)
        {}

        public UDPPacket(Address source, Address dest, UInt16 srcPort, UInt16 destPort, byte[] data)
            : base((UInt16)(data.Length + 8), 17, source, dest)
        {
            mRawData[this.dataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);
            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);
            udpLen = (UInt16)(data.Length + 8);
            mRawData[this.dataOffset + 4] = (byte)((udpLen >> 8) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((udpLen >> 0) & 0xFF);
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
