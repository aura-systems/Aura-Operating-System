/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ftp command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.TCP;
using System.Text;
using Aura_OS.System.Processing.Interpreter;
using CosmosFtpServer;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
{
    class CommandFtp : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandFtp(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to start a FTP server at port 21.";
        }

        /// <summary>
        /// CommandFtp
        /// </summary>
        public override ReturnInfo Execute()
        {
            try
            {
                using (var xServer = new FtpServer(Kernel.VirtualFileSystem, Kernel.CurrentDirectory, true))
                {
                    Console.WriteLine("FTP Server listening at " + NetworkConfiguration.CurrentAddress + ":21 ...");

                    xServer.Listen();
                }
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandFtp
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                string vol = arguments[0];

                using (var xServer = new FtpServer(Kernel.VirtualFileSystem, vol, true))
                {
                    Console.WriteLine("FTP Server listening at " + NetworkConfiguration.CurrentAddress + ":21 ...");

                    xServer.Listen();
                }
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - ftp            Open FTP server with path 0:\\");
            Console.WriteLine(" - ftp {path}     Open FTP server with custom path");
        }
    }
}