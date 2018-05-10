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

namespace Aura_OS.Shell.cmdIntr.Network
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
            Console.WriteLine("'" + str + "'");
            Console.ReadKey();
            //Address destination = Address.Parse(str);
            Address destination = new Address(192, 168, 1, 12);
            Console.WriteLine(destination.ToString());
            Address source = Config.FindNetwork(destination);
            Console.WriteLine(source.ToString());
            ICMPEchoRequest request = new ICMPEchoRequest(source, destination, 0x0001, 0x50);
            OutgoingBuffer.AddPacket(request);
            NetworkStack.Update();
            Kernel.debugger.Send("Ping done");
        }

    }
}