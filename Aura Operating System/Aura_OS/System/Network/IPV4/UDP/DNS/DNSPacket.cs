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

        public string Url;
        public Address ReceivedIP;

        internal static void DNSHandler(byte[] packetData)
        {
            DNSPacket dns_packet = new DNSPacket(packetData);

            if (dns_packet.Questions == 1 && dns_packet.answerRRs == 1)
            {
                Console.WriteLine("Received DNS answer!");
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

        public DNSPacket(Address source, Address dest, ushort len)
            : base(source, dest, 53, 53, len)
        {
            Random rnd = new Random();
            byte transactionID = (byte)rnd.Next(0, Int32.MaxValue);
            mRawData[this.dataOffset + 8] = (byte)((transactionID >> 8) & 0xFF);
            mRawData[this.dataOffset + 9] = (byte)((transactionID >> 0) & 0xFF);

            mRawData[this.dataOffset + 10] = (byte)((dNSFlags >> 8) & 0xFF);
            mRawData[this.dataOffset + 11] = (byte)((dNSFlags >> 0) & 0xFF);

            mRawData[this.dataOffset + 12] = (byte)((questions >> 8) & 0xFF);
            mRawData[this.dataOffset + 13] = (byte)((questions >> 0) & 0xFF);

            mRawData[this.dataOffset + 14] = (byte)((answerRRs >> 8) & 0xFF);
            mRawData[this.dataOffset + 15] = (byte)((answerRRs >> 0) & 0xFF);

            mRawData[this.dataOffset + 16] = (byte)((authorityRRs >> 8) & 0xFF);
            mRawData[this.dataOffset + 17] = (byte)((authorityRRs >> 0) & 0xFF);

            mRawData[this.dataOffset + 18] = (byte)((additionalRRs >> 8) & 0xFF);
            mRawData[this.dataOffset + 19] = (byte)((additionalRRs >> 0) & 0xFF);

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
            return "DNS Packet Src=" + sourceIP + ":" + 53 + ", Dest=" + destIP + ":" + 53 + ", URL=" + Url;
        }

    }
}