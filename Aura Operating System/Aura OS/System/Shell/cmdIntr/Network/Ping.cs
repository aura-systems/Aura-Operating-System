/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using ICMP = Aura_OS.System.Network.IPV4.ICMP;
using Aura_OS.System.Network.IPV4;

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
            string[] items = str.Split('.');

            if (Utils.Misc.IsIpv4Address(items))
            {
                Address destination = new Address((byte)Int32.Parse(items[0]), (byte)Int32.Parse(items[1]), (byte)Int32.Parse(items[2]), (byte)Int32.Parse(items[3]));
                ICMP.Ping ping = new ICMP.Ping(destination);
            }
            else
            {
                if (!str.Contains(" ")) //no space in DNS name
                {
                    ICMP.Ping ping = new ICMP.Ping(str);
                }
            }
        }
    }
}