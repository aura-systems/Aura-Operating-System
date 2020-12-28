/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.TCP
{
    public class TcpClient
    {
        private static TempDictionary<TcpClient> clients;

        protected int localPort;
        protected int destinationPort;

        protected Address destination;

        protected Queue<TCPPacket> rxBuffer;

        static TcpClient()
        {
            clients = new TempDictionary<TcpClient>();
        }

        internal static TcpClient Client(ushort destport)
        {
            if (clients.ContainsKey((uint)destport) == true)
            {
                return clients[(uint)destport];
            }

            return null;
        }

        public TcpClient() : this(0)
        { }

        public TcpClient(int localPort)
        {
            rxBuffer = new Queue<TCPPacket>(8);

            this.localPort = localPort;
            if (localPort > 0)
            {
                clients.Add((uint)localPort, this);
            }
        }

        public TcpClient(Address dest, int destPort) : this(0)
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

        public void receiveData(TCPPacket packet)
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
        }

        public byte[] NonBlockingReceive(ref EndPoint source)
        {
            return null;
        }

        public byte[] Receive(ref EndPoint source)
        {
            while (rxBuffer.Count < 1);

            TCPPacket packet = new TCPPacket(rxBuffer.Dequeue().RawData);
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            return null;
        }

    }
}
