using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aura_OS.System.Network;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Firewall - Core - TCP Filter
* PROGRAMMERS:      Alexy Da Cruz <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.Firewall
{
    class TCP
    {
        private static string[] TCPFilterList;
        //from ip, port, incoming bool, outgoing bool
        //192.168.1.1, 25565, true, true

        private static void LoadRules()
        {
            TCPFilterList = File.ReadAllLines(@"0:\System\firewall_tcp.conf");
        }

        public static bool Block_TCPIncomingPacket(IPV4.TCP.TCPPacket packet)
        {
            if (Core.Status())
            {
                LoadRules();
                IPV4.Address IPSource = packet.SourceIP;
                ushort Port = packet.SourcePort;

                for (int i = 0; i < TCPFilterList.Length; i++)
                {
                    if (TCPFilterList[i].Contains(IPSource.ToString() + ":" + Port.ToString() + ",true"))
                    {
                        return true;
                    }
                    if (TCPFilterList[i].Contains(IPSource.ToString() + ":*,true"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool Block_TCPOutgoingPacket(IPV4.TCP.TCPPacket packet)
        {
            if (Core.Status())
            {
                LoadRules();
                IPV4.Address IPDest = packet.DestinationIP;
                ushort Port = packet.DestinationPort;

                for (int i = 0; i < TCPFilterList.Length; i++)
                {
                    if (TCPFilterList[i].Contains(IPDest.ToString() + ":" + Port.ToString()))
                    {
                        string[] FilterList = TCPFilterList[i].Split(',');
                        bool OUTGOING = bool.Parse(FilterList[3]);

                        return OUTGOING;
                    }
                    if (TCPFilterList[i].Contains(IPDest.ToString() + ":*"))
                    {
                        string[] FilterList = TCPFilterList[i].Split(',');
                        bool OUTGOING = bool.Parse(FilterList[3]);

                        return OUTGOING;
                    }
                }
            }
            return false;
        }
    }
}
