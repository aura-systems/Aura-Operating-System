/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.TCP;
using System.Text;
using Aura_OS;
using Aura_OS.Interpreter;
using Aura_OS.System.Network.HTTP.Client;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandWget : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandWget(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to download a http file.";
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            PrintHelp();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                TcpClientHttpRequest hr = new TcpClientHttpRequest();
                hr.Action = "testingmcafeesites.com";
                hr.Method = "get";
                hr.Send();
                //Kernel.console.WriteLine(hr.Response.Xml);
            }
            catch (Exception ex)
            {
                Kernel.console.WriteLine("Exception: " + ex);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Usage:");
            Kernel.console.WriteLine(" - dns {domain_name}");
            Kernel.console.WriteLine(" - dns {dns_server_ip} {domain_name}");
        }
    }
}