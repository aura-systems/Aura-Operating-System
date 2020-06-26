/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Connection
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections;

namespace Aura_OS.System.Network.IPV4.TCP
{

    public static class TCPConnection
    {
        internal static TempDictionary<Connection> Connections = new TempDictionary<Connection>();

        internal class Connection
        {

            public Address source; public Address dest; public UInt16 destPort; public byte[] data = { 0x00, 0x00, 0x00, 0x00 }; public ulong sequencenumber; public ulong acknowledgmentnb; public UInt16 Headerlenght; public UInt16 Flags; public UInt16 WSValue; public UInt16 Checksum; public UInt16 UrgentPointer = 0x00;
            public UInt16 localPort;

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

            public bool IsLocal
            {
                get
                {
                    return false;
                }
            }

            public bool Start()
            {

                return true;
            }

            public bool Send(bool isdata)
            {
                Apps.System.Debugger.debugger.Send("Sending TCP packet...");
                if (isdata)
                {
                    TCPPacket packet = new TCPPacket(source, dest, localPort, destPort, data, sequencenumber, acknowledgmentnb, 0x50, Flags, WSValue, 0x0000, false, false);
                    OutgoingBuffer.AddPacket(packet);
                }
                else
                {
                    TCPPacket packet = new TCPPacket(source, dest, localPort, destPort, data, sequencenumber, acknowledgmentnb, 0x50, Flags, WSValue, 0x0000, true, true);
                    OutgoingBuffer.AddPacket(packet);
                }
                NetworkStack.Update();
                Apps.System.Debugger.debugger.Send("Sent!");
                return true;
            }

        }
    }
}
