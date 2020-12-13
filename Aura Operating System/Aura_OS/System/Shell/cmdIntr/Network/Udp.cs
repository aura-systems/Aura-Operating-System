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
using System.Collections.Generic;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandUdp : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandUdp(string[] commandvalues) : base(commandvalues)
        {
        }

        /// <summary>
        /// CommandEcho
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
            if (arguments[0] == "-l")
            {
                if (arguments.Count <= 1)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }
                int port = Int32.Parse(arguments[1]);
                Console.WriteLine("Listening at " + port + "...");
                new System.Network.IPV4.UDP.UdpClient(port);
            }
            else if (arguments[0] == "-s")
            {
                if (arguments.Count <= 3)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }
                Address ip = Address.Parse(arguments[1]);

                int port = Int32.Parse(arguments[2]);

                string message = arguments[3];

                var xClient = new System.Network.IPV4.UDP.UdpClient(port);

                xClient.Connect(ip, port);
                xClient.Send(Encoding.ASCII.GetBytes(message));
                xClient.Close();
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}