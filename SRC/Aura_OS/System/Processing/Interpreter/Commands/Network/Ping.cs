/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4.UDP.DNS;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
{
    class CommandPing : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandPing(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to send ping requests to an IP or domain name using ICMP";
        }

        /// <summary>
        /// CommandEcho
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            Address destination = Address.Parse(arguments[0]);

            if (destination == null) //Make a DNS request if it's not an IP
            {
                if (DNSConfig.DNSNameservers.Count == 0)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "No DNS server configured. Use ipconfig /nameserver -add or dhcp.");
                }

                try
                {
                    var xClient = new DnsClient();
                    xClient.Connect(DNSConfig.DNSNameservers[0]);
                    xClient.SendAsk(arguments[0]);
                    destination = xClient.Receive();
                    xClient.Close();
                }
                catch (Exception ex)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
                }

                if (destination == null)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Failed to get DNS response for " + arguments[0]);
                }

                Console.WriteLine(arguments[0] + " resolved to " + destination.ToString());
            }

            // GEN3-GAP(icmp): Cosmos gen3 has no ICMP support at all (the IPv4 handler only
            // dispatches TCP and UDP), so echo requests can neither be sent nor answered.
            // The gen2 ICMPClient loop is preserved in git history; restore it once ICMP
            // lands upstream.
            Console.WriteLine("ping is not supported on Cosmos gen3 yet (no ICMP).");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - ping {ip}");
            Console.WriteLine(" - ping {domain_name}");
        }
    }
}
