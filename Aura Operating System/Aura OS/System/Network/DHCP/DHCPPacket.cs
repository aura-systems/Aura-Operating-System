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
        public DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        public static void DHCPHandler(byte[] packetData)
        {
            DHCPPacket dhcp_packet = new DHCPPacket(packetData);

            if ((packetData[278] == 0x63) && (packetData[279] == 0x82) && (packetData[280] == 0x53) && (packetData[281] == 0x63)) //Magic cookie: DHCP
            {
                switch (packetData[284])
                {
                    case 0x02: //DHCP : Offer

                        NetworkStack.RemoveAllConfigIP();

                        Utils.Settings.LoadValues();
                        Utils.Settings.EditValue("ipaddress", new Address(packetData, 58).ToString());
                        Utils.Settings.EditValue("subnet", new Address(packetData, 299).ToString());
                        Utils.Settings.EditValue("gateway", new Address(packetData, 287).ToString());
                        Utils.Settings.PushValues();

                        NetworkInit.Init(false);
                        NetworkInit.Enable();

                        Apps.System.Debugger.debugger.Send("New DHCP configuration applied!");

                        break;
                    default:
                        break;
                }
            }

        }
        protected override void initFields()
        {
            base.initFields();
        }

        public static int PacketSize { get; set; }

        public DHCPPacket(Address source)
            : base(300, 0x11, source, Address.Broadcast, 0x00, MACAddress.Broadcast)
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

        public override string ToString()
        {
            return "DHCP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", ";
        }
    }
}