/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Text;

namespace Aura_OS.System.Network.IPV4.TCP
{
    public class TCPPacket : IPPacket
    {

        protected UInt16 sourcePort;
        protected UInt16 destPort;
        protected UInt16 tcpLen;
        protected UInt16 tcpCRC;
        ulong sequencenumber; ulong acknowledgmentnb; int Headerlenght; int Flags; int WSValue; int Checksum; int UrgentPointer;

        static int datalen = 0;

        internal static void TCPHandler(byte[] packetData)
        {

            TCPPacket tcp_packet = new TCPPacket(packetData);

            if (Firewall.TCP.TCPIncomingFilter(tcp_packet))
            {
                Apps.System.Debugger.debugger.Send("=== [FIREWALL] TCP INCOMING PACKET BLOCKED " + tcp_packet.SourceIP.ToString() + ":" + tcp_packet.SourcePort.ToString() + " ===");
                return;
            }

            foreach (HAL.Drivers.Network.NetworkDevice device in NetworkConfig.Keys)
            {
                if (device.Name == "PCNETII")
                {
                    if (packetData.Length == 64 && tcp_packet.ipLength < 50)
                    {
                        byte[] rdata = tcp_packet.mRawData;
                        int lenght = 64 - (64 - (tcp_packet.ipLength + 14));
                        tcp_packet.tcpLen = (ushort)(lenght - 34);
                        tcp_packet.mRawData = new byte[lenght];
                        for (int b = 0; b <= lenght; b++)
                        {
                            tcp_packet.mRawData[b] = rdata[b];
                        }
                    }
                }
            }

            Apps.System.Debugger.debugger.Send("=== Received TCP packet from " + tcp_packet.SourceIP.ToString() + ":" + tcp_packet.SourcePort.ToString() + " Len:" + tcp_packet.tcpLen.ToString() + "===");

            if (CheckCRC(tcp_packet))
            {

                bool SYN = (packetData[47] & (1 << 1)) != 0;
                bool ACK = (packetData[47] & (1 << 4)) != 0;
                bool FIN = (packetData[47] & (1 << 0)) != 0;
                bool PSH = (packetData[47] & (1 << 3)) != 0;
                bool RST = (packetData[47] & (1 << 2)) != 0;

                ulong CID = tcp_packet.SourceIP.Hash + tcp_packet.sourcePort + tcp_packet.destPort;

                TCPConnection.Connection connection = new TCPConnection.Connection();

                if (SYN && !ACK)
                {

                    Apps.System.Debugger.debugger.Send("FLAG: SYN, New connection");

                    if (TCPConnection.Connections.ContainsKey((uint)CID))
                    {
                        TCPConnection.Connections.Remove((uint)CID);
                    }
                    else
                    {
                        connection = new TCPConnection.Connection();
                    }

                    TCPConnection.Connections.Add((uint)CID, connection);

                    connection.CID = CID;

                    connection.dest = tcp_packet.sourceIP;
                    connection.source = tcp_packet.destIP;

                    connection.localPort = tcp_packet.destPort;
                    connection.destPort = tcp_packet.sourcePort;

                    connection.acknowledgmentnb = tcp_packet.sequencenumber + 1;

                    connection.WSValue = 1024;

                    connection.sequencenumber = 3455719727;

                    connection.Checksum = 0x0000;

                    connection.Flags = 0x12;

                    connection.Send(false);

                    connection.IsOpen = true;

                    return;
                }
                else if (SYN && ACK)
                {
                    connection.dest = tcp_packet.sourceIP;
                    connection.source = tcp_packet.destIP;

                    connection.localPort = tcp_packet.destPort;
                    connection.destPort = tcp_packet.sourcePort;

                    connection.acknowledgmentnb = tcp_packet.sequencenumber + 1;

                    connection.WSValue = 1024;

                    connection.sequencenumber = tcp_packet.acknowledgmentnb;

                    connection.Checksum = 0x0000;

                    connection.Flags = 0x10;

                    connection.Send(false);

                    TCPClient.lastack = tcp_packet.sequencenumber + 1;
                    TCPClient.lastsn = tcp_packet.acknowledgmentnb;

                    TCPConnection.Connections[(uint)CID].IsOpen = true;
                }
                else if ((FIN || RST) && ACK && TCPConnection.Connections[(uint)CID].IsOpen)
                {
                    Apps.System.Debugger.debugger.Send("FLAG: FIN, ACK, Disconnected by host!!!");

                    if (tcp_packet.sourcePort == 4224 && tcp_packet.destPort == 4224)
                    {
                        Console.WriteLine("Debugger closed by client computer!");
                        Kernel.debugger.Stop();
                    }

                    connection.dest = tcp_packet.sourceIP;
                    connection.source = tcp_packet.destIP;

                    connection.localPort = tcp_packet.destPort;
                    connection.destPort = tcp_packet.sourcePort;

                    connection.acknowledgmentnb = tcp_packet.sequencenumber + 1;

                    connection.WSValue = 1024;

                    connection.sequencenumber = tcp_packet.acknowledgmentnb;

                    connection.Checksum = 0x0000;

                    connection.Flags = 0x10;

                    connection.Send(false);

                    connection.Flags = 0x11;

                    connection.Send(false);

                    connection.Close();

                    return;
                }
                else if (PSH && ACK && TCPConnection.Connections[(uint)CID].IsOpen)
                {
                    Apps.System.Debugger.debugger.Send("FLAG: PSH, ACK, Data received!? :D");
                    TCPClient receiver = TCPClient.Client(tcp_packet.DestinationPort);
                    if (receiver != null)
                    {
                        receiver.receiveData(tcp_packet);
                    }

                    connection.dest = tcp_packet.sourceIP;
                    connection.source = tcp_packet.destIP;

                    connection.localPort = tcp_packet.destPort;
                    connection.destPort = tcp_packet.sourcePort;

                    connection.acknowledgmentnb = tcp_packet.sequencenumber + (ulong)tcp_packet.TCP_Data.Length;

                    connection.WSValue = 1024;

                    connection.sequencenumber = tcp_packet.acknowledgmentnb;

                    connection.Checksum = 0x0000;

                    connection.Flags = 0x10;

                    connection.Send(false);

                    connection.IsOpen = false;

                    return;
                }
                else if (RST && ACK)
                {
                    Apps.System.Debugger.debugger.Send("FLAG: RST, ACK, Port not listening on remote machine?");
                    return;
                }
                else if (RST)
                {
                    Apps.System.Debugger.debugger.Send("FLAG: RST, Connection aborted by host.");
                    return;
                }
                else if (ACK && TCPConnection.Connections[(uint)CID].IsOpen)
                {
                    TCPClient.lastack = tcp_packet.sequencenumber;
                    TCPClient.lastsn = tcp_packet.acknowledgmentnb;
                    TCPClient.readytosend = true;
                    return;
                }
                else
                {
                    Apps.System.Debugger.debugger.Send("What is going wrong? D:");
                }
            }
            else
            {
                Apps.System.Debugger.debugger.Send("But checksum is incorrect... Packet Passed.");

                TCPConnection.Connection connection = new TCPConnection.Connection();

                connection.dest = tcp_packet.sourceIP;
                connection.source = tcp_packet.destIP;

                connection.localPort = tcp_packet.destPort;
                connection.destPort = tcp_packet.sourcePort;

                connection.acknowledgmentnb = 0x0000;

                connection.WSValue = 1024;

                connection.sequencenumber = tcp_packet.acknowledgmentnb;

                connection.Checksum = 0x0000;

                connection.Flags = 0x04;

                connection.Send(false);

                connection.Close();

                return;
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
        {
        }

        public TCPPacket(Address source, Address dest, UInt16 srcPort, UInt16 destPort, byte[] data, ulong sequencenumber, ulong acknowledgmentnb, UInt16 Headerlenght, UInt16 Flags, UInt16 WSValue, UInt16 UrgentPointer, bool nodata, bool Sync)
            : base((UInt16)(20 + data.Length), 0x06, source, dest, 0x40)
        {
            mRawData[this.dataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);

            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);

            if (!nodata)
            {
                tcpLen = (UInt16)(data.Length + 20);
            }
            else
            {
                if (Sync)
                {
                    tcpLen = (UInt16)(20 + 4);
                }
                else
                {
                    tcpLen = (UInt16)(data.Length +20);
                }
            }

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

            if (Sync)
            {
                //Header lenght
                Headerlenght = 0x60;
                mRawData[this.dataOffset + 12] = (byte)((Headerlenght >> 0) & 0xFF);

                byte[] options = new byte[4];
                options[0] = 0x02;
                options[1] = 0x04;
                options[2] = 0x05;
                options[3] = 0xb4;

                for (int b = 0; b < options.Length; b++)
                {
                    mRawData[this.dataOffset + 20 + b] = options[b];
                }

                byte[] header = MakeHeader(source.address, dest.address, tcpLen, srcPort, destPort, sequencenumber, acknowledgmentnb, Headerlenght, Flags, WSValue, UrgentPointer, options, false);
                UInt16 calculatedcrc = Check(header, 0, header.Length);

                mRawData[this.dataOffset + 16] = (byte)((calculatedcrc >> 8) & 0xFF);
                mRawData[this.dataOffset + 17] = (byte)((calculatedcrc >> 0) & 0xFF);
            }
            else
            {
                byte[] header = MakeHeader(source.address, dest.address, tcpLen, srcPort, destPort, sequencenumber, acknowledgmentnb, Headerlenght, Flags, WSValue, UrgentPointer, data, false);
                UInt16 calculatedcrc = Check(header, 0, header.Length);

                //Console.WriteLine("0x" + Utils.Conversion.DecToHex(calculatedcrc));

                mRawData[this.dataOffset + 16] = (byte)((calculatedcrc >> 8) & 0xFF);
                mRawData[this.dataOffset + 17] = (byte)((calculatedcrc >> 0) & 0xFF);
            }

            if (!nodata)
            {
                for (int b = 0; b < data.Length; b++)
                {
                    mRawData[this.dataOffset + 20 + b] = data[b];
                }
            }
            initFields();
        }

        protected override void initFields()
        {
            base.initFields();

            sourcePort = (UInt16)((mRawData[dataOffset] << 8) | mRawData[dataOffset + 1]);
            destPort = (UInt16)((mRawData[dataOffset + 2] << 8) | mRawData[dataOffset + 3]);
            sequencenumber = (uint)((mRawData[dataOffset + 4] << 24) | (mRawData[dataOffset + 5] << 16) | (mRawData[dataOffset + 6] << 8) | mRawData[dataOffset + 7]);
            acknowledgmentnb = (uint)((mRawData[dataOffset + 8] << 24) | (mRawData[dataOffset + 9] << 16) | (mRawData[dataOffset + 10] << 8) | mRawData[dataOffset + 11]);
            Headerlenght = mRawData[dataOffset + 12];
            Flags = mRawData[dataOffset + 13];
            WSValue = (UInt16)((mRawData[dataOffset + 14] << 8) | mRawData[dataOffset + 15]);
            Checksum = (UInt16)((mRawData[dataOffset + 16] << 8) | mRawData[dataOffset + 17]);
            UrgentPointer = (UInt16)((mRawData[dataOffset + 18] << 8) | mRawData[dataOffset + 19]);
            tcpLen = (UInt16)(mRawData.Length - (34)); //34 TCP Begin 54TCP end
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

        public static byte[] MakeHeader(byte[] sourceIP, byte[] destIP, UInt16 tcpLen, UInt16 sourcePort, UInt16 destPort, ulong sequencenumber, ulong acknowledgmentnb, int Headerlenght, int Flags, int WSValue, int UrgentPointer, byte[] TCP_Data, bool nodata)
        {

            byte[] header;

            if (!nodata)
            {
                header = new byte[30 + TCP_Data.Length];
            }
            else
            {
                header = new byte[30];
            }
            
            header[0] = sourceIP[0];
            header[1] = sourceIP[1];
            header[2] = sourceIP[2];
            header[3] = sourceIP[3];

            header[4] = destIP[0];
            header[5] = destIP[1];
            header[6] = destIP[2];
            header[7] = destIP[3];

            header[8] = 0x00;

            header[9] = 0x06;

            header[10] = (byte)((tcpLen >> 8) & 0xFF);
            header[11] = (byte)((tcpLen >> 0) & 0xFF);

            header[12] = (byte)((sourcePort >> 8) & 0xFF);
            header[13] = (byte)((sourcePort >> 0) & 0xFF);

            header[14] = (byte)((destPort >> 8) & 0xFF);
            header[15] = (byte)((destPort >> 0) & 0xFF);

            //sequencenumber
            header[16] = (byte)((sequencenumber >> 24) & 0xFF);
            header[17] = (byte)((sequencenumber >> 16) & 0xFF);
            header[18] = (byte)((sequencenumber >> 8) & 0xFF);
            header[19] = (byte)((sequencenumber >> 0) & 0xFF);

            //Acknowledgment number
            header[20] = (byte)((acknowledgmentnb >> 24) & 0xFF);
            header[21] = (byte)((acknowledgmentnb >> 16) & 0xFF);
            header[22] = (byte)((acknowledgmentnb >> 8) & 0xFF);
            header[23] = (byte)((acknowledgmentnb >> 0) & 0xFF);

            //Header lenght
            header[24] = (byte)((Headerlenght >> 0) & 0xFF);

            //Flags
            header[25] = (byte)((Flags >> 0) & 0xFF);

            //Window size value
            header[26] = (byte)((WSValue >> 8) & 0xFF);
            header[27] = (byte)((WSValue >> 0) & 0xFF);

            //Urgent Pointer
            header[28] = (byte)((UrgentPointer >> 8) & 0xFF);
            header[29] = (byte)((UrgentPointer >> 0) & 0xFF);

            if (!nodata)
            {
                for (int i = 0; i < TCP_Data.Length; i++)
                {
                    header[30 + i] = TCP_Data[i];
                }
            }
            

            return header;
        }

        public static bool CheckCRC(TCPPacket packet)
        {
            byte[] header = MakeHeader(packet.sourceIP.address, packet.destIP.address, packet.tcpLen, packet.sourcePort, packet.destPort, (uint)packet.sequencenumber, (uint)packet.acknowledgmentnb, packet.Headerlenght, packet.Flags, packet.WSValue, packet.UrgentPointer, packet.TCP_Data, false);
            UInt16 calculatedcrc = Check(header, 0, header.Length);
            Apps.System.Debugger.debugger.Send("Calculated checksum: 0x" + Utils.Conversion.DecToHex(calculatedcrc));
            Apps.System.Debugger.debugger.Send("Received checksum :  0x" + Utils.Conversion.DecToHex(packet.Checksum));
            if (calculatedcrc == packet.Checksum)
            {
                Apps.System.Debugger.debugger.Send("checksum ok!!");
                return true;
            }
            else
            {
                Apps.System.Debugger.debugger.Send("checksum wrong!!");
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

        public override string ToString()
        {
            return "TCP Packet Src=" + sourceIP + ":" + sourcePort + ", Dest=" + destIP + ":" + destPort + ", DataLen=" + TCP_DataLength;
        }
    }
}
