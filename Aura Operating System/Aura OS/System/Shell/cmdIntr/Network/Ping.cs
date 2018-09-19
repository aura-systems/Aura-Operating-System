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

namespace Aura_OS.System.Shell.cmdIntr.Network
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
                string IPdest = "";

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
                    L.Text.Display("notcorrectaddress");
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
                System.Network.IPV4.UDP.DNS.DNSClient DNSRequest = new System.Network.IPV4.UDP.DNS.DNSClient(53);
                DNSRequest.Ask(str);
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
                c_Ping("     " + DNSRequest.address.ToString());
            }
        }

    }
}