/*
* PROJECT:          Aura Operating System Development
* CONTENT:          HttpServer class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   David Jeske
*                   Barend Erasmus
* LICENSE:          LICENSES\SimpleHttpServer\LICENSE.md
*/

using Cosmos.System.Network.IPv4.TCP;
using SimpleHttpServer.Models;
using System.Collections.Generic;

namespace SimpleHttpServer
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



