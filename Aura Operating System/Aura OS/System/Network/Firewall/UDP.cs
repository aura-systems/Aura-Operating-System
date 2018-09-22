using System;
using System.Collections.Generic;
using System.Text;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Firewall - Core - UDP Filter
* PROGRAMMERS:      Alexy Da Cruz <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.Firewall
{
    class UDP
    {
        private static List<String> UDPFilterList = new List<String>();
        //from ip, port, incoming bool, outgoing bool
        //192.168.1.1, 25565, true, true

        public static bool Block_UDPIncomingPacket(IPV4.UDP.UDPPacket packet)
        {
            if (Core.Status())
            {
                IPV4.Address IPSource = packet.SourceIP;
                ushort Port = packet.SourcePort;

                for (int i = 0; i < UDPFilterList.Count; i++)
                {
                    if (UDPFilterList[i].Contains(IPSource.ToString() + "," + Port.ToString() + ",true"))
                    {
                        return true;
                    }
                    if (UDPFilterList[i].Contains(IPSource.ToString() + ",*,true"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool Block_UDPOutgoingPacket(IPV4.UDP.UDPPacket packet)
        {
            IPV4.Address IPSource = packet.DestinationIP;
            ushort Port = packet.DestinationPort;

            for (int i = 0; i < UDPFilterList.Count; i++)
            {
                if (UDPFilterList[i].Contains(IPSource.ToString() + "," + Port.ToString()))
                {
                    string[] FilterList = UDPFilterList[i].Split(',');
                    bool OUTGOING = bool.Parse(FilterList[3]);

                    return OUTGOING;
                }
                if (UDPFilterList[i].Contains(IPSource.ToString() + ",*"))
                {
                    string[] FilterList = UDPFilterList[i].Split(',');
                    bool OUTGOING = bool.Parse(FilterList[3]);

                    return OUTGOING;
                }
            }
            return false;
        }
    }
}
