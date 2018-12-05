using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Options
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.DHCP
{
    class DHCPOption
    {
        private int SubnetOffset;
        private int DHCPServerIDOffset;
        private int DNSOffset;
        private int DNSNameservers;

        private static byte[] PacketData;

        public DHCPOption(byte[] data)
        {
            PacketData = data;

            //DHCP magic packet
            if ((PacketData[278] == 0x63) && (PacketData[279] == 0x82) && (PacketData[280] == 0x53) && (PacketData[281] == 0x63))
            {
                for (int a = 282; a < PacketData.Length; a++)
                {
                    //get the type of the DHCP packet
                    if (PacketData[a] == 0x35)
                    {
                        if (PacketData[a + 1] == 0x01)
                        {
                            Type = PacketData[a + 2];
                        }
                    }

                    //get the Subnet optionss
                    if ((PacketData[a] == 0x01) && (PacketData[a + 1] == 0x04) && (PacketData[a + 2] == 0xff))
                    {
                        SubnetOffset = a + 2;
                    }

                    //get the DNS option
                    if ((PacketData[a] == 0x06) && (Utils.Math.IsDivisible(PacketData[a + 1], 4)))
                    {
                        DNSOffset = a + 2;
                        DNSNameservers = (PacketData[a + 1] / 4) - 1;
                        for (int i = 0; i < DNSNameservers; i++)
                        {
                            DNS[i] = new Address(PacketData, DNSOffset);
                            i++;
                        }
                    }

                    //get the DHCP Server IP option
                    if ((PacketData[a] == 0x36) && (PacketData[a + 1] == 0x04))
                    {
                        DHCPServerIDOffset = a + 2;
                    }
                }
            }
        }

        /// <summary>
        /// Return type of DHCP packet
        /// </summary>
        public byte Type
        {
            get;
            set;
        }

        /// <summary>
        /// Return the address that is sent by the DHCP server
        /// </summary>
        public Address Address()
        {
            return new Address(PacketData, 58);
        }

        /// <summary>
        /// Return the gateway address that is sent by the DHCP server
        /// </summary>
        public Address Gateway()
        {
            return new Address(PacketData, 62);
        }

        /// <summary>
        /// Return the subnet located in the bootstrap options
        /// </summary>
        public Address Subnet()
        {
            return new Address(PacketData, SubnetOffset);
        }

        public Address[] DNS;

        /// <summary>
        /// Return the DHCP server IP located in the bootstrap options
        /// </summary>
        public Address Server()
        {
            return new Address(PacketData, DHCPServerIDOffset);
        }
    }
}
