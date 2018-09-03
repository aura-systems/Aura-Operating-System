/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.TCP
{
    public class TCPClient
    {
        // TODO: Once we support more than just IPv4, we really need to base all the IPv4 classes on abstract classes
        // that represent the required functionality, then we can generalize the stack to be independent from IPv4 or IPv6
        internal class DataGram
        {
            internal byte[] data;
            internal IPV4.EndPoint source;

            internal DataGram(byte[] data, IPV4.EndPoint src)
            {
                this.data = data;
                this.source = src;
            }
        }

        internal static List<ulong> Connections = new List<ulong>();

        private static TempDictionary<TCPClient> clients;

        protected Int32 localPort;
        protected IPV4.Address destination;
        protected Int32 destinationPort;

        private Queue<DataGram> rxBuffer;

        static TCPClient()
        {
            clients = new TempDictionary<TCPClient>();
        }

        internal static TCPClient Client(ushort destPort)
        {
            if (clients.ContainsKey((UInt32)destPort) == true)
            {
                Apps.System.Debugger.debugger.Send("Client exists for port" + destPort);
                return clients[(UInt32)destPort];
            }

            return null;
        }

        public TCPClient()
            :this(0)
        { }

        public TCPClient(Int32 localPort)
        {
            this.rxBuffer = new Queue<DataGram>(8);

            this.localPort = localPort;
            if (localPort > 0)
            {
                TCPClient.clients.Add((UInt32)localPort, this);
            }
        }

        public TCPClient(IPV4.Address dest, Int32 destPort)
            : this(0)
        {
            this.destination = dest;
            this.destinationPort = destPort;
        }

        public void Connect(IPV4.Address dest, Int32 destPort)
        {
            this.destination = dest;
            this.destinationPort = destPort;

            TCPConnection.Connection connection = new TCPConnection.Connection();
            Address source = Config.FindNetwork(dest);

            ulong CID = (ulong)(dest.Hash + this.localPort + destPort);

            TCPConnection.Connections.Add((uint)CID, connection);

            connection.CID = CID;

            Apps.System.Debugger.debugger.Send(CID.ToString());

            connection.dest = dest;
            connection.source = source;

            connection.localPort = (ushort)localPort;
            connection.destPort = (ushort)destPort;

            connection.acknowledgmentnb = 0x0000;

            connection.WSValue = 1024;

            connection.sequencenumber = 3455719727;

            connection.Checksum = 0x0000;

            connection.Flags = 0x02;

            connection.Send(false);

            int _deltaT = 0;
            int second = 0;

            while (!connection.IsOpen)
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

            Apps.System.Debugger.debugger.Send("Connected!!");
        }

        public void Send(byte[] data)
        {
            if ((this.destination == null) ||
                (this.destinationPort == 0))
            {
                throw new Exception("Must establish a default remote host by calling Connect() before using this Send() overload");
            }

            Send(data, this.destination, this.destinationPort);
            NetworkStack.Update();

            int _deltaT = 0;
            int second = 0;

            while (!readytosend)
            {
                if (_deltaT != Cosmos.HAL.RTC.Second)
                {
                    second++;
                    _deltaT = Cosmos.HAL.RTC.Second;
                }

                if (second >= 4)
                {
                    Console.WriteLine("No response in 4 secondes...");
                    break;
                }
            }
            //Console.WriteLine("Done!");
        }

        public static ulong lastack = 0x00;
        public static ulong lastsn = 0x00;

        public bool IsConnected { get; internal set; }

        public static bool readytosend = true;

        public void Send(byte[] data, IPV4.Address dest, Int32 destPort)
        {

            IPV4.Address source = IPV4.Config.FindNetwork(dest);

            //Console.WriteLine("Sending " + "TCP Packet Src=" + source + ":" + localPort + ", Dest=" + dest + ":" + destPort + ", DataLen=" + data.Length);

            TCPConnection.Connection connection = new TCPConnection.Connection();

            connection.dest = dest;
            connection.source = source;

            connection.localPort = (ushort)localPort;
            connection.destPort = (ushort)destPort;

            connection.acknowledgmentnb = lastack;

            connection.WSValue = 1024;

            connection.sequencenumber = lastsn;

            connection.Checksum = 0x0000;

            connection.Flags = 0x18;

            connection.data = data;

            connection.Send(true);

            readytosend = false;

        }

        public void Close()
        {

            Apps.System.Debugger.debugger.Send("Closing TCP Connection...");

            IPV4.Address source = IPV4.Config.FindNetwork(this.destination);

            TCPConnection.Connection connection = new TCPConnection.Connection();

            Apps.System.Debugger.debugger.Send("Closing " + destination.ToString() + ":" + this.destinationPort + "now!");

            connection.dest = this.destination;
            connection.source = source;

            connection.localPort = (ushort)this.localPort;
            connection.destPort = (ushort)this.destinationPort;

            connection.acknowledgmentnb = lastack;

            connection.WSValue = 1024;

            connection.sequencenumber = lastsn;

            connection.Checksum = 0x0000;

            connection.Flags = 0x11;

            connection.Send(false);

            readytosend = false;

            Apps.System.Debugger.debugger.Send("Closed!");


        }

        public byte[] Receive(ref IPV4.EndPoint source)
        {
            if (this.rxBuffer.Count < 1)
            {
                return null;
            }

            DataGram packet = rxBuffer.Dequeue();
            source.address = packet.source.address;
            source.port = packet.source.port;

            return packet.data;
        }

        internal void receiveData(IPV4.TCP.TCPPacket packet)
        {
            byte[] data = packet.TCP_Data;
            IPV4.EndPoint source = new IPV4.EndPoint(packet.SourceIP, packet.SourcePort);

            Console.WriteLine("\nReceived TCP Packet (" + data.Length + "bytes) from " + source.ToString());
            Console.WriteLine("Content: " + Encoding.ASCII.GetString(data));

            this.rxBuffer.Enqueue(new DataGram(data, source));
        }
    }
}
