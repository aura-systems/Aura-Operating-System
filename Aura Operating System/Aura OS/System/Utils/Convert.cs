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
                case '@':
                    return 0x40;
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
                case '[':
                    return 0x5B;
                case '\\':
                    return 0x5C;
                case ']':
                    return 0x5D;
                case '^':
                    return 0x5E;
                case '_':
                    return 0x5F;

                case '`':
                    return 0x60;
                case 'a':
                    return 0x61;
                case 'b':
                    return 0x62;
                case 'c':
                    return 0x63;
                case 'd':
                    return 0x64;
                case 'e':
                    return 0x65;
                case 'f':
                    return 0x66;
                case 'g':
                    return 0x67;
                case 'h':
                    return 0x68;
                case 'i':
                    return 0x69;
                case 'j':
                    return 0x6A;
                case 'k':
                    return 0x6B;
                case 'l':
                    return 0x6C;
                case 'm':
                    return 0x6D;
                case 'n':
                    return 0x6E;
                case 'o':
                    return 0x6F;
                case 'p':
                    return 0x70;
                case 'q':
                    return 0x71;
                case 'r':
                    return 0x72;
                case 's':
                    return 0x73;
                case 't':
                    return 0x74;
                case 'u':
                    return 0x75;
                case 'v':
                    return 0x76;
                case 'w':
                    return 0x77;
                case 'x':
                    return 0x78;
                case 'y':
                    return 0x79;
                case 'z':
                    return 0x7A;
                case '{':
                    return 0x7B;
                case '|':
                    return 0x7C;
                case '}':
                    return 0x7D;
                case '~':
                    return 0x7E;

                default:
                    return 0x00;
            }
        }
    }
}
