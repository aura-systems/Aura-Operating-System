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
    public class DNSPacket : IPPacket
    {

        protected ushort TransactionID;
        protected ushort DNSFlags;
        protected ushort Questions;
        protected ushort AnswerRRs = 0x00;
        protected ushort AuthorityRRs = 0x00;
        protected ushort AdditionalRRs = 0x00;

        protected UInt16 destPort = 53;

        public string Url;
        public Address ReceivedIP;

        internal static void DNSHandler(byte[] packetData)
        {
            DNSPacket dns_packet = new DNSPacket(packetData);

            if (dns_packet.Questions == 1 && dns_packet.AnswerRRs == 1)
            {
                DNSPacketAnswer dns_packetanswer = new DNSPacketAnswer(packetData);

                DNSClient receiver = DNSClient.Client(dns_packetanswer.DestinationPort);
                if (receiver != null)
                {
                    receiver.receiveData(dns_packetanswer);
                }
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

        public DNSPacket(Address source, Address dest, ushort transactionID, ushort dnsflags, ushort questions, ushort len)
            : base(len, 17, source, dest, 0x00)
        {

            mRawData[this.dataOffset + 0] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((destPort >> 0) & 0xFF);
            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);
            int udpLen = len;

            mRawData[this.dataOffset + 4] = (byte)((udpLen >> 8) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((udpLen >> 0) & 0xFF);

            //byte[] header = MakeHeader(source.address, dest.address, udpLen, srcPort, destPort, data);
            //UInt16 calculatedcrc = Check(header, 0, header.Length);

            mRawData[this.dataOffset + 6] = (byte)((0 >> 8) & 0xFF);
            mRawData[this.dataOffset + 7] = (byte)((0 >> 0) & 0xFF);

            mRawData[this.dataOffset + 8] = (byte)((transactionID >> 8) & 0xFF);
            mRawData[this.dataOffset + 9] = (byte)((transactionID >> 0) & 0xFF);

            mRawData[this.dataOffset + 10] = (byte)((dnsflags >> 8) & 0xFF);
            mRawData[this.dataOffset + 11] = (byte)((dnsflags >> 0) & 0xFF);

            mRawData[this.dataOffset + 12] = (byte)((questions >> 8) & 0xFF);
            mRawData[this.dataOffset + 13] = (byte)((questions >> 0) & 0xFF);

            mRawData[this.dataOffset + 14] = (byte)((AnswerRRs >> 8) & 0xFF);
            mRawData[this.dataOffset + 15] = (byte)((AnswerRRs >> 0) & 0xFF);

            mRawData[this.dataOffset + 16] = (byte)((AuthorityRRs >> 8) & 0xFF);
            mRawData[this.dataOffset + 17] = (byte)((AuthorityRRs >> 0) & 0xFF);

            mRawData[this.dataOffset + 18] = (byte)((AdditionalRRs >> 8) & 0xFF);
            mRawData[this.dataOffset + 19] = (byte)((AdditionalRRs >> 0) & 0xFF);

            initFields();

        }

        protected override void initFields()
        {
            base.initFields();
            TransactionID = (UInt16)((mRawData[this.dataOffset + 8] << 8) | mRawData[this.dataOffset + 9]);
            DNSFlags = (UInt16)((mRawData[this.dataOffset + 10] << 8) | mRawData[this.dataOffset + 11]);
            Questions = (UInt16)((mRawData[this.dataOffset + 12] << 8) | mRawData[this.dataOffset + 13]);
            AnswerRRs = (UInt16)((mRawData[this.dataOffset + 14] << 8) | mRawData[this.dataOffset + 15]);
            AuthorityRRs = (UInt16)((mRawData[this.dataOffset + 16] << 8) | mRawData[this.dataOffset + 17]);
            AdditionalRRs = (UInt16)((mRawData[this.dataOffset + 18] << 8) | mRawData[this.dataOffset + 19]);
        }

        internal UInt16 DestinationPort
        {
            get { return this.destPort; }
        }

        public override string ToString()
        {
            return "DNS Packet Src=" + sourceIP + ":" + 53 + ", Dest=" + destIP + ":" + 53 + ", URL=" + Url;
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

        public DNSPacketAsk(Address source, Address dest, ushort transactionID, ushort dnsflags, ushort questions, string url)
            : base(source, dest, transactionID, dnsflags, questions, (UInt16)(16 + Encoding.ASCII.GetBytes(url).Length + url.Split('.').Length + 8))
        {
            string[] items = url.Split('.');

            int counter = 20;

            foreach (string item in items)
            {
                byte[] itembytes = Encoding.ASCII.GetBytes(item);

                mRawData[this.dataOffset + counter] = (byte)itembytes.Length;
                counter++;

                foreach (byte itembyte in itembytes)
                {
                    mRawData[this.dataOffset + counter] = itembyte;
                    counter++;
                }

            }

            mRawData[this.dataOffset + counter] = 0x00;

            mRawData[this.dataOffset + counter + 1] = 0x00;
            mRawData[this.dataOffset + counter + 2] = 0x01;

            mRawData[this.dataOffset + counter + 3] = 0x00;
            mRawData[this.dataOffset + counter + 4] = 0x01;

            Url = url;
        }

        protected override void initFields()
        {
            base.initFields();
        }
    }

    public class DNSPacketAnswer : DNSPacket
    {

        public string Querie;
        public ushort QuerieType;
        public ushort QuerieClass;

        public ushort Name;
        public ushort AnswerType;
        public ushort AnswerClass;
        public int TimeToLive;
        public ushort DataLenght;
        public Address address;

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new DNSPacketAnswer();
        }

        internal DNSPacketAnswer()
            : base()
        { }

        public DNSPacketAnswer(byte[] rawData)
            : base(rawData)
        { }

        protected override void initFields()
        {
            base.initFields();

            byte item = 0xFF;
            int counter = 0;
            while(item != 0x00)
            {
                item = mRawData[this.dataOffset + 20 + counter];
                counter++;
            }

            counter--;

            List<byte> letters = new List<byte>();

            for (int i = 0; i < counter; i++)
            {
                int intemlenght = mRawData[this.dataOffset + 20 + i];
                for (int h = 0; h < intemlenght; h++)
                {
                    letters.Add(mRawData[this.dataOffset + 21 + i]);
                    i++;
                }

                letters.Add(0x2E);
            }
            letters.RemoveAt(letters.Count - 1);

            Querie = Encoding.ASCII.GetString(letters.ToArray());

            counter++;

            QuerieType = (UInt16)((mRawData[this.dataOffset + 20 + counter + 0] << 8) | mRawData[this.dataOffset + 20 + counter + 1]);
            QuerieClass = (UInt16)((mRawData[this.dataOffset + 20 + counter + 2] << 8) | mRawData[this.dataOffset + 20 + counter + 3]);

            Name = (UInt16)((mRawData[this.dataOffset + 20 + counter + 4] << 8) | mRawData[this.dataOffset + 20 + counter + 5]);
            AnswerType = (UInt16)((mRawData[this.dataOffset + 20 + counter + 6] << 8) | mRawData[this.dataOffset + 20 + counter + 7]);
            AnswerClass = (UInt16)((mRawData[this.dataOffset + 20 + counter + 8] << 8) | mRawData[this.dataOffset + 20 + counter + 9]);
            TimeToLive = (mRawData[this.dataOffset + 20 + counter + 10] << 24) | (mRawData[this.dataOffset + 20 + counter + 11] << 16) | (mRawData[this.dataOffset + 20 + counter + 12] << 8) | mRawData[this.dataOffset + 20 + counter + 13];
            DataLenght = (UInt16)((mRawData[this.dataOffset + 20 + counter + 14] << 8) | mRawData[this.dataOffset + 20 + counter + 15]);
            address = new Address(mRawData, this.dataOffset + 20 + counter + 16);
        }

    }
}
