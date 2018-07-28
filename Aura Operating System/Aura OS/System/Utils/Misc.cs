/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Different util methods
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System.Utils
{
    public class Misc
    {
        public static bool IsIpv4Address(string[] items)
        {
            if (items.Length == 4)
            {
                int n0;
                if (int.TryParse(items[0], out n0) && int.TryParse(items[1], out n0) && int.TryParse(items[2], out n0) && int.TryParse(items[3], out n0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsIpv4Address(string ip)
        {
            string[] items = ip.Split('.');
            if (IsIpv4Address(items))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
