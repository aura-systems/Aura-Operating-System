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
    class DNSClient
    {

        public static bool DNSAsked = false;

        /// <summary>
        /// Send a packet to ask DNS server the IP from an url
        /// </summary>
        public static void SendAskPacket(Address dnsserver, string url)
        {
            Address source = IPV4.Config.FindNetwork(dnsserver);

            DNSPacketAsk askpacket = new DNSPacketAsk(source, dnsserver, url);

            OutgoingBuffer.AddPacket(askpacket);

            NetworkStack.Update();
        }

    }
}
