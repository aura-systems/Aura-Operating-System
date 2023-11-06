/*
* PROJECT:          Aura Operating System Development
* CONTENT:          HttpServer class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   David Jeske
*                   Barend Erasmus
* LICENSE:          LICENSES\SimpleHttpServer\LICENSE.md
*/

using Aura_OS.System.Network.HTTP.SimpleHttpServer.Models;
using Cosmos.System.Network.IPv4.TCP;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Network.HTTP.SimpleHttpServer
{
    public class HttpServer
    {
        #region Fields

        private int Port;
        private TcpListener Listener;
        private HttpProcessor Processor;
        private bool IsActive = true;

        #endregion

        #region Public Methods

        public HttpServer(int port, List<Route> routes)
        {
            Port = port;
            Processor = new HttpProcessor();

            foreach (var route in routes)
            {
                Processor.AddRoute(route);
            }
        }

        public void Listen()
        {
            Listener = new TcpListener((ushort)Port);
            Listener.Start();

            Console.WriteLine("HTTP Server Listening on port 80...");

            while (IsActive)
            {
                try
                {
                    var s = Listener.AcceptTcpClient();
                    Processor.HandleClient(s);
                }
                catch
                {

                }
            }
        }

        #endregion

    }
}



