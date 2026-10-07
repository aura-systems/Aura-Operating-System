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

        // FileStream's buffer: 1 turns it off, as in Entries.
        private const int UnbufferedSize = 1;

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

            HttpResponseMessage response;
            try
            {
                // The body read below, into the file as it comes: no download held whole in memory.
                response = Http.Get(url, HttpCompletionOption.ResponseHeadersRead);
            }
            catch (Exception ex)
            {
                // HttpRequestException (no address, no answer, timeout, TLS handshake, untrusted certificate...),
                // UriFormatException (not a URL).
                return new ReturnInfo(this, ReturnCode.ERROR, Http.Describe(ex));
            }

            // The response disposed on every path below, without a using: a Cosmos kernel skips its finally block
            // while an exception unwinds (GEN3-GAP(finally)).
            string location = response.RequestMessage.RequestUri.AbsoluteUri;
            string status = ((int)response.StatusCode + " " + response.ReasonPhrase).TrimEnd();
            if (!response.IsSuccessStatusCode)
            {
                response.Dispose();
                return new ReturnInfo(this, ReturnCode.ERROR, location + ": " + status);
            }

            // Null without a Content-Type.
            string type = response.Content.Headers.ContentType?.MediaType;

            FileStream file = null;
            long length = 0;
            string error = null;
            try
            {
                // Binary-safe: gen2 wrote the ASCII text to file.html whatever was downloaded.
                file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, UnbufferedSize);
            }
            catch (Exception ex)
            {
                error = "Can't save " + path + ": " + ex.Message;
            }

            if (file != null)
            {
                try
                {
                    response.Content.CopyTo(file);
                    length = file.Length;
                }
                catch (Exception ex)
                {
                    // Cut short, a timeout, a disk full.
                    error = location + ": " + Http.Describe(ex);
                }

                try
                {
                    file.Dispose();
                }
                catch (Exception ex)
                {
                    error ??= "Can't save " + path + ": " + ex.Message;
                }

                if (error != null)
                {
                    // Not a file that looks downloaded.
                    try
                    {
                        File.Delete(path);
                    }
                    catch
                    {
                    }
                }
            }

            response.Dispose();

            if (error != null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, error);
            }

            Console.WriteLine(location + ": " + status + ", " + length + " bytes" + (type is null ? "" : " [" + type + "]"));
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