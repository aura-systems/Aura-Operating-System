/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP (For local network discovering)
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.DHCP
{
    internal class DHCPDiscoverRequest
    {
        int xID;

        public byte[] DHCPData;
        public byte[] EthernetData;
        public byte[] IPData;
        public byte[] UDPData;
        protected UInt16 udpCRC;
        protected UInt16 fragmentID;
        protected UInt16 ipCRC;

        public List<byte> Trame = new List<byte>();

        public byte[] Packet()
        {
            foreach (byte octet in EthernetData)
            {
                Trame.Add(octet);
            }
            foreach (byte octet in IPData)
            {
                Trame.Add(octet);
            }
            foreach (byte octet in UDPData)
            {
                Trame.Add(octet);
            }
            foreach (byte octet in DHCPData)
            {
                Trame.Add(octet);
            }

            return Trame.ToArray();
        }

        public DHCPDiscoverRequest(HAL.MACAddress srcMAC, UInt16 datalength)
        {
            //ETHII
            byte[] EthernetData = new byte[14];

            //BROADCAST MAC
            EthernetData[0] = 0xff;
            EthernetData[1] = 0xff;
            EthernetData[2] = 0xff;
            EthernetData[3] = 0xff;
            EthernetData[4] = 0xff;
            EthernetData[5] = 0xff;

            //SRC MAC
            for(int i = 0; i < srcMAC.bytes.Length - 1; i++)
            {
                EthernetData[6 + i] = srcMAC.bytes[i];
            }

            EthernetData[13] = 0x08;
            EthernetData[14] = 0x00;

            byte[] IPData = new byte[20];
            byte lghIP = (byte)(20 + 14 + 296 + Computer.Info.HostnameLength());
            ipCRC = CalcIPCRC(20);

            IPData[0] = 0x45;
            IPData[1] = 0x00;
            IPData[2] = (byte)((lghIP >> 8) & 0xFF);
            IPData[3] = (byte)((lghIP >> 0) & 0xFF);
            IPData[4] = (byte)((fragmentID >> 8) & 0xFF); 
            IPData[5] = (byte)((fragmentID >> 0) & 0xFF);
            IPData[1] = 0x00;
            IPData[1] = 128;
            IPData[1] = 17;
            IPData[1] = (byte)((ipCRC >> 8) & 0xFF);
            IPData[1] = (byte)((ipCRC >> 0) & 0xFF);
            IPData[1] = 0x00;
            IPData[1] = 0x00;
            IPData[1] = 0x00;
            IPData[1] = 0x00;
            IPData[1] = 255;
            IPData[1] = 255;
            IPData[1] = 255;
            IPData[1] = 255;

            byte[] UDPData = new byte[8];
            byte lghUDP = (byte)(8 + 20 + 14 + 296 + Computer.Info.HostnameLength());
            udpCRC = CalcIPCRC(8);

            UDPData[0] = 68;
            UDPData[1] = 67;

            UDPData[2] = (byte)((lghUDP >> 8) & 0xFF);
            UDPData[3] = (byte)((lghUDP >> 0) & 0xFF);

            UDPData[4] = (byte)((udpCRC >> 8) & 0xFF);
            UDPData[5] = (byte)((udpCRC >> 0) & 0xFF);

            DHCPData = new byte[296 + Computer.Info.HostnameLength()];
            //Bootstrap Protocol
            //Message type
            DHCPData[0] = 0x01;

            DHCPData[1] = 0x01;
            DHCPData[2] = 6;
            DHCPData[3] = 0;

            //Transaction ID (xID)
            Random rnd = new Random();
            xID = rnd.Next(0, Int32.MaxValue);
            DHCPData[4] = (byte)((xID >> 24) & 0xFF);
            DHCPData[5] = (byte)((xID >> 16) & 0xFF);
            DHCPData[6] = (byte)((xID >> 8) & 0xFF);
            DHCPData[7] = (byte)((xID >> 0) & 0xFF);

            //Seconds elapsed
            DHCPData[8] = 0;
            DHCPData[9] = 0;

            //Bootp flags
            DHCPData[10] = 0;
            DHCPData[11] = 0;

            //Client IP address 0.0.0.0
            DHCPData[12] = 0;
            DHCPData[13] = 0;
            DHCPData[14] = 0;
            DHCPData[15] = 0;

            //Your (client) IP address 0.0.0.0
            DHCPData[16] = 0;
            DHCPData[17] = 0;
            DHCPData[18] = 0;
            DHCPData[19] = 0;

            //Next server IP address 0.0.0.0
            DHCPData[20] = 0;
            DHCPData[21] = 0;
            DHCPData[22] = 0;
            DHCPData[23] = 0;

            //Relay agent IP address 0.0.0.0
            DHCPData[24] = 0;
            DHCPData[25] = 0;
            DHCPData[26] = 0;
            DHCPData[27] = 0;

            //Client MAC address
            DHCPData[28] = srcMAC.bytes[0];
            DHCPData[29] = srcMAC.bytes[1];
            DHCPData[30] = srcMAC.bytes[2];
            DHCPData[31] = srcMAC.bytes[3];
            DHCPData[32] = srcMAC.bytes[4];
            DHCPData[33] = srcMAC.bytes[5];

            //Client hardware address padding & Server hostname & Boot file name
            for (int i = 34; i < 236; i++)
            {
                DHCPData[i] = 0; //only 0 because its not given
            }

            //Magic cookie: DHCP
            DHCPData[237] = 0x63;
            DHCPData[238] = 0x82;
            DHCPData[239] = 0x53;
            DHCPData[240] = 0x63;

            //Option (53) DHCP Message Type
            DHCPData[241] = 0x35; //35 = 53
            DHCPData[242] = 1; //Length
            DHCPData[243] = 1; //DHCP Discover

            //Option (61) Client identifier
            DHCPData[244] = 7;
            DHCPData[245] = 1;
            DHCPData[246] = srcMAC.bytes[0];
            DHCPData[247] = srcMAC.bytes[1];
            DHCPData[248] = srcMAC.bytes[2];
            DHCPData[249] = srcMAC.bytes[3];
            DHCPData[250] = srcMAC.bytes[4];
            DHCPData[251] = srcMAC.bytes[5];

            //Option (50) Requested IP Address             
            DHCPData[252] = 50;
            DHCPData[253] = 4;
            DHCPData[254] = 0;
            DHCPData[255] = 0;
            DHCPData[256] = 0;
            DHCPData[257] = 0;

            //Option (12) Host name            
            DHCPData[258] = 12;
            DHCPData[259] = (byte) Computer.Info.HostnameLength();
            int a = 0;
            for (int i = 260; i < (260 + Computer.Info.HostnameLength() - 1); i++)
            {
                DHCPData[i] = Computer.Info.getHostname()[a];
                a++;
            }

            int lgh = 259 + Computer.Info.HostnameLength();

            //Option (60) Vendor class identifier
            DHCPData[lgh + 0] = 60;
            DHCPData[lgh + 1] = 8;
            DHCPData[lgh + 2] = 0x4d;
            DHCPData[lgh + 3] = 0x53;
            DHCPData[lgh + 4] = 0x46;
            DHCPData[lgh + 5] = 0x54;
            DHCPData[lgh + 6] = 0x20;
            DHCPData[lgh + 7] = 0x35;
            DHCPData[lgh + 8] = 0x2e;
            DHCPData[lgh + 9] = 0x30;

            //Option (55) Parameter Request List
            DHCPData[lgh + 9] = 0x37; //55
            DHCPData[lgh + 10] = 0x0e;
            DHCPData[lgh + 11] = 0x01;
            DHCPData[lgh + 12] = 0x03;
            DHCPData[lgh + 13] = 0x06;
            DHCPData[lgh + 14] = 0x0f;
            DHCPData[lgh + 15] = 0x1f;
            DHCPData[lgh + 16] = 0x21;
            DHCPData[lgh + 17] = 0x2b;
            DHCPData[lgh + 18] = 0x2c;
            DHCPData[lgh + 19] = 0x2e;
            DHCPData[lgh + 20] = 0x2f;
            DHCPData[lgh + 21] = 0x77;
            DHCPData[lgh + 22] = 0x79;
            DHCPData[lgh + 23] = 0xf9;
            DHCPData[lgh + 24] = 0xfc;

            //End
            DHCPData[lgh + 25] = 0xff;
            for (int i = 26; i < (26 + 9); i++)
            {
                DHCPData[lgh + i] = 0;
            }
        }

        protected UInt16 CalcOcCRC(UInt16 offset, UInt16 length)
        {
            return CalcOcCRC(this.Trame.ToArray(), offset, length);
        }

        protected static UInt16 CalcOcCRC(byte[] buffer, UInt16 offset, int length)
        {
            UInt32 crc = 0;

            for (UInt16 w = offset; w < offset + length; w += 2)
            {
                crc += (UInt16)((buffer[w] << 8) | buffer[w + 1]);
            }

            crc = (~((crc & 0xFFFF) + (crc >> 16)));

            return (UInt16)crc;
        }

        protected UInt16 CalcIPCRC(UInt16 headerLength)
        {
            return CalcOcCRC(14, headerLength);
        }
    }
}
