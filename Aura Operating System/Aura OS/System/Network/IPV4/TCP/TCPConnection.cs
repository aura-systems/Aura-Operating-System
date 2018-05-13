/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Connection
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System.Network.IPV4.TCP
{
    public class TCPConnection
    {

        public Address source; public Address dest; public UInt16 destPort; public byte[] data = { 0x00 }; public int sequencenumber; public int acknowledgmentnb; public UInt16 Headerlenght; public UInt16 Flags; public UInt16 WSValue; public UInt16 Checksum; public UInt16 UrgentPointer = 0x00;
        public UInt16 localPort;

        public bool isClosing = false;
        public bool isOpen = false;

        public void Send(bool finish)
        {
            if (finish)
            {
                TCPPacket packet = new TCPPacket(source, dest, localPort, destPort, data, sequencenumber, acknowledgmentnb, 0x50, 0x18, WSValue, 0x0000, true, false);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();
            }
            else
            {
                Kernel.debugger.Send("Starting response... 2");
                TCPPacket packet = new TCPPacket(source, dest, localPort, destPort, data, sequencenumber, acknowledgmentnb, 0x50, 0x12, WSValue, 0x0000, true, true);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();
                Kernel.debugger.Send("Response sent! 2");
            }
        }

    }
}
