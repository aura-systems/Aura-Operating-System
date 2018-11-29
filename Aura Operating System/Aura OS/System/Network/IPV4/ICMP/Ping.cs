using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.ICMP
{
    class Ping
    {
        private static Utils.Timer timer = new Utils.Timer();

        public static void Send(Address destination, int count)
        {

        }

        public static void Send(string DNSname, int count)
        {
            UDP.DNS.DNSClient DNSRequest = new UDP.DNS.DNSClient(53);
            DNSRequest.Ask(DNSname);

            timer.Wait(4, DNSRequest.ReceivedResponse);

            DNSRequest.Close();
            Send(DNSRequest.address, count);
        }
    }
}
