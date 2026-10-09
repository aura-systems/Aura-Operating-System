/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Httpd command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Network;
using Cosmos.Network.Http;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
{
    class CommandHttpd : ICommand
    {
        private const int DefaultPort = 80;

        // A file is sent in pieces of this size: no whole file in memory.
        private const int ChunkSize = 16 * 1024;

        // FileStream's buffer: 1 turns it off, as in Entries.
        private const int UnbufferedSize = 1;

        /// <summary>
        /// The background HTTP server, if one was started.
        /// </summary>
        private static HttpListener s_listener;

        private static int s_port;

        /// <summary>
        /// True from the start of a server until its thread has returned.
        /// </summary>
        private static volatile bool s_running;

        /// <summary>
        /// Whether a server runs, which the FTP one can't beside it (GEN3-GAP(net-threads)).
        /// </summary>
        internal static bool IsRunning => s_running;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        /// <remarks>
        /// Not CommandType.Network: 'httpd /stop' must stay usable after the network configuration is gone,
        /// starting a server checks the configuration itself.
        /// </remarks>
        public CommandHttpd(string[] commandvalues) : base(commandvalues)
        {
            Description = "to start or stop a HTTP server (port 80 by default)";
        }

        /// <summary>
        /// CommandHttpd
        /// </summary>
        public override ReturnInfo Execute()
        {
            return Start(Kernel.CurrentDirectory, DefaultPort);
        }

        /// <summary>
        /// CommandHttpd
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

                if (s_listener == null || !s_running)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "The HTTP server is not running.");
                }

                // The server thread closes the sockets: within ~50 ms when waiting for a request, else once the
                // request it serves is answered (HttpListener.Stop from another thread only asks).
                s_listener.Stop();
                Console.WriteLine("HTTP server stopped.");

                return new ReturnInfo(this, ReturnCode.OK);
            }

            if (arguments.Count > 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string root = AuraPath.Resolve(arguments[0]);

            int port = DefaultPort;
            if (arguments.Count > 1 && (!int.TryParse(arguments[1], out port) || port < 1 || port > 65535))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Invalid port: " + arguments[1]);
            }

            return Start(root, port);
        }

        /// <summary>
        /// Starts the HTTP server on a thread of its own, so the terminal stays usable.
        /// </summary>
        private ReturnInfo Start(string root, int port)
        {
            if (s_running)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "The HTTP server already runs on port " + s_port + ", use 'httpd /stop' first.");
            }

            // GEN3-GAP(net-threads): one background user of the lock-free network stack at a time.
            if (CommandFtp.IsRunning)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "The FTP server runs: only one background server at a time, use 'ftp /stop' first.");
            }

            if (!NetworkHelper.IsConfigured)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "No network configuration detected! Use ipconfig /ask or ipconfig /set.");
            }

            if (!Directory.Exists(root))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "No such directory: " + root);
            }

            root = AuraPath.AsDirectory(root);

            HttpListener listener = new HttpListener("http", port);
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Can't listen on port " + port + ": " + ex.Message);
            }

            s_listener = listener;
            s_port = port;
            s_running = true;

            string error = null;
            try
            {
                // GEN3-GAP(net-threads): the network stack has no locks; like the FTP one, the server thread is a
                // background user of it, and the only one (see IsRunning). HttpListener.GetContext sleeps between
                // rounds, so it can't run on the UI thread (rule C9). A thread's stack is never freed: each start
                // costs 256 KiB, as each FTP one does.
                new global::System.Threading.Thread(() => Serve(listener, root)).Start();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (error != null)
            {
                s_running = false;
                listener.Close();
                return new ReturnInfo(this, ReturnCode.ERROR, "Can't start the HTTP thread: " + error);
            }

            string address = NetworkHelper.CurrentAddress is null ? "0.0.0.0" : NetworkHelper.CurrentAddress.ToString();

            Console.WriteLine("HTTP server serving " + root + " at http://" + address + ":" + port + "/, use 'httpd /stop' to stop it.");
            Console.WriteLine("Under QEMU, forward the port (hostfwd=tcp::8080-:" + port + ") to reach it from the host.");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// HTTP thread body: serves requests until 'httpd /stop'.
        /// </summary>
        /// <remarks>
        /// Runs on the HTTP thread: never Console nor Logs (not thread-safe), serial only.
        /// </remarks>
        private static void Serve(HttpListener listener, string root)
        {
            ThreadNames.NameCurrent("HTTP server");

            while (listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = listener.GetContext();
                }
                catch (Exception ex)
                {
                    Log("Server stopped: " + ex.Message);
                    break;
                }

                if (context == null)
                {
                    // Stopped.
                    break;
                }

                try
                {
                    Handle(context, root);
                }
                catch (Exception ex)
                {
                    // A client gone, a request that couldn't be read, a file that couldn't be.
                    Log("Request failed: " + ex.Message);
                    Fail(context);
                }
            }

            // On this thread, which the sockets are (a Stop from the UI thread only asks).
            listener.Close();
            s_running = false;
        }

        /// <summary>
        /// Ends a request whose handling failed: a 500 when nothing was sent yet, the connection closed after it.
        /// </summary>
        private static void Fail(HttpListenerContext context)
        {
            try
            {
                HttpListenerResponse response = context.Response;
                if (!response.HasStarted)
                {
                    response.KeepAlive = false;
                    SendText(response, "GET", 500, "The request failed.");
                }
            }
            catch
            {
                // The client is gone, or the request couldn't be read.
            }

            context.Close();
        }

        private static void Handle(HttpListenerContext context, string root)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            response.KeepAlive = request.KeepAlive;

            string method = request.HttpMethod;
            string url = request.RawUrl ?? "/";
            Log(method + " " + url);

            if (method != "GET" && method != "HEAD")
            {
                response.Headers.Add("Allow", "GET, HEAD");
                SendText(response, method, 405, "Only GET and HEAD are served.");
                return;
            }

            string relative = MapPath(url);
            if (relative == null)
            {
                SendText(response, method, 400, "Bad path: " + url);
                return;
            }

            string path = root + relative;

            if (Directory.Exists(path))
            {
                string urlPath = PathOf(url);
                if (!urlPath.EndsWith("/"))
                {
                    // Relative links in the directory resolve against it with the slash; RedirectLocation adds the
                    // Location header.
                    response.RedirectLocation = urlPath + "/";
                    SendText(response, method, 302, "Moved to " + urlPath + "/");
                    return;
                }

                string index = AuraPath.AsDirectory(path) + "index.html";
                if (File.Exists(index))
                {
                    SendFile(response, method, index);
                }
                else
                {
                    SendListing(response, method, urlPath, AuraPath.AsDirectory(path));
                }

                return;
            }

            if (File.Exists(path))
            {
                SendFile(response, method, path);
                return;
            }

            SendText(response, method, 404, "Not found: " + url);
        }

        /// <summary>
        /// The path of a request's URL, decoded and relative to the served directory, or null when it would leave it.
        /// </summary>
        private static string MapPath(string url)
        {
            // A '+' in a path is itself, not a space as in a query.
            string decoded = HttpUtility.UrlDecode(PathOf(url).Replace("+", "%2B"));

            var segments = new List<string>();
            foreach (string segment in decoded.Split('/'))
            {
                if (segment.Length == 0 || segment == ".")
                {
                    continue;
                }

                if (segment == ".." || segment.IndexOf('\\') >= 0 || segment.IndexOf(':') >= 0)
                {
                    return null;
                }

                segments.Add(segment);
            }

            return string.Join("/", segments);
        }

        /// <summary>
        /// A request URL without its query and fragment.
        /// </summary>
        private static string PathOf(string url)
        {
            int cut = url.IndexOfAny(new char[] { '?', '#' });
            return cut >= 0 ? url.Substring(0, cut) : url;
        }

        private static void SendFile(HttpListenerResponse response, string method, string path)
        {
            // GEN3-GAP(finally): the stream is disposed explicitly on every path, as in Entries.
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, UnbufferedSize);

            Exception error = null;
            try
            {
                response.ContentType = ContentTypeOf(path);
                response.ContentLength64 = stream.Length;

                if (method == "GET")
                {
                    byte[] buffer = new byte[ChunkSize];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        response.OutputStream.Write(buffer, 0, read);
                    }
                }

                response.Close();
            }
            catch (Exception ex)
            {
                error = ex;
            }

            stream.Dispose();

            if (error != null)
            {
                throw error;
            }
        }

        private static void SendListing(HttpListenerResponse response, string method, string urlPath, string directory)
        {
            var html = new StringBuilder();
            string title = HtmlEncode(HttpUtility.UrlDecode(urlPath));
            html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Index of ").Append(title).Append("</title></head><body>");
            html.Append("<h1>Index of ").Append(title).Append("</h1><ul>");

            if (urlPath != "/")
            {
                html.Append("<li><a href=\"../\">../</a></li>");
            }

            foreach (string entry in Directory.GetDirectories(directory))
            {
                string name = Path.GetFileName(entry.TrimEnd('/', '\\')) + "/";
                html.Append("<li><a href=\"").Append(UrlEncodePath(name.TrimEnd('/'))).Append("/\">").Append(HtmlEncode(name)).Append("</a></li>");
            }

            foreach (string entry in Directory.GetFiles(directory))
            {
                string name = Path.GetFileName(entry);
                html.Append("<li><a href=\"").Append(UrlEncodePath(name)).Append("\">").Append(HtmlEncode(name)).Append("</a></li>");
            }

            html.Append("</ul><hr><address>AuraOS/").Append(Kernel.Version).Append("</address></body></html>");

            Send(response, method, 200, "text/html; charset=utf-8", Encoding.UTF8.GetBytes(html.ToString()));
        }

        private static void SendText(HttpListenerResponse response, string method, int status, string text)
        {
            Send(response, method, status, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes(text + "\n"));
        }

        private static void Send(HttpListenerResponse response, string method, int status, string contentType, byte[] body)
        {
            response.StatusCode = status;
            response.ContentType = contentType;
            response.ContentLength64 = body.Length;

            if (method != "HEAD")
            {
                response.OutputStream.Write(body, 0, body.Length);
            }

            response.Close();
        }

        private static string ContentTypeOf(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".html":
                case ".htm":
                    return "text/html; charset=utf-8";
                case ".txt":
                case ".md":
                case ".log":
                case ".lua":
                case ".cs":
                    return "text/plain; charset=utf-8";
                case ".css":
                    return "text/css; charset=utf-8";
                case ".js":
                    return "text/javascript; charset=utf-8";
                case ".json":
                    return "application/json";
                case ".xml":
                    return "application/xml";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".svg":
                    return "image/svg+xml";
                case ".ico":
                    return "image/x-icon";
                case ".pdf":
                    return "application/pdf";
                case ".zip":
                    return "application/zip";
                case ".wasm":
                    return "application/wasm";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// A name as a URL path segment: HttpUtility.UrlEncode's, with %20 for a space, as '+' is one in a query only.
        /// </summary>
        private static string UrlEncodePath(string name)
        {
            return HttpUtility.UrlEncode(name).Replace("+", "%20");
        }

        private static string HtmlEncode(string text)
        {
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private static void Log(string message)
        {
            Cosmos.Kernel.System.Diagnostics.Log.WriteString("[HTTP] " + message + "\n");
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - httpd                     Start a HTTP server on the current directory (port 80)");
            Console.WriteLine(" - httpd {path} [port]       Start a HTTP server on a custom path (/0/ is the first volume)");
            Console.WriteLine(" - httpd /stop               Stop the HTTP server");
            Console.WriteLine("The server runs in the background: GET and HEAD, a directory's index.html or listing.");
            Console.WriteLine("Under QEMU, forward the port (hostfwd=tcp::8080-:80) to reach it from the host.");
        }
    }
}
