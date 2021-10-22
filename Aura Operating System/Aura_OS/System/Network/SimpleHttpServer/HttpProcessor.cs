// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleHttpServer
{
    public class HttpProcessor
    {

        #region Fields

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        private List<Route> Routes = new List<Route>();

        #endregion

        #region Constructors

        public HttpProcessor()
        {
        }

        #endregion

        #region Public Methods
        public void HandleClient(TcpClient tcpClient)
        {
                HttpRequest request = GetRequest(tcpClient);

                // route and handle the request...
                HttpResponse response = RouteRequest(tcpClient, request);      
          
                Console.WriteLine("{0} {1}",response.StatusCode,request.Url);
                // build a default response for errors
                if (response.Content == null) {
                    if (response.StatusCode != "200") {
                        response.ContentAsUTF8 = string.Format("{0} {1} <p> {2}", response.StatusCode, request.Url, response.ReasonPhrase);
                    }
                }

                WriteResponse(tcpClient, response);
        }

        // this formats the HTTP response...
        private static void WriteResponse(TcpClient client, HttpResponse response) {            
            if (response.Content == null) {           
                response.Content = new byte[]{};
            }
            
            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type")) {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            var sb = new StringBuilder();
            sb.Append(string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            sb.Append(string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            sb.Append("\r\n\r\n");

            client.Send(Encoding.ASCII.GetBytes(sb.ToString()));     
        }

        public void AddRoute(Route route)
        {
            this.Routes.Add(route);
        }

        #endregion

        #region Private Methods

        protected virtual HttpResponse RouteRequest(TcpClient client, HttpRequest request)
        {
            if (!Routes.Any())
                return HttpBuilder.NotFound();

            var route = Routes.SingleOrDefault(x => x.Method == request.Method);

            if (route == null)
            {
                return new HttpResponse()
                {
                    ReasonPhrase = "Method Not Allowed",
                    StatusCode = "405",

                };
            }

            // trigger the route handler...
            request.Route = route;
            try {
                var discussion = new HttpDiscussion() { Request = request, Response = null };
                route.Callable(discussion);
                return discussion.Response;
            } catch(Exception ex) {
                Console.WriteLine(ex);
                return HttpBuilder.InternalServerError();
            }
        }

        private HttpRequest GetRequest(TcpClient client)
        {
            var ep = new EndPoint(Address.Zero, 0);
            //Read Request Line
            string request = Encoding.ASCII.GetString(client.Receive(ref ep));

            var lines = request.Split("\r\n");

            string[] tokens = lines[0].Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;

            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Equals(""))
                {
                    break;
                }

                int separator = lines[i].IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + lines[i]);
                }
                string name = lines[i].Substring(0, separator);
                int pos = separator + 1;
                while ((pos < lines[i].Length) && (lines[i][pos] == ' '))
                {
                    pos++;
                }

                string value = lines[i].Substring(pos, lines[i].Length - pos);
                headers.Add(name, value);
            }

            /*

            string content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];
               
                while(bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.ASCII.GetString(bytes);
            }*/


            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = "content" //TODO: add content
            };
        }

        #endregion


    }
}
