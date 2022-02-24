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
using Cosmos.System.Network.IPv4.TCP.FTP;
using Aura_OS;
using Aura_OS.Interpreter;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandFtp : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandFtp(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to start a FTP server at port 21. (only ACTIVE mode supported yet).";
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            try
            {
                var xServer = new FtpServer(Kernel.VirtualFileSystem, Kernel.CurrentDirectory, true);

                Kernel.console.WriteLine("FTP Server listening at " + NetworkConfig.CurrentConfig.Value.IPAddress.ToString() + ":21 ...");

                xServer.Listen();
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
            Kernel.console.WriteLine("Usage:");
            Kernel.console.WriteLine(" - ftp");
        }
    }
}