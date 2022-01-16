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
using SimpleHttpServer.RouteHandlers;
using System.IO;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandHttpServer : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandHttpServer(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to start an HTTP Server";
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
                        Url = "/",
                        Callable = (HttpDiscussion result) => {
                            result.Response = new HttpResponse()
                            {
                                Content = Encoding.ASCII.GetBytes(@"<html>" +
                                            "\t<h1>Hello from <a href=\"https://github.com/aura-systems/Aura-Operating-System\">AuraOS</a>!</h1>" +
                                            "\t<p>Version: " + Global.version + "." + Global.revision + "</p>" +
                                            "\t<p>Server Hour: " + DateTime.Now.ToString() + "</p>" +
                                            "\t<p>Server Boot Time: " + Global.boottime + "</p>" +
                                            "\t<p>Powered by <a href=\"https://github.com/CosmosOS/Cosmos\">Cosmos</a>.</p>" +
                                            "</html>"),
                                ReasonPhrase = "OK",
                                StatusCode = "200"
                            };
                        }
                    }
                };

                var httpServer = new HttpServer(80, route_config);
                httpServer.Listen();
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandTree
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count != 1)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            try
            {
                string path = arguments[0];

                if (!path.EndsWith("/"))
                {
                    path += "/";
                }

                path = path.Replace("/", "\\");

                if (!Directory.Exists(path))
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Directory does not exist.");
                }

                var route_config = new List<Route>() {
                    new Route {
                        Name = "FileSystem Static Handler",
                        Method = "GET",
                        Url = "",
                        Callable = new FileSystemRouteHandler(path).Handle,
                    }
                };

                var httpServer = new HttpServer(80, route_config);
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
            Console.WriteLine(" - httpserver {path}");
        }
    }
}