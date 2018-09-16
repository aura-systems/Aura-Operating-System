using System;
using System.Collections.Generic;
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
        private static List<String> TCPFilterList = new List<String>();
        //from ip, port, incoming bool, outgoing bool
        //192.168.1.1, 25565, true, true

        public static bool Block_TCPIncomingPacket(IPV4.TCP.TCPPacket packet)
        {
            IPV4.Address IPSource = packet.SourceIP;
            ushort Port = packet.SourcePort;

            for (int i = 0; i < TCPFilterList.Count; i++)
            {
                if (TCPFilterList[i].Contains(IPSource.ToString() + "," + Port.ToString() + ",true"))
                {
                    return true;
                }
                if (TCPFilterList[i].Contains(IPSource.ToString() + ",*,true"))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Block_TCPOutgoingPacket(IPV4.TCP.TCPPacket packet)
        {
            IPV4.Address IPSource = packet.SourceIP;
            ushort Port = packet.SourcePort;

            for (int i = 0; i < TCPFilterList.Count; i++)
            {
                if (TCPFilterList[i].Contains(IPSource.ToString() + "," + Port.ToString()))
                {
                    string[] FilterList = TCPFilterList[i].Split(',');
                    bool OUTGOING = bool.Parse(FilterList[3]);

                    return OUTGOING;
                }
                if (TCPFilterList[i].Contains(IPSource.ToString() + ",*"))
                {
                    string[] FilterList = TCPFilterList[i].Split(',');
                    bool OUTGOING = bool.Parse(FilterList[3]);

                    return OUTGOING;
                }
            }
            return false;
        }
    }
}
