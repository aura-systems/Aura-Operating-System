using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network
{
    /*class ARP
    {
        public void Request(Net.IPv4.Address IPSrc, Net.IPv4.Address IPDest, HALNet.MACAddress MACSrc)
        {
            byte[] IPSrcByte = IPSrc.ToByteArray();
            byte[] IPDestByte = IPDest.ToByteArray();
            byte[] MACaddress = MACSrc.bytes;

            byte[] TrameArray = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,  MACaddress[0], MACaddress[1], MACaddress[2], MACaddress[3], MACaddress[4], MACaddress[5],
                            0x08, 0x06, 0x00, 0x01, 0x08, 0x00, 0x06, 0x04, 0x00, 0x01, MACaddress[0], MACaddress[1], MACaddress[2], MACaddress[3], MACaddress[4],
                            MACaddress[5], IPSrcByte[0], IPSrcByte[1], IPSrcByte[2], IPSrcByte[3], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, IPDestByte[0],
                            IPDestByte[1], IPDestByte[2], IPDestByte[3]};
        }
    }
    */
}
