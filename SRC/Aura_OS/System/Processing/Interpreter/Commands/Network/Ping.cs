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
                    var dnsClient = new DnsClient();
                    dnsClient.Connect(DNSConfig.DNSNameservers[0]);
                    dnsClient.SendAsk(arguments[0]);
                    destination = dnsClient.Receive();
                    dnsClient.Close();
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

            if (IPConfig.FindNetwork(destination) == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "No network route to " + destination.ToString() + ". Use ipconfig or dhcp first.");
            }

            int packetSent = 0;
            int packetReceived = 0;
            int packetLost = 0;

            Console.WriteLine("Sending ping to " + destination.ToString());

            var xClient = new ICMPClient();
            xClient.Connect(destination);

            try
            {
                for (int i = 0; i < 4; i++)
                {
                    xClient.SendEcho(0x0001, (ushort)(i + 1));
                    packetSent++;

                    var endpoint = new EndPoint(Address.Zero, 0);

                    // gen3 Receive reports elapsed milliseconds (10ms granularity),
                    // where gen2 reported whole seconds.
                    int ms = xClient.Receive(ref endpoint, 4000);

                    if (ms == -1)
                    {
                        Console.WriteLine("Request timed out.");
                        packetLost++;
                    }
                    else if (ms < 10)
                    {
                        Console.WriteLine("Reply received from " + endpoint.Address.ToString() + " time < 10ms");
                        packetReceived++;
                    }
                    else
                    {
                        Console.WriteLine("Reply received from " + endpoint.Address.ToString() + " time = " + ms + "ms");
                        packetReceived++;
                    }
                }
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
            finally
            {
                xClient.Close();
            }

            int percentLoss = packetSent == 0 ? 0 : packetLost * 100 / packetSent;

            Console.WriteLine();
            Console.WriteLine("Ping statistics for " + destination.ToString() + ":");
            Console.WriteLine("    Packets: Sent = " + packetSent + ", Received = " + packetReceived + ", Lost = " + packetLost + " (" + percentLoss + "% loss)");

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
