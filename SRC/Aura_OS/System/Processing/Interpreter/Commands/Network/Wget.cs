/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Wget command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Network;
using Cosmos.Network.Http;

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
            Description = "to download a file through HTTP or HTTPS.";
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

            HttpResponse response;
            try
            {
                response = Http.CreateRequest(url).Send();
            }
            catch (Exception ex)
            {
                // HttpException (no address, no answer, timeout, TLS handshake, untrusted certificate...),
                // NotSupportedException (neither http:// nor https://), FormatException (bad host or port).
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            string status = (response.StatusCode + " " + response.ReasonPhrase).TrimEnd();
            if (!response.IsSuccessStatusCode)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, response.Url + ": " + status);
            }

            try
            {
                // Binary-safe: gen2 wrote the ASCII text to file.html whatever was downloaded.
                File.WriteAllBytes(path, response.Content);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Can't save " + path + ": " + ex.Message);
            }

            string type = response.ContentType is null ? "" : " [" + response.ContentType + "]";
            Console.WriteLine(response.Url + ": " + status + ", " + response.Content.Length + " bytes" + type);
            Console.WriteLine("Saved to " + path);

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