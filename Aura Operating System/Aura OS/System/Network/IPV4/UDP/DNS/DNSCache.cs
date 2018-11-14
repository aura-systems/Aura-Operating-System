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
        //public static List<string> DNSName = new List<string>();
        //public static List<string> DNSAddress = new List<string>();
        static string[] DNSName;
        static string[] DNSAddress;
        static string[] ArrayNull;

        static int position = 0;

        public static void Add_A_Entry(string name, Address address)
        {
            DNSName[position] = name;
            DNSAddress[position] = address.ToString();
        }

        public static string GetAddress(string name)
        {
            if (ArrayContains(DNSName, name))
            {
                //return DNSAddress[Array.IndexOf(DNSName, name)];
                return "8.8.8.8";
            }
            else
            {
                return null;
            }
        }

        //public static string GetName(Address address)
        //{
        //    if (Address.Contains(address.ToString()))
        //    {
        //        return DNSName[Array.IndexOf(DNSAddress, address.ToString())];
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public static void FlushDNS()
        {
            DNSName = ArrayNull;
            DNSAddress = ArrayNull;
        }

        private static bool ArrayContains(string[] array, string value)
        {
            int pos = Array.IndexOf(array, value);
            if(pos < 0)
            {
                return false;
            }
            return true;
        }
    }
}
