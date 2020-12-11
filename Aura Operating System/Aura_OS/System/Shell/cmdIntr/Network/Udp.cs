/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using L = Aura_OS.System.Translation;
using Aura_OS.System.Network.IPV4;
using Aura_OS.System.Network;
using Aura_OS.System;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class Udp
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
        public Udp() { }

        /// <summary>
        /// c = command, c_Ping
        /// </summary>
        /// <param name="arg">IP Address</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Udp(string arg, short startIndex = 0, short count = 4)
        {
            string str = arg.Remove(startIndex, count);

            Char separator = ' ';
            string[] cmdargs = str.Split(separator);

            if (cmdargs[0] == "-l")
            {
                int port = Int32.Parse(cmdargs[1]);
                Console.WriteLine("Listening at " + port + "...");
                new System.Network.IPV4.UDP.UdpClient(port);
            }
            else if (cmdargs[0] == "-s")
            {
                string ip = cmdargs[1];

                Kernel.debugger.Send("ip=" + ip);

                int port = Int32.Parse(cmdargs[2]);

                Kernel.debugger.Send("port=" + port);

                string message = cmdargs[3];

                Kernel.debugger.Send("message=" + message);

                var xClient = new System.Network.IPV4.UDP.UdpClient(port);
                xClient.Connect(Address.Parse(ip), port);
                xClient.Send(Encoding.ASCII.GetBytes(message));
                xClient.Close();
            }
        }

    }
}