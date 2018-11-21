/*
* PROJECT:          Aura Operating System Development
* CONTENT:          CRC-16
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

namespace Aura_OS.System.Security
{
    public class CRC
    {
        public static ushort Check(byte[] buffer, ushort offset, int length)
        {
            uint crc = 0;
            for (ushort w = offset; w < offset + length; w += 2)
            {
                crc += (ushort)((buffer[w] << 8) | buffer[w + 1]);
            }
            crc = (~((crc & 0xFFFF) + (crc >> 16)));
            return (ushort)crc;
        }
    }
}
