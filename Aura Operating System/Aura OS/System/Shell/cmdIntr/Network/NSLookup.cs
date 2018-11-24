using Aura_OS.System.Network.IPV4;
using Aura_OS.System.Translation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class NSLookup
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public NSLookup() { }

        static string dns_name;
        static Address dns_server;

        public static void c_NSLookup(string cmd)
        {
            string[] args = cmd.Split(' ');            

            System.Network.IPV4.UDP.DNS.DNSClient DNSRequest;

            if(args.Length == 1)
            {
                Console.WriteLine("Usage : nslookup geomtech.fr {dns_server_ip}");
            }
            else if (args.Length == 2)
            {
                DNSRequest = new System.Network.IPV4.UDP.DNS.DNSClient(53);
                DNSRequest.Ask(dns_name, dns_server);
                int _deltaT = 0;
                int second = 0;
                while (!DNSRequest.ReceivedResponse)
                {
                    if (_deltaT != Cosmos.HAL.RTC.Second)
                    {
                        second++;
                        _deltaT = Cosmos.HAL.RTC.Second;
                    }

                    if (second >= 4)
                    {
                        Apps.System.Debugger.debugger.Send("No response in 4 secondes...");
                        break;
                    }
                }
                DNSRequest.Close();
            }
            else if(args.Length == 3)
            {
                DNSRequest = new System.Network.IPV4.UDP.DNS.DNSClient(53);
                DNSRequest.Ask(dns_name, dns_server);
                int _deltaT = 0;
                int second = 0;
                while (!DNSRequest.ReceivedResponse)
                {
                    if (_deltaT != Cosmos.HAL.RTC.Second)
                    {
                        second++;
                        _deltaT = Cosmos.HAL.RTC.Second;
                    }

                    if (second >= 4)
                    {
                        Apps.System.Debugger.debugger.Send("No response in 4 secondes...");
                        break;
                    }
                }
                DNSRequest.Close();
                Console.WriteLine("Name :      " + DNSRequest.URL);
                Console.WriteLine("Address :   " + DNSRequest.address.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Text.Display("UnknownCommand");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }                        
        }

    }
}

