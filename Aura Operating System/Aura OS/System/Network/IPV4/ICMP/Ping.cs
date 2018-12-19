using Aura_OS.System.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.ICMP
{
    class Ping
    {

        public static void Send(Address destination, int NumberOfPing, string servername = null)
        {
            int PacketSent = 0;
            int PacketReceived = 0;
            int PacketLost = 0;
            int PercentLoss = 0;            

            if (servername != null)
            {
                Console.WriteLine("Sending ping to " + servername + " [" + destination.ToString() + "]");
            }
            else
            {
                Console.WriteLine("Sending ping to " + destination.ToString());
            }

            for (int i = 0; i < NumberOfPing; i++)
            {
                Address source = Config.FindNetwork(destination);

                ICMPEchoRequest request = new ICMPEchoRequest(source, destination, 0x0001, 0x50);
                OutgoingBuffer.AddPacket(request);
                NetworkStack.Update();

                PacketSent++;

                Timer pingTimer = new Timer(2);                

                while (true)
                {
                    pingTimer.IncrementSecond();

                    if (ICMPPacket.recvd_reply != null)
                    {
                        Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString());
                        PacketReceived++;
                        ICMPPacket.recvd_reply = null;

                        break;
                    }
                    else
                    {
                        if (destination.IsLoopbackAddress()) //Loopback address => PingOld ok
                        {
                            Console.WriteLine("Reply received from " + destination.ToString());
                            PacketReceived++;
                            ICMPPacket.recvd_reply = null;

                            break;
                        }
                    }
                }
            }

            PercentLoss = (100 / PacketSent) * PacketLost;

            Console.WriteLine();
            Console.WriteLine("Ping statistics for " + destination.ToString() + ":");
            Console.WriteLine("    Packets: Sent = " + PacketSent + ", Received = " + PacketReceived + ", Lost = " + PacketLost + " (" + PercentLoss + "% loss)");
        }

        public static void Send(string DNSname, int NumberOfPing)
        {
            UDP.DNS.DNSClient DNSRequest = new UDP.DNS.DNSClient(53);
            DNSRequest.Ask(DNSname);

            Timer.Wait(4, DNSRequest.ReceivedResponse);

            DNSRequest.Close();
            Send(DNSRequest.address, NumberOfPing, DNSname);
        }
    }
}
