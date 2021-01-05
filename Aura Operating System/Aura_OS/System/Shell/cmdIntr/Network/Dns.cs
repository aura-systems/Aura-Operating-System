/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using L = Aura_OS.System.Translation;
using Aura_OS.System.Network.IPV4;
using Aura_OS.System.Network;
using Aura_OS.System;
using System.Text;
using System.Collections.Generic;
using Aura_OS.System.Network.IPV4.UDP.DNS;
using Aura_OS.System.Network.IPV4.UDP;
using Aura_OS.System.Network.Config;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandDns : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandDns(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to send a DNS ask request";
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            PrintHelp();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            var xClient = new DnsClient();
            string domainname;

            if (arguments.Count < 1 || arguments.Count > 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
            else if (arguments.Count == 1)
            {
                xClient.Connect(DNSConfig.Server(0));
                Console.WriteLine("DNS used : " + DNSConfig.Server(0).ToString());
                xClient.SendAsk(arguments[0]);
                domainname = arguments[0];
            }
            else
            {
                xClient.Connect(Address.Parse(arguments[0]));
                xClient.SendAsk(arguments[1]);
                domainname = arguments[1];
            }

            Address address = xClient.Receive();

            xClient.Close();

            if (address == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Unable to find " + arguments[0]);
            }
            else
            {
                Console.WriteLine(domainname + " is " + address.ToString());
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - dns {domain_name}");
            Console.WriteLine(" - dns {dns_server_ip} {domain_name}");
        }
    }
}