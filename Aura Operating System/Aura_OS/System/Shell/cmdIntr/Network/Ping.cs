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
            if (System.Utils.Misc.IsIpv4Address(arguments[0]))
            {
                string IPdest = "";

                int PacketSent = 0;
                int PacketReceived = 0;
                int PacketLost = 0;

                int PercentLoss = 0;

                try
                {
                    Address destination = Address.Parse(arguments[0]);
                    Address source = Config.FindNetwork(destination);

                    IPdest = destination.ToString();

                    int _deltaT = 0;
                    int second;

                    Console.WriteLine("Sending ping to " + destination.ToString());

                    for (int i = 0; i < 4; i++)
                    {
                        second = 0;
                        //CustomConsole.WriteLineInfo("Sending ping to " + destination.ToString() + "...");

                        try
                        {
                            //replace address by source
                            //System.Network.IPV4.Address address = new System.Network.IPV4.Address(192, 168, 1, 70);
                            ICMPEchoRequest request = new ICMPEchoRequest(source , destination, 0x0001, 0x50); //this is working
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
                                //if (ICMPPacket.recvd_reply.SourceIP == destination)
                                //{

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
                                //}
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
                finally
                {
                    PercentLoss = 25 * PacketLost;

                    Console.WriteLine();
                    Console.WriteLine("Ping statistics for " + IPdest + ":");
                    Console.WriteLine("    Packets: Sent = " + PacketSent + ", Received = " + PacketReceived + ", Lost = " + PacketLost + " (" + PercentLoss + "% loss)");
                }
                return new ReturnInfo(this, ReturnCode.OK);
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Not a IPv4 address."); //L.Text notcorrectaddress
            }
        }
    }
}