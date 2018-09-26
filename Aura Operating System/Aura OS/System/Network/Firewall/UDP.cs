using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aura_OS.System.Network;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Firewall - Core - UDP Filter
* PROGRAMMERS:      Alexy Da Cruz <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.Firewall
{
    class UDP
    {
        private static string[] UDPFilterList;
        //from ip, port, incoming bool, outgoing bool
        //192.168.1.1, 25565, true, true

        private static void LoadRules()
        {
            UDPFilterList = File.ReadAllLines(@"0:\System\firewall_udp.conf");
        }

        public static bool Block_UDPIncomingPacket(IPV4.UDP.UDPPacket packet)
        {
            if (Core.Status())
            {
                LoadRules();
                IPV4.Address IPSource = packet.SourceIP;
                ushort Port = packet.SourcePort;

                for (int i = 0; i < UDPFilterList.Length; i++)
                {
                    if (UDPFilterList[i].Contains(IPSource.ToString() + ":" + Port.ToString() + ",true"))
                    {
                        return true;
                    }
                    if (UDPFilterList[i].Contains(IPSource.ToString() + ":*,true"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool Block_UDPOutgoingPacket(IPV4.UDP.UDPPacket packet)
        {
            if (Core.Status())
            {
                LoadRules();
                IPV4.Address IPDest = packet.DestinationIP;
                ushort Port = packet.DestinationPort;

                for (int i = 0; i < UDPFilterList.Length; i++)
                {
                    if (UDPFilterList[i].Contains(IPDest.ToString() + ":" + Port.ToString()))
                    {
                        string[] FilterList = UDPFilterList[i].Split(',');
                        bool OUTGOING = bool.Parse(FilterList[3]);

                        return OUTGOING;
                    }
                    if (UDPFilterList[i].Contains(IPDest.ToString() + ":*"))
                    {
                        string[] FilterList = UDPFilterList[i].Split(',');
                        bool OUTGOING = bool.Parse(FilterList[3]);

                        return OUTGOING;
                    }
                }
            }
            return false;
        }
    }
}
