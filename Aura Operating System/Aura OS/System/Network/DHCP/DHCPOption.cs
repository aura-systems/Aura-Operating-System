using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.DHCP
{
    class DHCPOption
    {


        public DHCPOption(byte[] data)
        {
            if ((data[278] == 0x63) && (data[279] == 0x82) && (data[280] == 0x53) && (data[281] == 0x63))
            {
                Console.WriteLine("DHCP Packet detected!");
                for (int a = 282; a < data.Length; a++)
                {
                    byte[] Addr = { data[58], data[59], data[60], data[61] };
                    byte[] Gatw = { data[62], data[63], data[64], data[65] };

                    if (data[a] == 0x35)
                    {
                        if (data[a + 1] == 0x01)
                        {
                            Type = data[a + 2];
                        }
                    }

                    if ((data[a] == 0x01) && (data[a + 1] == 0x04) && (data[a + 2] == 0xff))
                    {
                        byte[] Subn = { data[a + 2], data[a + 3], data[a + 4], data[a + 5] };
                    }
                }
            }
        }

        public byte Type
        {
            get;
            set;
        }
    }
}
