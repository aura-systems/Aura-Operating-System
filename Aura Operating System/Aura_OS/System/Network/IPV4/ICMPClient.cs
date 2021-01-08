/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ICMP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Aura_OS.System.Network.Config;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPv4
{
    public class ICMPClient
    {
        private static TempDictionary<ICMPClient> clients;

        protected Address destination;

        protected Queue<ICMPPacket> rxBuffer;

        static ICMPClient()
        {
            clients = new TempDictionary<ICMPClient>();
        }

        internal static ICMPClient Client(uint iphash)
        {
            if (clients.ContainsKey(iphash) == true)
            {
                return clients[iphash];
            }

            return null;
        }

        public ICMPClient()
        {
            rxBuffer = new Queue<ICMPPacket>(8);
        }

        public void Connect(Address dest)
        {
            destination = dest;
            clients.Add(dest.Hash, this);
        }

        public void Close()
        {
            if (clients.ContainsKey(destination.Hash) == true)
            {
                clients.Remove(destination.Hash);
            }
        }

        public void receiveData(ICMPPacket packet)
        {
            rxBuffer.Enqueue(packet);
        }

        public void SendEcho()
        {
            Address source = IPConfig.FindNetwork(destination);
            ICMPEchoRequest request = new ICMPEchoRequest(source, destination, 0x0001, 0x50); //this is working
            OutgoingBuffer.AddPacket(request); //Aura doesn't work when this is called.
            NetworkStack.Update();
        }

        public int Receive(ref EndPoint source, int timeout = 5000)
        {
            int second = 0;
            int _deltaT = 0;

            while (rxBuffer.Count < 1)
            {
                if (second > (timeout / 1000))
                {
                    return -1;
                }
                if (_deltaT != RTC.Second)
                {
                    second++;
                    _deltaT = RTC.Second;
                }
            }

            ICMPEchoReply packet = new ICMPEchoReply(rxBuffer.Dequeue().RawData);
            source.address = packet.SourceIP;

            return second;
        }
    }
}
