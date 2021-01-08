/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ICMP Packet (to ping for exemple)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;

namespace Aura_OS.System.Network.IPV4
{
    public class ICMPPacket : IPPacket
    {
        protected byte icmpType;
        protected byte icmpCode;
        protected UInt16 icmpCRC;

        internal static void ICMPHandler(byte[] packetData)
        {
            Kernel.debugger.Send("ICMP Handler called");
            ICMPPacket icmp_packet = new ICMPPacket(packetData);
            switch (icmp_packet.ICMP_Type)
            {
                case 0:
                    ICMPClient receiver = ICMPClient.Client(icmp_packet.SourceIP.Hash);
                    if (receiver != null)
                    {
                        receiver.receiveData(icmp_packet);
                    }
                    Kernel.debugger.Send("Received ICMP Echo reply from " + icmp_packet.SourceIP.ToString());
                    break;
                case 8:
                    ICMPEchoRequest request = new ICMPEchoRequest(packetData);
                    ICMPEchoReply reply = new ICMPEchoReply(request);
                    Kernel.debugger.Send("Sending ICMP Echo reply to " + reply.DestinationIP.ToString());
                    OutgoingBuffer.AddPacket(reply);
                    NetworkStack.Update();
                    break;
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new ICMPPacket();
        }

        internal ICMPPacket()
            : base()
        { }

        internal ICMPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPPacket.initFields() called;");
            base.initFields();
            icmpType = RawData[this.dataOffset];
            icmpCode = RawData[this.dataOffset + 1];
            icmpCRC = (UInt16)((RawData[this.dataOffset + 2] << 8) | RawData[this.dataOffset + 3]);
        }

        internal ICMPPacket(Address source, Address dest, byte type, byte code, UInt16 id, UInt16 seq, UInt16 icmpDataSize)
            : base(icmpDataSize, 1, source, dest, 0x00)
        {
            RawData[this.dataOffset] = type;
            RawData[this.dataOffset + 1] = code;
            RawData[this.dataOffset + 2] = 0x00;
            RawData[this.dataOffset + 3] = 0x00;
            RawData[this.dataOffset + 4] = (byte)((id >> 8) & 0xFF);
            RawData[this.dataOffset + 5] = (byte)((id >> 0) & 0xFF);
            RawData[this.dataOffset + 6] = (byte)((seq >> 8) & 0xFF);
            RawData[this.dataOffset + 7] = (byte)((seq >> 0) & 0xFF);

            icmpCRC = CalcICMPCRC((UInt16)(icmpDataSize));

            RawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
            initFields();
        }

        protected UInt16 CalcICMPCRC(UInt16 length)
        {
            return CalcOcCRC(this.dataOffset, length);
        }

        internal byte ICMP_Type
        {
            get { return this.icmpType; }
        }
        internal byte ICMP_Code
        {
            get { return this.icmpCode; }
        }
        internal UInt16 ICMP_CRC
        {
            get { return this.icmpCRC; }
        }
        internal UInt16 ICMP_DataLength
        {
            get { return (UInt16)(this.DataLength - 8); }
        }

        internal byte[] GetICMPData()
        {
            byte[] data = new byte[ICMP_DataLength];

            for (int b = 0; b < ICMP_DataLength; b++)
            {
                data[b] = RawData[this.dataOffset + 8 + b];
            }

            return data;
        }

        public override string ToString()
        {
            return "ICMP Packet Src=" + sourceIP + ", Dest=" + destIP + ", Type=" + icmpType + ", Code=" + icmpCode;
        }
    }

    internal class ICMPEchoRequest : ICMPPacket
    {
        protected UInt16 icmpID;
        protected UInt16 icmpSequence;

        internal ICMPEchoRequest()
            : base()
        { }

        internal ICMPEchoRequest(byte[] rawData)
            : base(rawData)
        {
        }

        internal ICMPEchoRequest(Address source, Address dest, UInt16 id, UInt16 sequence)
            : base(source, dest, 8, 0, id, sequence, 40)
        {
            for (int b = 8; b < this.ICMP_DataLength; b++)
            {
                RawData[this.dataOffset + b] = (byte)b;
            }

            RawData[this.dataOffset + 2] = 0x00;
            RawData[this.dataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((UInt16)(this.ICMP_DataLength + 8));
            RawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new ICMPEchoRequest();
        }

        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPEchoRequest.initFields() called;");
            base.initFields();
            icmpID = (UInt16)((RawData[this.dataOffset + 4] << 8) | RawData[this.dataOffset + 5]);
            icmpSequence = (UInt16)((RawData[this.dataOffset + 6] << 8) | RawData[this.dataOffset + 7]);
        }

        internal UInt16 ICMP_ID
        {
            get { return this.icmpID; }
        }
        internal UInt16 ICMP_Sequence
        {
            get { return this.icmpSequence; }
        }

        public override string ToString()
        {
            return "ICMP Echo Request Src=" + sourceIP + ", Dest=" + destIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }
    internal class ICMPEchoReply : ICMPPacket
    {
        protected UInt16 icmpID;
        protected UInt16 icmpSequence;

        internal ICMPEchoReply()
            : base()
        { }

        internal ICMPEchoReply(byte[] rawData)
            : base(rawData)
        {
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new ICMPEchoReply();
        }

        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPEchoReply.initFields() called;");
            base.initFields();
            icmpID = (UInt16)((RawData[this.dataOffset + 4] << 8) | RawData[this.dataOffset + 5]);
            icmpSequence = (UInt16)((RawData[this.dataOffset + 6] << 8) | RawData[this.dataOffset + 7]);
        }

        internal ICMPEchoReply(ICMPEchoRequest request)
            : base(request.DestinationIP, request.SourceIP, 0, 0, request.ICMP_ID, request.ICMP_Sequence, (UInt16)(request.ICMP_DataLength))
        {
            for (int b = 0; b < this.ICMP_DataLength; b++)
            {
                RawData[this.dataOffset + 8 + b] = request.RawData[this.dataOffset + 8 + b];
            }

            RawData[this.dataOffset + 2] = 0x00;
            RawData[this.dataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((UInt16)(this.ICMP_DataLength + 8));
            RawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        internal UInt16 ICMP_ID
        {
            get { return this.icmpID; }
        }
        internal UInt16 ICMP_Sequence
        {
            get { return this.icmpSequence; }
        }

        public override string ToString()
        {
            return "ICMP Echo Reply Src=" + sourceIP + ", Dest=" + destIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }
}
