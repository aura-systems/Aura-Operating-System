using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.Network;

namespace Aura_OS.System.Network.Firewall
{
    class TCP
    {
        private static List<String> TCPFilterList = new List<String>();
        //from ip, port, incoming bool, outcoming bool
        //192.168.1.1, 25565, true, true

        public static bool TCPIncomingFilter(IPV4.TCP.TCPPacket packet)
        {
            IPV4.Address IPSource = packet.SourceIP;
            ushort Port = packet.SourcePort;

            for (int i = 0; i < TCPFilterList.Count; i++)
            {
                if (TCPFilterList[i].Contains(IPSource.ToString() + "," + Port.ToString() + ",true"))
                {
                    //string[] FilterList = TCPFilterList[i].Split(',');
                    //bool INCOMING = bool.Parse(FilterList[2]);
                    
                    return true;
                }
                if (TCPFilterList[i].Contains(IPSource.ToString() + ",*,true"))
                {
                    //string[] FilterList = TCPFilterList[i].Split(',');
                    //bool INCOMING = bool.Parse(FilterList[2]);

                    return true;
                }
            }
            return false;
        }
    }
}
