/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Aura_OS.System.Network;
using Cosmos.Kernel.System.Network;

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
            int PacketSent = 0;
            int PacketReceived = 0;
            int PacketLost = 0;
            int PercentLoss;

            if (arguments.Count != 1)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            // IP literal (v4 or v6), else a DNS request (A record) on the first nameserver.
            Address destination = NetworkHelper.Resolve(arguments[0]);

            if (destination is null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to get DNS response for " + arguments[0]);
            }

            Address6 destination6 = destination as Address6;
            IcmpClient xClient = null;
            Icmpv6Client xClient6 = null;
            string error = null;

            try
            {
                Console.WriteLine("Sending ping to " + destination.ToString());

                // IcmpClient builds IPv4 packets only.
                if (destination6 is not null)
                {
                    xClient6 = new Icmpv6Client();
                    xClient6.Connect(destination6);
                }
                else
                {
                    xClient = new IcmpClient();
                    xClient.Connect(destination);
                }

                for (ushort sequence = 1; sequence <= 4; sequence++)
                {
                    // InvalidOperationException when no configured interface reaches the destination.
                    if (xClient6 != null)
                    {
                        xClient6.SendEcho(1, sequence);
                    }
                    else
                    {
                        xClient.SendEcho(1, sequence);
                    }

                    PacketSent++;

                    var endpoint = new EndPoint(Address4.Zero, 0);

                    // gen3 returns the elapsed milliseconds (10 ms steps), gen2 returned seconds.
                    // GEN3-GAP(net-misc): IcmpClient doesn't match the echo id/sequence.
                    int milliseconds = xClient6 != null ? xClient6.Receive(ref endpoint, 4000) : xClient.Receive(ref endpoint, 4000);

                    if (milliseconds == -1)
                    {
                        Console.WriteLine("Destination host unreachable.");
                        PacketLost++;
                    }
                    else
                    {
                        if (milliseconds == 0)
                        {
                            Console.WriteLine("Reply received from " + endpoint.Address.ToString() + " time<10ms");
                        }
                        else
                        {
                            Console.WriteLine("Reply received from " + endpoint.Address.ToString() + " time=" + milliseconds + "ms");
                        }

                        PacketReceived++;
                    }
                }
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

            if (xClient6 != null)
            {
                xClient6.Dispose();
            }

            if (error != null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Ping process error: " + error);
            }

            PercentLoss = 25 * PacketLost;

            Console.WriteLine();
            Console.WriteLine("Ping statistics for " + destination.ToString() + ":");
            Console.WriteLine("    Packets: Sent = " + PacketSent + ", Received = " + PacketReceived + ", Lost = " + PacketLost + " (" + PercentLoss + "% loss)");

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