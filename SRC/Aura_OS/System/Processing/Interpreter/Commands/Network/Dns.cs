/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Network;
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
            Address server;
            string domainname;

            if (arguments.Count < 1 || arguments.Count > 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
            else if (arguments.Count == 1)
            {
                if (DnsConfig.Nameservers.Count == 0)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "No DNS server, use ipconfig /ask or ipconfig /nameserver -add {IP}");
                }

                server = DnsConfig.Nameservers[0];
                domainname = arguments[0];
                Console.WriteLine("DNS used : " + server.ToString());
            }
            else
            {
                server = Address.Parse(arguments[0]);
                if (server is null)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse DNS server address: " + arguments[0]);
                }

                domainname = arguments[1];
            }

            // Cosmos DnsClient (not System.Net.Dns): it can query a chosen server.
            DnsClient xClient = null;
            Address address = null;
            string error = null;

            try
            {
                xClient = new DnsClient();
                xClient.Connect(server);
                xClient.SendQuery(domainname); // InvalidOperationException: no configured interface reaches the server
                address = xClient.Receive(5000);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            // gen3: finally/using do not run on the exception path, dispose explicitly.
            if (xClient != null)
            {
                xClient.Dispose();
            }

            if (error != null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Can't query DNS server " + server.ToString() + ": " + error);
            }

            if (address is null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Unable to find " + domainname);
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