using Aura_OS.HAL;
using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP Packet
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.DHCP
{
    public class DHCPPacket : IPPacket
    {

        public static void VMTInclude()
        {
            new DHCPPacket();
        }

        public DHCPPacket()
            : base()
        { }

        public DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        public static void DHCPHandler(byte[] packetData)
        {
            DHCPPacket dhcp_packet = new DHCPPacket(packetData);

            if (IsDHCPPacket(packetData))
            {
                Apps.System.Debugger.debugger.Send("Received DHCP packet from " + dhcp_packet.SourceIP.ToString());
                Apps.System.Debugger.debugger.Send("DHCP Offer received!");
            }
        }
        protected override void initFields()
        {
            base.initFields();


        }

        public static int PacketSize { get; set; }

        public DHCPPacket(MACAddress src, Address source, Address requested)
            : base(src, MACAddress.Broadcast, 300, 0x11, source, Address.Broadcast, 0x00)
        {
            //UDP

            //Source Port 68
            mRawData[dataOffset + 0] = 0x00;
            mRawData[dataOffset + 1] = 0x44;

            //Destination Port 67
            mRawData[dataOffset + 2] = 0x00;
            mRawData[dataOffset + 3] = 0x43;

            //Length
            PacketSize = 271;
            mRawData[dataOffset + 4] = (byte)((PacketSize >> 8) & 0xFF);
            mRawData[dataOffset + 5] = (byte)((PacketSize >> 0) & 0xFF);

            //Checksum
            mRawData[dataOffset + 6] = 0x00;
            mRawData[dataOffset + 7] = 0x00;

            initFields();
        }
        
        public static bool IsDHCPPacket(byte[] dhcpPacket)
        {
            if ((dhcpPacket[278] == 0x63) && (dhcpPacket[279] == 0x82) && (dhcpPacket[280] == 0x53) && (dhcpPacket[281] == 0x63)) //Magic cookie: DHCP
            {
                switch (dhcpPacket[284])
                {
                    case 0x02: //DHCP : Offer

                        byte DHCPMessageTypeLenght = dhcpPacket[283];
                        byte DHCPServerIDLenght = dhcpPacket[286];

                        NetworkStack.RemoveAllConfigIP();

                        Utils.Settings.LoadValues();
                        Utils.Settings.EditValue("ipaddress", new Address(dhcpPacket, 58).ToString());
                        Utils.Settings.EditValue("subnet", new Address(dhcpPacket, 299).ToString());
                        Utils.Settings.EditValue("gateway", new Address(dhcpPacket, 287).ToString());
                        Utils.Settings.PushValues();

                        NetworkInit.Init(false);
                        NetworkInit.Enable();

                        Console.WriteLine("DHCP : New IP config applied!");

                        break;
                    default:
                        break;
                }              
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "DHCP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", ";
        }
    }
}