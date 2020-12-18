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

            DNSPacketAsk askpacket = new DNSPacketAsk(source, destination, url);

            OutgoingBuffer.AddPacket(askpacket);

            NetworkStack.Update();
        }

        public string Receive()
        {
            while (rxBuffer.Count < 1) ;

            DNSPacket packet = (DNSPacket)rxBuffer.Dequeue();

            return packet.UDP_Data.ToString();
        }

    }
}
