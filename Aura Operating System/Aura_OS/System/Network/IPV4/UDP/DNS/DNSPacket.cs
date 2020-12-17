/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DNS Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.UDP.DNS
{
    public class DNSPacket : UDPPacket
    {
        protected ushort transactionID;
        protected ushort dNSFlags;
        protected ushort questions;
        protected ushort answerRRs;
        protected ushort authorityRRs;
        protected ushort additionalRRs;

        internal static void DNSHandler(byte[] packetData)
        {
            DNSPacket dns_packet = new DNSPacket(packetData);

            if (dns_packet.Questions == 1 && dns_packet.answerRRs == 1)
            {
                Console.WriteLine(dns_packet.ToString());
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new DNSPacket();
        }

        internal DNSPacket()
            : base()
        { }

        public DNSPacket(byte[] rawData)
            : base(rawData)
        { }

        public DNSPacket(Address source, Address dest, ushort urlnb, ushort len)
            : base(source, dest, 53, 53, (ushort)(len + 12))
        {
            Random rnd = new Random();
            byte transactionID = (byte)rnd.Next(0, Int32.MaxValue);
            mRawData[this.dataOffset + 8] = (byte)((transactionID >> 8) & 0xFF);
            mRawData[this.dataOffset + 9] = (byte)((transactionID >> 0) & 0xFF);

            mRawData[this.dataOffset + 10] = (byte)((0x0100 >> 8) & 0xFF);
            mRawData[this.dataOffset + 11] = (byte)((0x0100 >> 0) & 0xFF);

            mRawData[this.dataOffset + 12] = (byte)((urlnb >> 8) & 0xFF);
            mRawData[this.dataOffset + 13] = (byte)((urlnb >> 0) & 0xFF);

            mRawData[this.dataOffset + 14] = (byte)((0 >> 8) & 0xFF);
            mRawData[this.dataOffset + 15] = (byte)((0 >> 0) & 0xFF);

            mRawData[this.dataOffset + 16] = (byte)((0 >> 8) & 0xFF);
            mRawData[this.dataOffset + 17] = (byte)((0 >> 0) & 0xFF);

            mRawData[this.dataOffset + 18] = (byte)((0 >> 8) & 0xFF);
            mRawData[this.dataOffset + 19] = (byte)((0 >> 0) & 0xFF);

            initFields();
        }

        protected override void initFields()
        {
            base.initFields();
            transactionID = (UInt16)((mRawData[this.dataOffset + 8] << 8) | mRawData[this.dataOffset + 9]);
            dNSFlags = (UInt16)((mRawData[this.dataOffset + 10] << 8) | mRawData[this.dataOffset + 11]);
            questions = (UInt16)((mRawData[this.dataOffset + 12] << 8) | mRawData[this.dataOffset + 13]);
            answerRRs = (UInt16)((mRawData[this.dataOffset + 14] << 8) | mRawData[this.dataOffset + 15]);
            authorityRRs = (UInt16)((mRawData[this.dataOffset + 16] << 8) | mRawData[this.dataOffset + 17]);
            additionalRRs = (UInt16)((mRawData[this.dataOffset + 18] << 8) | mRawData[this.dataOffset + 19]);
        }

        internal ushort TransactionID
        {
            get { return this.transactionID; }
        }

        internal ushort DNSFlags
        {
            get { return this.dNSFlags; }
        }

        internal ushort Questions
        {
            get { return this.questions; }
        }

        public override string ToString()
        {
            return "DNS Packet Src=" + sourceIP + ":" + SourcePort + ", Dest=" + destIP + ":" + DestinationPort;
        }

    }
    public class DNSPacketAsk : DNSPacket
    {
        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new DNSPacketAsk();
        }

        internal DNSPacketAsk()
            : base()
        { }

        public DNSPacketAsk(byte[] rawData)
            : base(rawData)
        { }

        public DNSPacketAsk(Address source, Address dest, string url)
            : base(source, dest, 1, (ushort)(5 + url.Length + 1))
        {
            int b = 0;

            foreach (string item in url.Split('.'))
            {
                byte[] word = Encoding.ASCII.GetBytes(item);

                mRawData[this.dataOffset + 20 + b] = (byte)word.Length; //set word length

                b++;

                foreach (byte letter in word)
                {
                    mRawData[this.dataOffset + 20 + b] = letter;
                    b++;
                }

            }

            mRawData[this.dataOffset + 20 + b] = 0x00;

            mRawData[this.dataOffset + 20 + b + 1] = 0x00;
            mRawData[this.dataOffset + 20 + b + 2] = 0x01;

            mRawData[this.dataOffset + 20 + b + 3] = 0x00;
            mRawData[this.dataOffset + 20 + b + 4] = 0x01;
        }

        protected override void initFields()
        {
            base.initFields();
        }
    }
}