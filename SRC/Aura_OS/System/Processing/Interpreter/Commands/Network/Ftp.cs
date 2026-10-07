/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ftp command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Network;
using Cosmos.Network.Ftp;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
{
    class CommandFtp : ICommand
    {
        /// <summary>
        /// The background FTP server, if one was started.
        /// </summary>
        private static FtpServer s_server;

        /// <summary>
        /// True from the start of a server until its thread has returned from Listen().
        /// </summary>
        private static volatile bool s_running;

        /// <summary>
        /// Whether a server runs, which the HTTP one can't beside it (GEN3-GAP(net-threads)).
        /// </summary>
        internal static bool IsRunning => s_running;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        /// <remarks>
        /// Not CommandType.Network: 'ftp /stop' must stay usable after the network configuration is gone,
        /// starting a server checks the configuration itself.
        /// </remarks>
        public CommandFtp(string[] commandvalues) : base(commandvalues)
        {
            Description = "to start or stop a FTP server (port 21 by default)";
        }

        /// <summary>
        /// CommandFtp
        /// </summary>
        public override ReturnInfo Execute()
        {
            return Start(Kernel.CurrentDirectory, FtpServer.DefaultPort, null);
        }

        /// <summary>
        /// CommandFtp
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return Execute();
            }

            if (arguments[0] == "/stop")
            {
                if (arguments.Count != 1)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }

                if (s_server == null || !s_running)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "The FTP server is not running.");
                }

                // Listen() disconnects its clients and returns within ~10 ms; a closed server can't listen again.
                s_server.Close();
                Console.WriteLine("FTP server stopped.");

                return new ReturnInfo(this, ReturnCode.OK);
            }

            if (arguments.Count > 3)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string root = AuraPath.Resolve(arguments[0]);

            int port = FtpServer.DefaultPort;
            if (arguments.Count > 1 && (!int.TryParse(arguments[1], out port) || port < 1 || port > 65535))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Invalid port: " + arguments[1]);
            }

            // The address PASV replies name: 127.0.0.1 lets a client on the QEMU host reach the forwarded passive ports.
            IPAddress passiveAddress = null;
            if (arguments.Count > 2 && !IPAddress.TryParse(arguments[2], out passiveAddress))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Invalid IPv4 address: " + arguments[2]);
            }

            return Start(root, port, passiveAddress);
        }

        /// <summary>
        /// Starts the FTP server on a thread of its own, so the terminal stays usable.
        /// </summary>
        private ReturnInfo Start(string root, int port, IPAddress passiveAddress)
        {
            // GEN3-GAP(net-misc): FtpServer can't detect a port clash, so only one server at a time.
            if (s_running)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "The FTP server already runs on port " + s_server.Port + ", use 'ftp /stop' first.");
            }

            // GEN3-GAP(net-threads): one background user of the lock-free network stack at a time.
            if (CommandHttpd.IsRunning)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "The HTTP server runs: only one background server at a time, use 'httpd /stop' first.");
            }

            if (!NetworkHelper.IsConfigured)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "No network configuration detected! Use ipconfig /ask or ipconfig /set.");
            }

            FtpServer server;
            try
            {
                server = new FtpServer(root, port)
                {
                    // Runs on the FTP thread: never Console nor Logs (not thread-safe), serial only.
                    Log = static message => Cosmos.Kernel.System.Diagnostics.Log.WriteString(message + "\n"),
                    PassiveAddress = passiveAddress,
                };
            }
            catch (DirectoryNotFoundException ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
            catch (ArgumentException ex)
            {
                // Empty root, port out of range.
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            s_server = server;
            s_running = true;

            string error = null;
            try
            {
                // GEN3-GAP(net-threads): the network stack has no locks; the FTP thread is the only background user.
                // GEN3-GAP(gc-conservative): FtpServer.Listen sleeps in Thread.Sleep; on kernels without the
                // conservative-root GC fix the first collection on the UI thread corrupts its wait monitor (#GP).
                new global::System.Threading.Thread(() => Serve(server)).Start();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (error != null)
            {
                s_running = false;
                return new ReturnInfo(this, ReturnCode.ERROR, "Can't start the FTP thread: " + error);
            }

            string address = NetworkHelper.CurrentAddress is null ? "0.0.0.0" : NetworkHelper.CurrentAddress.ToString();

            Console.WriteLine("FTP server serving " + server.RootDirectory + " at " + address + ":" + port
                + " (passive ports " + server.PassivePortMin + "-" + server.PassivePortMax + "), use 'ftp /stop' to stop it.");

            if (passiveAddress is null)
            {
                Console.WriteLine("Under QEMU, add 127.0.0.1 as pasv-ip for clients that connect where PASV says, such as FileZilla.");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// FTP thread body: serves clients until 'ftp /stop'.
        /// </summary>
        private static void Serve(FtpServer server)
        {
            // No terminal to report to from this thread: whatever stops the server goes to the serial log.
            try
            {
                server.Listen();
            }
            catch (Exception ex)
            {
                Cosmos.Kernel.System.Diagnostics.Log.WriteString("[FTP] Server stopped: " + ex.Message + "\n");
            }

            s_running = false;
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - ftp                           Start a FTP server on the current directory (port 21)");
            Console.WriteLine(" - ftp {path} [port] [pasv-ip]   Start a FTP server on a custom path (/0/ is the first volume)");
            Console.WriteLine(" - ftp /stop                     Stop the FTP server");
            Console.WriteLine("The server runs in the background (anonymous login, up to 8 clients).");
            Console.WriteLine("Under QEMU, forward the ports (hostfwd=tcp::2121-:21 and tcp::50000-:50000 ... 50009)");
            Console.WriteLine("and use 127.0.0.1 as pasv-ip.");
        }
    }
}