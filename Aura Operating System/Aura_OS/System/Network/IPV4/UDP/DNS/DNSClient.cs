/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DNS Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.UDP.DNS
{
    class DnsClient : UdpClient
    {

        private string queryurl;

        public DnsClient() : base(53)
        {
        }

        public void Connect(Address address)
        {
            Connect(address, 53);
        }

        public void SendAsk(string url)
        {
            Address source = Config.FindNetwork(destination);

            queryurl = url;

            DNSPacketAsk askpacket = new DNSPacketAsk(source, destination, url);

            OutgoingBuffer.AddPacket(askpacket);

            NetworkStack.Update();
        }

        public Address Receive(int timeout = 5000)
        {
            int second = 0;
            int _deltaT = 0;

            while (rxBuffer.Count < 1)
            {
                if (second > (timeout / 1000))
                {
                    return null;
                }
                if (_deltaT != RTC.Second)
                {
                    second++;
                    _deltaT = RTC.Second;
                }
            }

            DNSPacketAnswer packet = new DNSPacketAnswer(rxBuffer.Dequeue().RawData);

            if ((ushort)(packet.DNSFlags & 0x0F) == (ushort)ReplyCode.OK)
            {
                if (packet.Queries.Count > 0 && packet.Queries[0].Name == queryurl)
                {
                    if (packet.Answers.Count > 0 && packet.Answers[0].Address.Length == 4)
                    {
                        return new Address(packet.Answers[0].Address, 0);
                    }
                }
            }
            return null;
        }

    }
}
