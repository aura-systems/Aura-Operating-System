using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System
{
    public static class CharImpl
    {
        public static char ToLower(char c)
        {
            switch (c)
            {

                case 'A':
                    return 'a';
                case 'B':
                    return 'b';
                case 'C':
                    return 'c';
                case 'D':
                    return 'd';
                case 'E':
                    return 'e';
                case 'F':
                    return 'f';
                case 'G':
                    return 'g';
                case 'H':
                    return 'h';
                case 'I':
                    return 'i';
                case 'J':
                    return 'j';
                case 'K':
                    return 'k';
                case 'L':
                    return 'l';
                case 'M':
                    return 'm';
                case 'N':
                    return 'n';
                case 'O':
                    return 'o';
                case 'P':
                    return 'p';
                case 'Q':
                    return 'q';
                case 'R':
                    return 'r';
                case 'S':
                    return 's';
                case 'T':
                    return 't';
                case 'U':
                    return 'u';
                case 'V':
                    return 'v';
                case 'W':
                    return 'w';
                case 'X':
                    return 'x';
                case 'Y':
                    return 'y';
                case 'Z':
                    return 'z';
                default:
                    return ' ';
            }
        }


        public static string ToUpper(string c)
        {
            List<char> list = new List<char>();
            string returned = "";
            foreach (char cc in c) {
                switch (cc)
                {
                    case 'a':
                        returned = returned + "A";
                        break;
                    case 'b':
                        returned = returned + "B";
                        break;
                    case 'c':
                        returned = returned + "C";
                        break;
                    case 'd':
                        returned = returned + "D";
                        break;
                    case 'e':
                        returned = returned + "E";
                        break;
                    case 'f':
                        returned = returned + "F";
                        break;
                    case 'g':
                        returned = returned + "G";
                        break;
                    case 'h':
                        returned = returned + "H";
                        break;
                    case 'i':
                        returned = returned + "I";
                        break;
                    case 'j':
                        returned = returned + "J";
                        break;
                    case 'k':
                        returned = returned + "K";
                        break;
                    case 'l':
                        returned = returned + "L";
                        break;
                    case 'm':
                        returned = returned + "M";
                        break;
                    case 'n':
                        returned = returned + "N";
                        break;
                    case 'o':
                        returned = returned + "O";
                        break;
                    case 'p':
                        returned = returned + "P";
                        break;
                    case 'q':
                        returned = returned + "Q";
                        break;
                    case 'r':
                        returned = returned + "R";
                        break;
                    case 's':
                        returned = returned + "S";
                        break;
                    case 't':
                        returned = returned + "T";
                        break;
                    case 'u':
                        returned = returned + "U";
                        break;
                    case 'v':
                        returned = returned + "V";
                        break;
                    case 'w':
                        returned = returned + "W";
                        break;
                    case 'x':
                        returned = returned + "X";
                        break;
                    case 'y':
                        returned = returned + "Y";
                        break;
                    case 'z':
                        returned = returned + "Z";
                        break;
                    default:
                        break;
                }
            }
            return returned;
        }
    }
}
