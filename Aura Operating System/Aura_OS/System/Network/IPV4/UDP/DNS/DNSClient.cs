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
            //DNSPacketAsk askpacket = new DNSPacketAsk(source, gateway, url);

            //OutgoingBuffer.AddPacket(askpacket);
            //NetworkStack.Update();
        }

    }
}
