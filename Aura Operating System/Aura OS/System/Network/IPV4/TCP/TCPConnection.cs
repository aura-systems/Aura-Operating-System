/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Connection
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections;

namespace Aura_OS.System.Network.IPV4.TCP
{

    public static class TCPFlux
    {
        internal static TempDictionary<Status> Connections = new TempDictionary<Status>();

        internal class Status
        {

            public Address Source;
            public Address Destination;

            public ushort DestinationPort;
            public ushort SourcePort;

            public byte[] Data = { 0x00, 0x00, 0x00, 0x00 };

            public ulong SequenceNumber;
            public ulong ACKNumber;

            public uint Headerlenght;
            public ushort Flags;
            public ushort WSValue;
            public uint Checksum;
            public uint UrgentPointer = 0x00;

            internal bool isClosing = false;
            private bool isOpen = false;
            private uint cid = 0;

            public void Close()
            {
                if (Connections.ContainsKey(cid) == true)
                {
                    Connections.Remove((UInt32)cid);
                }
            }

            internal bool IsOpen
            {
                get { return isOpen; }
                set
                {
                    isOpen = true;
                }
            }

            public ulong CID
            {
                get
                {
                    return cid;
                }
                set
                {
                    cid = (uint)value;
                }
            }

            public bool Send(bool isdata)
            {
                if (isdata)
                {
                    TCPPacket packet = new TCPPacket(Source, Destination, SourcePort, DestinationPort, Data, SequenceNumber, ACKNumber, 0x50, Flags, WSValue, 0x0000, false, false);
                    OutgoingBuffer.AddPacket(packet);
                }
                else
                {
                    TCPPacket packet = new TCPPacket(Source, Destination, SourcePort, DestinationPort, Data, SequenceNumber, ACKNumber, 0x50, Flags, WSValue, 0x0000, true, true);
                    OutgoingBuffer.AddPacket(packet);
                }
                NetworkStack.Update();
                return true;
            }

        }
    }
}
