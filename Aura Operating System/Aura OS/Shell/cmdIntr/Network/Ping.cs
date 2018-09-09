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

namespace Aura_OS.Shell.cmdIntr.Network
{
    class Ping
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
        public Ping() { }

        /// <summary>
        /// c = command, c_Ping
        /// </summary>
        /// <param name="arg">IP Address</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Ping(string arg, short startIndex = 0, short count = 5)
        {
            string str = arg.Remove(startIndex, count);
            string[] items = str.Split('.');

            if (System.Utils.Misc.IsIpv4Address(items))
            {
                String IPdest = "";

                int PacketSent = 0;
                int PacketReceived = 0;
                int PacketLost = 0;

                int PercentLoss = 0;

                try
                {
                    Address destination = new Address((byte)(Int32.Parse(items[0])), (byte)(Int32.Parse(items[1])), (byte)(Int32.Parse(items[2])), (byte)(Int32.Parse(items[3])));
                    Address source = Config.FindNetwork(destination);

                    IPdest = destination.ToString();

                    int _deltaT = 0;
                    int second;

                    Console.WriteLine("Sending ping to " + destination.ToString());

                    for (int i = 0; i < 4; i++)
                    {
                        second = 0;
                        CustomConsole.WriteLineInfo("Sending ping to " + destination.ToString() + "...");

                        try
                        {
                            ICMPEchoRequest request = new ICMPEchoRequest(source, destination, 0x0001, 0x50); //this is working
                            CustomConsole.WriteLineInfo("ICMP Request has been created.");
                            OutgoingBuffer.AddPacket(request); //Aura doesn't work when this is called.
                            CustomConsole.WriteLineInfo("Packet has been added to the OutgoingBuffer");
                            NetworkStack.Update();
                            CustomConsole.WriteLineInfo("NetworkStack updating...");
                        }
                        catch (Exception ex)
                        {
                            CustomConsole.WriteLineError(ex.ToString());
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
                    Console.WriteLine("It is not a correct IP address!");
                }
                finally
                {
                    PercentLoss = 25 * PacketLost;

                    Console.WriteLine();
                    Console.WriteLine("Ping statistics for " + IPdest + ":");
                    Console.WriteLine("    Packets: Sent = " + PacketSent + ", Received = " + PacketReceived + ", Lost = " + PacketLost + " (" + PercentLoss + "% loss)");
                }
            }
            else
            {
                Console.WriteLine("It is not an IP address!");
            }
        }

    }
}