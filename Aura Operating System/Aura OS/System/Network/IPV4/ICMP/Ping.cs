using Aura_OS.System.Translation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.ICMP
{
    class Ping
    {
        public static void Send(Address address)
        {
            string strAddress = address.ToString();
            string[] items = strAddress.Split('.');
            string env = strAddress.Remove(0, 1);

            //Support of the env variables.
            if (strAddress.StartsWith("$"))
            {
                if (Kernel.environmentvariables.ContainsKey(env))
                {
                    Send(Address.Parse(Kernel.environmentvariables[env]));
                    return;
                }
                else
                {
                    return;
                }
            }

            //Is it an IPv4 address ? or DNS ?
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

                        try
                        {
                            ICMPEchoRequest request = new ICMPEchoRequest(source, destination, 0x0001, 0x50);
                            OutgoingBuffer.AddPacket(request);
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
                                Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString());
                                PacketReceived++;
                                ICMPPacket.recvd_reply = null;

                                break;
                            }

                            if (source == destination) //If we're pinging our interface => Ping ok
                            {
                                Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString());
                                ICMPPacket.recvd_reply = null;

                                break;

                            }
                            else if (destination.IsLoopbackAddress()) //Loopback address => Ping ok
                            {
                                Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString());
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
                    Text.Display("notcorrectaddress");
                }
                finally
                {
                    PercentLoss = (100 / PacketSent) * PacketLost;

                    Console.WriteLine();
                    Console.WriteLine("Ping statistics for " + IPdest + ":");
                    Console.WriteLine("    Packets: Sent = " + PacketSent + ", Received = " + PacketReceived + ", Lost = " + PacketLost + " (" + PercentLoss + "% loss)");
                }
            }
            else
            {
                System.Network.IPV4.UDP.DNS.DNSClient DNSRequest = new System.Network.IPV4.UDP.DNS.DNSClient(53);
                DNSRequest.Ask(strAddress);
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
                Send(DNSRequest.address);
            }
        }
    }
}
