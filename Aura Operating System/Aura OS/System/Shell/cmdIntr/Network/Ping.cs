/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using IPv4 = Aura_OS.System.Network.IPV4;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class Ping
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public Ping() { }

        /// <summary>
        /// c = command, c_Ping
        /// </summary>
        /// <param name="arg">IP Address</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Ping(string arg, short startIndex = 0, short count = 5)
        {
            string str = arg.Remove(startIndex, count);

            if (Utils.Misc.IsIpv4Address(str))
            {
                IPv4.Address destination = IPv4.Address.Parse(str);
                IPv4.ICMP.PingOld.Send(destination);
            }
            else
            {
                IPv4.ICMP.PingOld.Send(str); //Using DNS
            }
        }

    }
}