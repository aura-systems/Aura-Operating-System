/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ftp command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Aura_OS.System.Processing.Interpreter;

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
            // GEN3-GAP(ftp-server): CosmosFtpServer targets gen2's kernel TcpListener and
            // CosmosVFS, neither of which exists in gen3. A rewrite on the plugged
            // System.Net.Sockets.TcpListener + System.IO is planned as a follow-up, but
            // gen3's SocketPlug.Accept currently supports only one connection per listener
            // with no cancellation, which makes a usable server impractical for now.
            Console.WriteLine("The FTP server is not supported on Cosmos gen3 yet (no CosmosFtpServer; TcpListener accept semantics too limited).");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandFtp
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            return Execute();
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - ftp            Open FTP server with path " + Kernel.RootVolume);
            Console.WriteLine(" - ftp {path}     Open FTP server with custom path");
        }
    }
}
