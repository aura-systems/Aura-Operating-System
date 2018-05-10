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
            string[] items = str.Split('.');
            Address destination = new Address((byte)(Int32.Parse(items[0])), (byte)(Int32.Parse(items[1])), (byte)(Int32.Parse(items[2])), (byte)(Int32.Parse(items[3])));
            Address source = Config.FindNetwork(destination);

            int _deltaT = 0;
            int second;

            for (int i = 0; i<4; i++)
            {
                second = 0;
                Console.WriteLine("Sending ping to " + destination.ToString() + "...");

                ICMPEchoRequest request = new ICMPEchoRequest(source, destination, 0x0001, 0x50);
                OutgoingBuffer.AddPacket(request);
                NetworkStack.Update();

                while (true)
                {

                    if (ICMPPacket.recvd_reply != null)
                    {
                        //if (ICMPPacket.recvd_reply.SourceIP == destination)
                        //{
                            Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString() + " in " + second + " secondes!");
                            ICMPPacket.recvd_reply = null;
                            break;
                        //}
                    }

                    if (second >= 5)
                    {
                        Console.WriteLine("Unable to reach the destination host.");
                        break;
                    }

                    if (_deltaT != Cosmos.HAL.RTC.Second)
                    {
                        second++;
                        _deltaT = Cosmos.HAL.RTC.Second;
                    }
                }
            }

        }

    }
}