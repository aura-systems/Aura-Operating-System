using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.DHCP
{
    class DHCPOption
    {
        private int SubnetOffset;

        public DHCPOption(byte[] data)
        {
            if ((data[278] == 0x63) && (data[279] == 0x82) && (data[280] == 0x53) && (data[281] == 0x63))
            {
                for (int a = 282; a < data.Length; a++)
                {
                    if (data[a] == 0x35)
                    {
                        if (data[a + 1] == 0x01)
                        {
                            Type = data[a + 2];
                        }
                    }

                    if ((data[a] == 0x01) && (data[a + 1] == 0x04) && (data[a + 2] == 0xff))
                    {
                        SubnetOffset = a + 2;
                        return;
                    }
                }
            }
        }

        public byte Type
        {
            get;
            set;
        }

        public Address Address(byte[] data)
        {
            return new Address(data, 58);
        }

        public Address Gateway(byte[] data)
        {
            return new Address(data, 62);
        }

        public Address Subnet(byte[] data)
        {
            return new Address(data, SubnetOffset);
        }
    }
}
