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
    public class UdpClient : BaseClient
    {

        public UdpClient(int localPort) : base(localPort)
        { }

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

            UDPPacket packet = (UDPPacket)rxBuffer.Dequeue();
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            return packet.UDP_Data;
        }

        public byte[] Receive(ref EndPoint source)
        {
            while (rxBuffer.Count < 1) ;

            UDPPacket packet = (UDPPacket)rxBuffer.Dequeue();
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            return packet.UDP_Data;
        }

    }
}
