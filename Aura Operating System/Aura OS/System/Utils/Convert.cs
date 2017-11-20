using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Utils
{
    public static class Convert
    {
        public static byte StringToByte(char text)
        {
            switch (text)
            {
                case 'A':
                    return 0x41;
                case 'B':
                    return 0x42;
                case 'C':
                    return 0x43;
                case 'D':
                    return 0x44;
                case 'E':
                    return 0x45;
                case 'F':
                    return 0x46;
                case 'G':
                    return 0x47;
                case 'H':
                    return 0x48;
                case 'I':
                    return 0x49;
                case 'J':
                    return 0x4A;
                case 'K':
                    return 0x4B;
                case 'L':
                    return 0x4C;
                case 'M':
                    return 0x4D;
                case 'N':
                    return 0x4E;
                case 'O':
                    return 0x4F;
                case 'P':
                    return 0x50;
                case 'Q':
                    return 0x51;
                case 'R':
                    return 0x52;
                case 'S':
                    return 0x53;
                case 'T':
                    return 0x54;
                case 'U':
                    return 0x55;
                case 'V':
                    return 0x56;
                case 'W':
                    return 0x57;
                case 'X':
                    return 0x58;
                case 'Y':
                    return 0x59;
                case 'Z':
                    return 0x5A;

                default:
                    return 0x00;
            }
        }
    }
}
