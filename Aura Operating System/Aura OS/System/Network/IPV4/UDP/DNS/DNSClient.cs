/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DNS Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using Aura_OS.HAL.Drivers.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.UDP.DNS
{
    static class DNSClient
    {   
        public static bool Request(string name)
        {
            if (!DNSCache.Name.Contains(name))
            {
                Utils.Settings settings = new Utils.Settings(@"0:\System\" + NetworkInterfaces.Interface("eth0") + ".conf");
                Address gateway = Address.Parse(settings.Get("dns01"));
                Address source = Config.FindNetwork(gateway);

                DNSPacketAsk askpacket = new DNSPacketAsk(source, gateway, 0x1234, 0x0100, 1, name);

                OutgoingBuffer.AddPacket(askpacket);
                NetworkStack.Update();

                return false;
            }
            else
            {
                return true;
            }
        }

        public static void PacketReceived(DNSPacketAnswer DNS_Answer)
        {
            if(DNS_Answer.AnswerType == 1) //Record A
            {
                DNSCache.AddEntry(DNS_Answer.Name.ToString(), DNS_Answer.address);
            }   
            else if (DNS_Answer.AnswerType == 28) //Record AAAA (IPv6)
            {
                //not implemented because of IPv6 that is not yet available in Aura.
            }
        }
    }
}
