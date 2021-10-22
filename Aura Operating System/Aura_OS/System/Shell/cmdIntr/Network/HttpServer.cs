/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Http Server command
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
using SimpleHttpServer;
using SimpleHttpServer.Models;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandHttpServer : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandHttpServer(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to start an HTTP Server.";
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            try
            {
                var route_config = new List<Route>() {
                    new Route {
                        Name = "Hello Handler",
                        Method = "GET",
                        Callable = (HttpDiscussion result) => {
                            result.Response = new HttpResponse()
                            {
                                Content = Encoding.ASCII.GetBytes(@"<html>" +
                                            "\t<h1>Hello from AuraOS!</h1>" +
                                            "\t<p>Server hour: " + DateTime.Now.ToString() + "</p>" +
                                            "</html>"),
                                ReasonPhrase = "OK",
                                StatusCode = "200"
                            };
                        }
                    },
                };

                HttpServer httpServer = new HttpServer(80, route_config);

                httpServer.Listen();

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
            Console.WriteLine(" - httpserver");
        }
    }
}