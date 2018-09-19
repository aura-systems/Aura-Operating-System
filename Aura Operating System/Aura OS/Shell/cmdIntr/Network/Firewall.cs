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
        public static void c_Firewall(string arg, short startIndex = 0, short count = 3)
        {
            string str = arg.Remove(startIndex, count);
            string[] args = str.Split(' ');
            //fw add 192.168.1.1 8080 true true

            if(args.Length == 5)
            {
                if(args[0] == "add")
                {                    
                    System.Network.Firewall.TCPRules.Create(args[1], args[2], args[3], args[4]);
                }
            }
            else
            {
                Console.WriteLine("Invalid firewall command");
            }
            
        }
    }
}
