/*
* PROJECT:          Aura Operating System Development
* CONTENT:          UDP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.UDP
{
    public class UdpClient
    {
        private static TempDictionary<UdpClient> clients;

        protected int localPort;
        protected int destinationPort;

        protected Address destination;

        protected Queue<UDPPacket> rxBuffer;

        static UdpClient()
        {
            clients = new TempDictionary<UdpClient>();
        }

        internal static UdpClient Client(ushort destport)
        {
            if (clients.ContainsKey((uint)destport) == true)
            {
                return clients[(uint)destport];
            }

            return null;
        }

        public UdpClient() : this(0)
        { }

        public UdpClient(int localPort)
        {
            rxBuffer = new Queue<UDPPacket>(8);

            this.localPort = localPort;
            if (localPort > 0)
            {
                clients.Add((uint)localPort, this);
            }
        }

        public UdpClient(Address dest, int destPort) : this(0)
        {
            destination = dest;
            destinationPort = destPort;
        }

        public void Connect(Address dest, int destPort)
        {
            destination = dest;
            destinationPort = destPort;
        }

        public void Close()
        {
            if (clients.ContainsKey((uint)this.localPort) == true)
            {
                clients.Remove((uint)this.localPort);
            }
        }

        public void receiveData(UDPPacket packet)
        {
            rxBuffer.Enqueue(packet);
        }

        public void Send(byte[] data)
        {
            if ((destination == null) || (destinationPort == 0))
            {
                throw new Exception("Must establish a default remote host by calling Connect() before using this Send() overload");
            }

            Send(data, destination, destinationPort);
            NetworkStack.Update();
        }

        public void Send(byte[] data, Address dest, int destPort)
        {
            Address source = Config.FindNetwork(dest);
            UDPPacket packet = new UDPPacket(source, dest, (ushort)localPort, (ushort)destPort, data);
            OutgoingBuffer.AddPacket(packet);
        }

        public byte[] NonBlockingReceive(ref EndPoint source)
        {
            if (rxBuffer.Count < 1)
            {
                return null;
            }

            UDPPacket packet = new UDPPacket(rxBuffer.Dequeue().RawData);
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            return packet.UDP_Data;
        }

        public byte[] Receive(ref EndPoint source)
        {
            while (rxBuffer.Count < 1) ;

            UDPPacket packet = new UDPPacket(rxBuffer.Dequeue().RawData);
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            return packet.UDP_Data;
        }

    }
}
