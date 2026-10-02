/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.Kernel.System.Timer;
using Aura_OS.System.Processing.Interpreter;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
{
    class CommandUdp : ICommand
    {
        private const int DefaultListenTimeoutSeconds = 30;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandUdp(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to send or received UDP packets";
        }

        /// <summary>
        /// CommandEcho
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
            if (arguments[0] == "/l")
            {
                if (arguments.Count < 2 || arguments.Count > 3)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }

                int port;
                if (!int.TryParse(arguments[1], out port) || port < 1 || port > 65535)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Invalid port: " + arguments[1]);
                }

                // gen2 waited forever (ESC does not reach a command running in the GUI terminal): bounded wait.
                int timeoutSeconds = DefaultListenTimeoutSeconds;
                if (arguments.Count == 3 && (!int.TryParse(arguments[2], out timeoutSeconds) || timeoutSeconds < 1 || timeoutSeconds > 86400))
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Invalid timeout: " + arguments[2]);
                }

                Console.WriteLine("Listening at " + port + " for " + timeoutSeconds + "s...");

                UdpClient client = null;
                byte[] received = null;
                string remote = null;
                string error = null;

                try
                {
                    client = new UdpClient(port);

                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    // The BCL Receive returns an empty array at once when nothing is queued: wait first.
                    // GEN3-GAP(socket-misc): UDP Available counts queued datagrams, not bytes.
                    int timeoutMs = timeoutSeconds * 1000;
                    int waited = 0;
                    while (client.Available == 0 && waited < timeoutMs)
                    {
                        TimerManager.Wait(50);
                        waited += 50;
                    }

                    if (client.Available > 0)
                    {
                        received = client.Receive(ref remoteIpEndPoint);

                        // GEN3-GAP(ipaddress): explicit ToString(), interpolation prints 0.0.0.0.
                        // The IPEndPoint plug returns a null Address for an endpoint it doesn't know.
                        IPAddress remoteAddress = remoteIpEndPoint == null ? null : remoteIpEndPoint.Address;
                        remote = (remoteAddress is null ? "?" : remoteAddress.ToString()) + ":"
                            + (remoteIpEndPoint == null ? "?" : remoteIpEndPoint.Port.ToString());
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                // gen3: finally/using do not run on the exception path, close explicitly.
                if (client != null)
                {
                    try
                    {
                        client.Close();
                    }
                    catch (Exception)
                    {
                    }
                }

                if (error != null)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, error);
                }

                if (received == null)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "No UDP packet received at " + port + " after " + timeoutSeconds + "s.");
                }

                Console.WriteLine("Received UDP packet from " + remote + ": \"" + Encoding.ASCII.GetString(received) + "\"");

                return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (arguments[0] == "/s")
            {
                if (arguments.Count <= 3)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }

                // GEN3-GAP(ipaddress): the plugged IPAddress.Parse returns null instead of throwing.
                IPAddress ip = IPAddress.Parse(arguments[1]);
                if (ip is null)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse IPv4 address: " + arguments[1]);
                }

                int port;
                if (!int.TryParse(arguments[2], out port) || port < 1 || port > 65535)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Invalid port: " + arguments[2]);
                }

                string message = string.Join(" ", arguments.GetRange(3, arguments.Count - 3));
                byte[] data = Encoding.ASCII.GetBytes(message);

                UdpClient xClient = null;
                string error = null;

                try
                {
                    xClient = new UdpClient();
                    xClient.Send(data, data.Length, new IPEndPoint(ip, port)); // SendTo pumps the stack
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                if (xClient != null)
                {
                    try
                    {
                        xClient.Close();
                    }
                    catch (Exception)
                    {
                    }
                }

                if (error != null)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, error);
                }

                Console.WriteLine("Sent UDP packet to " + ip.ToString() + ":" + port);

                return new ReturnInfo(this, ReturnCode.OK);
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - udp /l {port} [timeout_s]           listen for an UDP packet at a specific port (default timeout 30s)");
            Console.WriteLine(" - udp /s {ip} {port} {text_message}   send an UDP packet to and IP/port");
        }
    }
}