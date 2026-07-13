/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Network.IPv4.UDP.DNS;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.Config;
using Aura_OS;
using Aura_OS.System.Processing.Interpreter;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
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

            try
            {
                if (arguments.Count == 1)
                {
                    if (DNSConfig.DNSNameservers.Count == 0)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "No DNS server configured. Use ipconfig /nameserver -add or dhcp.");
                    }

                    xClient.Connect(DNSConfig.DNSNameservers[0]);
                    Console.WriteLine("DNS used : " + DNSConfig.DNSNameservers[0].ToString());
                    xClient.SendAsk(arguments[0]);
                    domainname = arguments[0];
                }
                else
                {
                    Address dnsServer = Address.Parse(arguments[0]);

                    if (dnsServer == null)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse DNS server address " + arguments[0]);
                    }

                    xClient.Connect(dnsServer);
                    xClient.SendAsk(arguments[1]);
                    domainname = arguments[1];
                }
            }
            catch (Exception ex)
            {
                // gen3 DnsClient.SendAsk throws when no route/config exists.
                xClient.Close();
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
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