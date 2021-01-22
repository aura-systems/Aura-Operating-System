/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using L = Aura_OS.System.Translation;
using Cosmos.System.Network;
using Aura_OS.System;
using System.Text;
using System.Collections.Generic;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandUdp : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandUdp(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to send or received UDP packets";
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
            if (arguments[0] == "/l")
            {
                if (arguments.Count <= 1)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }
                int port = Int32.Parse(arguments[1]);

                Console.WriteLine("Listening at " + port + "...");

                var client = new UdpClient(port);

                EndPoint RemoteIpEndPoint = new EndPoint(Address.Zero, 0);

                byte[] received = client.Receive(ref RemoteIpEndPoint);

                Console.WriteLine("Received UDP packet from " + RemoteIpEndPoint.address.ToString() + ": \"" + Encoding.ASCII.GetString(received) + "\"");

                client.Close();

                return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (arguments[0] == "/s")
            {
                if (arguments.Count <= 3)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }
                Address ip = Address.Parse(arguments[1]);

                int port = int.Parse(arguments[2]);

                string message = arguments[3];

                var xClient = new UdpClient(port);

                xClient.Connect(ip, port);

                xClient.Send(Encoding.ASCII.GetBytes(message));
                Console.WriteLine("Sent UDP packet to " + ip.ToString() + ":" + port);

                xClient.Close();
                return new ReturnInfo(this, ReturnCode.OK);
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - udp /l {port}                       listen for an UDP packet at a specific port");
            Console.WriteLine(" - udp /s {ip} {port} {text_message}   send an UDP packet to and IP/port");
        }
    }
}