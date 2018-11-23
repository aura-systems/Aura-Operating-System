using System;
using System.Collections.Generic;
using System.Text;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DNS Cache
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.IPV4.UDP.DNS
{
    public class DNSCache
    {
        static List<string> DNSName = new List<string>();
        static List<Address> DNSAddress = new List<Address>();

        public static Address IP;
        
        public static bool IsInCache(string name)
        {
            if (DNSName.Contains(name))
            {
                IP = DNSAddress[DNSName.IndexOf(name)];
                return true;
            }
            return false;
        }
    }
}
