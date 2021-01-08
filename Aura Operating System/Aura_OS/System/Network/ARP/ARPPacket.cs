/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ARP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using Aura_OS.HAL;
using Aura_OS.HAL.Drivers.Network;

namespace Aura_OS.System.Network.ARP
{
    internal class ARPPacket : EthernetPacket
    {
        protected UInt16 aHardwareType;
        protected UInt16 aProtocolType;
        protected byte aHardwareLen;
        protected byte aProtocolLen;
        protected UInt16 aOperation;

        internal static void ARPHandler(byte[] packetData)
        {
            ARPPacket arp_packet = new ARPPacket(packetData);
            Kernel.debugger.Send("[Received] " + arp_packet.ToString());
            if (arp_packet.Operation == 0x01)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    ARPRequest_Ethernet arp_request = new ARPRequest_Ethernet(packetData);
                    if (arp_request.SenderIP == null)
                    {
                        Kernel.debugger.Send("SenderIP null in ARPHandler!");
                    }
                    arp_request = new ARPRequest_Ethernet(packetData);
                    
                    ARPCache.Update(arp_request.SenderIP, arp_request.SenderMAC);

                    if (NetworkStack.AddressMap.ContainsKey(arp_request.TargetIP.Hash) == true)
                    {
                        Kernel.debugger.Send("ARP Request Recvd from " + arp_request.SenderIP.ToString());
                        NetworkDevice nic = NetworkStack.AddressMap[arp_request.TargetIP.Hash];

                        ARPReply_Ethernet reply =
                            new ARPReply_Ethernet(nic.MACAddress, arp_request.TargetIP, arp_request.SenderMAC, arp_request.SenderIP);

                        nic.QueueBytes(reply.RawData);
                    }
                }
            }
            else if (arp_packet.Operation == 0x02)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    ARPReply_Ethernet arp_reply = new ARPReply_Ethernet(packetData);
                    Kernel.debugger.Send("Received ARP Reply");
                    Kernel.debugger.Send(arp_reply.ToString());
                    Kernel.debugger.Send("ARP Reply Recvd from " + arp_reply.SenderIP.ToString());
                    ARPCache.Update(arp_reply.SenderIP, arp_reply.SenderMAC);

                    IPv4.OutgoingBuffer.ARPCache_Update(arp_reply);
                }
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new ARPPacket();
        }

        internal ARPPacket()
            : base()
        { }

        internal ARPPacket(byte[] rawData)
            : base(rawData)
        { }

        protected override void initFields()
        {
            base.initFields();
            aHardwareType = (UInt16)((RawData[14] << 8) | RawData[15]);
            aProtocolType = (UInt16)((RawData[16] << 8) | RawData[17]);
            aHardwareLen = RawData[18];
            aProtocolLen = RawData[19];
            aOperation = (UInt16)((RawData[20] << 8) | RawData[21]);
        }

        protected ARPPacket(MACAddress dest, MACAddress src, UInt16 hwType, UInt16 protoType,
            byte hwLen, byte protoLen, UInt16 operation, int packet_size)
            : base(dest, src, 0x0806, packet_size)
        {
            RawData[14] = (byte)(hwType >> 8);
            RawData[15] = (byte)(hwType >> 0);
            RawData[16] = (byte)(protoType >> 8);
            RawData[17] = (byte)(protoType >> 0);
            RawData[18] = hwLen;
            RawData[19] = protoLen;
            RawData[20] = (byte)(operation >> 8);
            RawData[21] = (byte)(operation >> 0);

            initFields();
        }

        internal UInt16 Operation
        {
            get { return this.aOperation; }
        }
        internal UInt16 HardwareType
        {
            get { return this.aHardwareType; }
        }
        internal UInt16 ProtocolType
        {
            get { return this.aProtocolType; }
        }

        public override string ToString()
        {
            return "ARP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", HWType=" + aHardwareType + ", Protocol=" + aProtocolType +
                ", Operation=" + Operation;
        }
    }
}
