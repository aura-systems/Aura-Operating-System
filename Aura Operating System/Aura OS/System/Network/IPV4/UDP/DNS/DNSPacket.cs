/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DNS Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
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

        internal static void DNSandler(byte[] packetData)
        {
            DNSPacket dns_packet = new DNSPacket(packetData);

            DNSClient receiver = DNSClient.Client(dns_packet.DestinationPort);
            if (receiver != null)
            {
                receiver.receiveData(dns_packet);
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

        public DNSPacket(Address source, Address dest, ushort transactionID, ushort dnsflags, ushort questions, string url)
            : base((UInt16)(16 + Encoding.ASCII.GetBytes(url).Length + url.Split('.').Length + 8), 17, source, dest, 0x00)
        {

            mRawData[this.dataOffset + 0] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((destPort >> 0) & 0xFF);
            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);
            int udpLen = (UInt16)(16 + Encoding.ASCII.GetBytes(url).Length + url.Split('.').Length + 8);

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

            //for (int i = 0; i < mRawData.Length; i++)
            //{
            //    Console.WriteLine("0x" + Utils.Conversion.DecToHex(mRawmRawData[this.dataOffset + i]));
            //    Console.ReadKey();
            //}

            initFields();

        }

        protected override void initFields()
        {

            ReceivedIP = new Address(0, 0, 0, 0);

            base.initFields();
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
}
