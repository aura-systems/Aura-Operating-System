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
using System.Collections.Generic;
using Aura_OS.System.Network.IPV4.UDP.DNS;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandPing : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandPing(string[] commandvalues) : base(commandvalues)
        {
        }

        /// <summary>
        /// CommandEcho
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (NetworkStack.ConfigEmpty())
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "No network configuration detected! Use ipconfig /set.");
            }

            int PacketSent = 0;
            int PacketReceived = 0;
            int PacketLost = 0;
            int PercentLoss;

            Address source;
            Address destination = Address.Parse(arguments[0]);

            if (destination != null)
            {
                source = Config.FindNetwork(destination);
            }
            else //Make a DNS request if it's not an IP
            {
                var xClient = new DnsClient();
                xClient.Connect(Address.Parse("192.168.1.1")); //TODO: https://github.com/aura-systems/Aura-Operating-System/issues/173
                xClient.SendAsk(arguments[0]);
                destination = xClient.Receive();
                source = Config.FindNetwork(destination);
            }

            try
            {
                int _deltaT = 0;
                int second;

                Console.WriteLine("Sending ping to " + destination.ToString());

                for (int i = 0; i < 4; i++)
                {
                    second = 0;

                    try
                    {
                        ICMPEchoRequest request = new ICMPEchoRequest(source, destination, 0x0001, 0x50); //this is working
                        OutgoingBuffer.AddPacket(request); //Aura doesn't work when this is called.
                        NetworkStack.Update();
                    }
                    catch (Exception ex)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, ex.ToString());
                    }

                    PacketSent++;

                    while (true)
                    {

                        if (ICMPPacket.recvd_reply != null)
                        {

                            if (second < 1)
                            {
                                Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString() + " time < 1s");
                            }
                            else if (second >= 1)
                            {
                                Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString() + " time " + second + "s");
                            }

                            PacketReceived++;

                            ICMPPacket.recvd_reply = null;
                            break;
                        }

                        if (second >= 5)
                        {
                            Console.WriteLine("Destination host unreachable.");
                            PacketLost++;
                            break;
                        }

                        if (_deltaT != Cosmos.HAL.RTC.Second)
                        {
                            second++;
                            _deltaT = Cosmos.HAL.RTC.Second;
                        }
                    }
                }
            }
            catch
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Ping process error.");
            }

            PercentLoss = 25 * PacketLost;

            Console.WriteLine();
            Console.WriteLine("Ping statistics for " + destination.ToString() + ":");
            Console.WriteLine("    Packets: Sent = " + PacketSent + ", Received = " + PacketReceived + ", Lost = " + PacketLost + " (" + PercentLoss + "% loss)");

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}