using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4
{
    public class BaseClient
    {
        private static TempDictionary<BaseClient> clients;

        protected int localPort;
        protected int destinationPort;

        protected Address destination;

        protected Queue<IPPacket> rxBuffer;

        static BaseClient()
        {
            clients = new TempDictionary<BaseClient>();
        }

        internal static BaseClient Client(ushort destport)
        {
            if (clients.ContainsKey((uint)destport) == true)
            {
                return clients[(uint)destport];
            }

            return null;
        }

        public BaseClient() :this(0)
        { }

        public BaseClient(int localPort)
        {
            rxBuffer = new Queue<IPPacket>(8);

            this.localPort = localPort;
            if (localPort > 0)
            {
                clients.Add((uint)localPort, this);
            }
        }

        public BaseClient(Address dest, int destPort) : this(0)
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

        public void receiveData(IPPacket packet)
        {
            rxBuffer.Enqueue(packet);
        }

    }
}
