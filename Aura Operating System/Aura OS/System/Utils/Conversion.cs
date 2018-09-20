/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Conversion
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Text;

namespace Aura_OS.System.Utils
{
    public static class Conversion
    {
        
        public static string D2(uint number)
        {
            if (number < 10)
            {
                return "0" + number;
            }
            else
            {
                return number.ToString();
            }
        }

        public static string D4(string text)
        {
            if (text.Length < 4)
            {
                switch (text.Length)
                {
                    case 3:
                        return "0" + text;
                    case 2:
                        return "00" + text;
                    case 1:
                        return "000" + text;
                    default:
                        return text;
                }
            }
            else
            {
                return text;
            }
        }

        public static string Hex(this byte[] value)
        {
            int offset = 0;
            int length = -1;

            if (length < 0)
                length = value.Length - offset;
            var builder = new StringBuilder(length * 2);
            int b;
            for (int i = offset; i < length + offset; i++)
            {
                b = value[i] >> 4;
                builder.Append((char)(55 + b + (((b - 10) >> 31) & -7)));
                b = value[i] & 0xF;
                builder.Append((char)(55 + b + (((b - 10) >> 31) & -7)));
            }
            return builder.ToString();
        }

        public static string DecToHex(int x)
        {
            string result = "";

            while (x != 0)
            {
                if ((x % 16) < 10)
                    result = x % 16 + result;
                else
                {
                    string temp = "";

                    switch (x % 16)
                    {
                        case 10: temp = "A"; break;
                        case 11: temp = "B"; break;
                        case 12: temp = "C"; break;
                        case 13: temp = "D"; break;
                        case 14: temp = "E"; break;
                        case 15: temp = "F"; break;
                    }

                    result = temp + result;
                }

                x /= 16;
            }

            return result;
        }

        //public static int HexToDec(string x)
        //{
        //    int result = 0;
        //    int count = x.Length - 1;
        //    for (int i = 0; i < x.Length; i++)
        //    {
        //        int temp = 0;
        //        switch (x[i])
        //        {
        //            case 'A': temp = 10; break;
        //            case 'B': temp = 11; break;
        //            case 'C': temp = 12; break;
       //             case 'D': temp = 13; break;
       //             case 'E': temp = 14; break;
       //             case 'F': temp = 15; break;
       //             default: temp = -48 + (int)x[i]; break; // -48 because of ASCII
       //         }

                //result += temp * (int)(Math.Pow(16, count));
        //        count--;
        //    }

        //    return result;
        //}

    }
}

