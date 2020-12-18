/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DNS Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.UDP.DNS
{
    class DnsClient : BaseClient
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

        public Address Receive()
        {
            while (rxBuffer.Count < 1);

            DNSPacketAnswer packet = (DNSPacketAnswer)rxBuffer.Dequeue();

            if (packet.Queries.Count > 0 && packet.Queries[0].Name == queryurl)
            {
                if (packet.Answers.Count > 0 && packet.Answers[0].Address.Length == 4)
                {
                    return new Address(packet.Answers[0].Address, 0);
                }
            }
            return null;
        }

    }
}
