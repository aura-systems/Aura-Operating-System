using Aura_OS.System.Translation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.ICMP
{
    class Ping
    {
        int PacketSent = 0;
        int PacketReceived = 0;
        int PacketLost = 0;

        float PercentLoss = 0;

        public bool NameResolved = false;

        /// <summary>
        /// Ping using an IPv4
        /// </summary>
        /// <param name="address">IPv4 that will be requested</param>
        public Ping(Address destination)
        {           
            try
            {  
                Address source = Config.FindNetwork(destination);

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
                        Apps.System.Debugger.debugger.Send("[ICMP REQUEST][EXCEPTION] " + ex.ToString());
                        CustomConsole.WriteLineError("Packet has not been sent correctly. (Please see debugging logs)");
                    }

                    PacketSent++;

                    while (second < 3)
                    {
                        if (ICMPPacket.recvd_reply != null)
                        {                            
                            if (ICMPPacket.recvd_reply.SourceIP == destination)
                            {
                                Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString() + " time < 1s"); //Pinging interface itself
                            }
                            else
                            {
                                Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString() + "");
                            }                           

                            PacketReceived++;

                            ICMPPacket.recvd_reply = null;
                            break;
                        }

                        if (second >= 2)
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
                PercentLoss = (PacketSent * 100) / 4;

                Console.WriteLine(Stats(destination));
            }
        }

        /// <summary>
        /// Ping using DNS name
        /// </summary>
        /// <param name="dns_name">Hostname that will be requested</param>
        public Ping(string dns_name)
        {
            if (UDP.DNS.DNSClient.Request(dns_name))
            {
                Address destination = UDP.DNS.DNSCache.GetCache(dns_name);
                NameResolved = true;
                new Ping(destination);
            }
            else
            {
                Console.WriteLine("Destination host unreachable. (Unknow host " + dns_name + ")");
                for (int i = 0; i < 3; i++) //will try 3 times while NameResolved is false
                {
                    Ping DNS_Retry = new Ping(dns_name);
                    if (DNS_Retry.NameResolved)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Destination host unreachable. (Unknow host " + dns_name + ")");
                    }
                }                
            }
        }

        /// <summary>
        /// Stats about ping
        /// </summary>
        /// <param name="destination">IPv4 that has been requested by ping</param>
        /// <returns>String that contains the statistics</returns>
        private string Stats(Address destination)
        {           
            return
                "Ping statistics for " + destination.ToString() + ":" +
                "    Packets: Sent = " + PacketSent + ", Received = " + PacketReceived + ", Lost = " + PacketLost + " (" + PercentLoss + "% loss)";
        }
    }
}
