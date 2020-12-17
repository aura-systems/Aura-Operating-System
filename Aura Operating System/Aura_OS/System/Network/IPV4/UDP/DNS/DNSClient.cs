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
        public static void SendAskPacket(string url)
        {
            //Address dest = Address.Parse("192.168.1.1");
            //Address source = IPV4.Config.FindNetwork(dest);

            DNSPacketAsk askpacket = new DNSPacketAsk(Address.Parse("192.168.1.43"), Address.Parse("192.168.1.1"), url);

            OutgoingBuffer.AddPacket(askpacket);
            NetworkStack.Update();
        }

    }
}
