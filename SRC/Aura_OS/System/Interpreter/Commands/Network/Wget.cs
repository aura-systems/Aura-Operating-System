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
using CosmosHttp.Client;
using Aura_OS.Interpreter;
using System.IO;

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
            string url = arguments[0];

            if (url.StartsWith("https://"))
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG, "HTTPS currently not supported, please use http://");
            }

            string path = ExtractPathFromUrl(url);
            string domainName = ExtractDomainNameFromUrl(url);

            try
            {
                var dnsClient = new DnsClient();

                dnsClient.Connect(DNSConfig.DNSNameservers[0]);
                dnsClient.SendAsk(domainName);
                Address address = dnsClient.Receive();
                dnsClient.Close();

                HttpRequest request = new();
                request.IP = address.ToString();
                request.Path = path;
                request.Method = "GET";
                request.Send();

                string httpresponse = request.Response.Content;

                File.WriteAllText(Kernel.CurrentDirectory + "file.html", httpresponse);

                Kernel.console.WriteLine(url + " saved to file.html");
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG, "Exception: " + ex);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        private string ExtractDomainNameFromUrl(string url)
        {
            int start;
            if (url.Contains("://"))
            {
                start = url.IndexOf("://") + 3;
            }
            else
            {
                start = 0;
            }

            int end = url.IndexOf("/", start);
            if (end == -1)
            {
                end = url.Length;
            }

            return url[start..end];
        }


        private string ExtractPathFromUrl(string url)
        {
            int start;
            if (url.Contains("://"))
            {
                start = url.IndexOf("://") + 3;
            }
            else
            {
                start = 0;
            }

            int indexOfSlash = url.IndexOf("/", start);
            if (indexOfSlash != -1)
            {
                return url.Substring(indexOfSlash);
            }
            else
            {
                return "/";
            }
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Usage:");
            Kernel.console.WriteLine(" - wget {url}");
        }
    }
}