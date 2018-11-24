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
    public class DNSClient
    {
        public static bool ReceivedResponse = false;
        public static string DNS_Name;
        public static Address DNS_ReceivedIP;

        public static void GetName(string dns_name)
        {
            Utils.Settings settings = new Utils.Settings(@"0:\System\resolv.conf");
            Address primary_dns_server = Address.Parse(settings.Get("primary_dns"));
            Address source = Config.FindNetwork(primary_dns_server);

            DNSPacketAsk askpacket = new DNSPacketAsk(source, primary_dns_server, 0x1234, 0x0100, 1, dns_name);

            OutgoingBuffer.AddPacket(askpacket);
            NetworkStack.Update();
        }

        public static void DNSHandler(DNSPacketAnswer packet)
        {
            DNS_Name = packet.Name.ToString();
            DNS_ReceivedIP = packet.address;
            ReceivedResponse = false;
        }
    }
}
