/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - FIREWALL
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using System;
using System.Collections.Generic;
using L = Aura_OS.System.Translation;
namespace Aura_OS.Shell.cmdIntr.Network
{
    class Firewall
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
        public Firewall() { }

        /// <summary>
        /// c = command, c_Firewall
        /// </summary>
        /// <param name="arg">IP Address</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Firewall(string arg)
        {            
            string[] args = arg.Split(' ');
            //fw add tcp/udp 192.168.1.1 8080 true true

            if (args[1] == "add")
            {
                if (args[2] == "tcp")
                {
                    System.Network.Firewall.TCPRules.Create(args[3], args[4], args[5], args[6]);
                }
                if (args[2] == "udp")
                {
                    System.Network.Firewall.TCPRules.Create(args[3], args[4], args[5], args[6]);
                }
            }
            else if (args[1] == "rm")
            {
                if (args[2] == "tcp")
                {
                    System.Network.Firewall.TCPRules.Delete(args[2], args[3], args[4], args[5]);
                }
                if (args[2] == "udp")
                {
                    System.Network.Firewall.UDPRules.Create(args[3], args[4], args[5], args[6]);
                }
            }
            else if (args[1] == "help")
            {
                L.Text.Display("usagefw");
            }
            else if (args[1] == "enable")
            {
                System.Network.Firewall.Core.Enable();
            }
            else if (args[1] == "disable")
            {
                System.Network.Firewall.Core.Disable();
            }
            else if (args[1] == "status")
            {
                Console.WriteLine("Firewall status : " + System.Network.Firewall.Core.Status().ToString());
            }
            else
            {
                Console.WriteLine("Invalid firewall command");
            }
        }
    }
}
