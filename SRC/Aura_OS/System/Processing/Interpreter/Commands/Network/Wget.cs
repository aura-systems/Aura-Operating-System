/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Network;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
{
    class CommandWget : ICommand
    {
        private const string DefaultFileName = "index.html";

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandWget(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to download a file through HTTP.";
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
            if (arguments.Count < 1 || arguments.Count > 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string url = arguments[0];

            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // GEN3-GAP(http-tls): no TLS in gen3.
                return new ReturnInfo(this, ReturnCode.ERROR_ARG, "HTTPS currently not supported, please use http://");
            }

            string fileName = GetFileName(url);
            string path;

            if (arguments.Count == 2)
            {
                path = AuraPath.Resolve(arguments[1]);

                if (Directory.Exists(path))
                {
                    path = AuraPath.AsDirectory(path) + fileName;
                }
                else if (!AuraPath.IsValidName(Path.GetFileName(path)))
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG, "Invalid file name: " + arguments[1]);
                }
            }
            else
            {
                path = AuraPath.AsDirectory(Kernel.CurrentDirectory) + fileName;
            }

            try
            {
                // Binary-safe: gen2 wrote the ASCII text to file.html whatever was downloaded.
                byte[] data = Http.DownloadRawFile(url);

                File.WriteAllBytes(path, data);

                Console.WriteLine(url + " saved to " + path + " (" + data.Length + " bytes)");
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Last URL path segment (query and fragment removed), or index.html.
        /// </summary>
        private static string GetFileName(string url)
        {
            string rest = url;

            int scheme = rest.IndexOf("://");
            if (scheme >= 0)
            {
                rest = rest.Substring(scheme + 3);
            }

            int cut = rest.IndexOfAny(new char[] { '?', '#' });
            if (cut >= 0)
            {
                rest = rest.Substring(0, cut);
            }

            int slash = rest.LastIndexOf('/');
            if (slash < 0)
            {
                return DefaultFileName; // host only
            }

            string name = rest.Substring(slash + 1);

            return AuraPath.IsValidName(name) ? name : DefaultFileName;
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - wget {url}            save to the current directory (last URL segment or index.html)");
            Console.WriteLine(" - wget {url} {path}     save to a custom file or directory (/N is the N-th volume)");
        }
    }
}