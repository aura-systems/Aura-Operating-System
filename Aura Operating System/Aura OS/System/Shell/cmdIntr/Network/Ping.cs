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
        public static void c_Ping(string cmd, short startIndex = 0, short count = 5)
        {
            string[] args = cmd.Split(' ');
            string IP = args[1];
            string env = IP.Remove(0, 1);

            //Support of the env variables.
            if (IP.StartsWith("$"))
            {
                if (Kernel.environmentvariables.ContainsKey(env))
                {
                    IPv4.ICMP.Ping.Send(IPv4.Address.Parse(Kernel.environmentvariables[env]), 4);
                    return;
                }
                else
                {
                    return;
                }
            }

            if (Utils.Misc.IsIpv4Address(IP))
            {
                IPv4.Address destination = IPv4.Address.Parse(IP);
                IPv4.ICMP.Ping.Send(destination,4);
            }
            else
            {
                IPv4.ICMP.Ping.Send(IP,4); //Using DNS
            }
        }

    }
}