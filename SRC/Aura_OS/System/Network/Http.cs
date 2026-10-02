/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Http utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.Kernel.System.Timer;

namespace Aura_OS.System.Network
{
    /// <summary>
    /// Error raised by the HTTP client (bad URL, DNS failure, HTTP status, timeout...).
    /// </summary>
    public sealed class HttpException : Exception
    {
        public int StatusCode { get; }

        public HttpException(string message, int statusCode = 0) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Minimal HTTP/1.1 GET client (no TLS: http:// only).
    /// GEN3-GAP(http-tls): no HttpClient and no TLS in gen3, and the gen2 HTTP package is gone.
    /// </summary>
    public static class Http
    {
        private const int IdleTimeoutMs = 15000;
        private const int MaxRedirects = 3;

        public static string DownloadFile(string url)
        {
            byte[] data = DownloadRawFile(url);

            // Skip a UTF-8 BOM so JSON readers see '{' / '[' first.
            int start = data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF ? 3 : 0;

            return Encoding.UTF8.GetString(data, start, data.Length - start);
        }

        public static byte[] DownloadRawFile(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new HttpException("No URL given.");
            }

            for (int hop = 0; hop <= MaxRedirects; hop++)
            {
                string host;
                int port;
                string path;
                ParseUrl(url, out host, out port, out path);

                byte[] raw = Get(host, port, path);

                int bodyStart = FindBodyStart(raw, raw.Length);
                if (bodyStart < 0)
                {
                    throw new HttpException("Malformed HTTP response.");
                }

                string headers = Encoding.ASCII.GetString(raw, 0, bodyStart);
                int status = ParseStatus(headers);

                if (status == 301 || status == 302 || status == 303 || status == 307 || status == 308)
                {
                    string location = GetHeader(headers, "Location");
                    if (string.IsNullOrEmpty(location))
                    {
                        throw new HttpException("Redirect without Location.", status);
                    }

                    // An https:// Location is rejected by ParseUrl on the next hop.
                    url = location.StartsWith("/") ? "http://" + host + (port == 80 ? "" : ":" + port.ToString()) + location : location;
                    continue;
                }

                if (status < 200 || status > 299)
                {
                    throw new HttpException("HTTP error " + status.ToString(), status);
                }

                return ExtractBody(raw, headers, bodyStart);
            }

            throw new HttpException("Too many redirects.");
        }

        private static byte[] Get(string host, int port, string path)
        {
            IPAddress address = Resolve(host);

            TcpClient client = new TcpClient();
            byte[] response = null;
            string error = null;

            try
            {
                // GEN3-GAP(socket-misc): bare Exception after 5 s without SYN-ACK,
                // InvalidOperationException without an IPv4 config on the primary adapter.
                client.Connect(address, port);

                // GEN3-GAP(tcp-receive): raw Socket + byte[] only (span receives drop data,
                // GetStream throws once the server has already answered and closed).
                Socket socket = client.Client;
                if (socket == null)
                {
                    // gen3: a null dereference halts the kernel, the plug returns null for an unknown client.
                    throw new InvalidOperationException("Not connected to " + host + ".");
                }

                byte[] request = Encoding.ASCII.GetBytes(
                    "GET " + path + " HTTP/1.1\r\n" +
                    "Host: " + host + (port == 80 ? "" : ":" + port.ToString()) + "\r\n" +
                    "User-Agent: AuraOS/" + Kernel.Version + "\r\n" +
                    "Accept: */*\r\nAccept-Encoding: identity\r\nConnection: close\r\n\r\n");

                socket.Send(request, 0, request.Length, SocketFlags.None);
                response = ReadToEnd(socket);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            // gen3: finally does not run on the exception path, close explicitly.
            try
            {
                client.Close();
            }
            catch (Exception)
            {
                // FIN timeout: the plug detaches the connection anyway.
            }

            // Never hand a null response to the parser (a null dereference halts the kernel).
            if (error != null || response == null)
            {
                throw new HttpException(error ?? "HTTP request to " + host + " failed.");
            }

            return response;
        }

        private static IPAddress Resolve(string host)
        {
            IPAddress literal;
            if (IPAddress.TryParse(host, out literal))
            {
                return literal;
            }

            // GEN3-GAP(socket-misc): TcpClient.Connect(string, int) does not resolve names, resolve first.
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host); // plugged: A records, DnsConfig.Nameservers[0]
                if (addresses != null && addresses.Length > 0)
                {
                    return addresses[0];
                }
            }
            catch (Exception)
            {
                // SocketException: no nameserver, no route or unknown host.
            }

            throw new HttpException("Unable to resolve " + host + " (configure the network / a DNS server first).");
        }

        /// <summary>
        /// Reads until the peer closes (Connection: close), Content-Length is reached, or idle timeout.
        /// </summary>
        private static byte[] ReadToEnd(Socket socket)
        {
            MemoryStream response = new MemoryStream();
            byte[] buffer = new byte[4096];
            long lastData = Stopwatch.GetTimestamp();
            int bodyStart = -1;
            long contentLength = -1;

            while (true)
            {
                int available = socket.Available;
                if (available > 0)
                {
                    int read = socket.Receive(buffer, 0, Math.Min(available, buffer.Length), SocketFlags.None);
                    response.Write(buffer, 0, read);
                    lastData = Stopwatch.GetTimestamp();

                    if (bodyStart < 0)
                    {
                        bodyStart = FindBodyStart(response.GetBuffer(), (int)response.Length);
                        if (bodyStart >= 0)
                        {
                            contentLength = GetContentLength(Encoding.ASCII.GetString(response.GetBuffer(), 0, bodyStart));
                        }
                    }

                    if (bodyStart >= 0 && contentLength >= 0 && response.Length >= bodyStart + contentLength)
                    {
                        break;
                    }

                    continue;
                }

                // GEN3-GAP(tcp-receive): Receive returns 0 while the connection is open.
                // Readable with nothing to read means the server closed its side.
                if (socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0)
                {
                    break;
                }

                if (Stopwatch.GetElapsedTime(lastData).TotalMilliseconds > IdleTimeoutMs)
                {
                    throw new HttpException("Timed out waiting for the server.");
                }

                TimerManager.Wait(10); // hlt loop: the UI thread must never sleep or block (C9)
            }

            return response.ToArray();
        }

        private static void ParseUrl(string url, out string host, out int port, out string path)
        {
            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("HTTPS is not supported (no TLS), use http://");
            }

            string rest = url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? url.Substring(7) : url;
            int slash = rest.IndexOf('/');
            string authority = slash < 0 ? rest : rest.Substring(0, slash);
            path = slash < 0 ? "/" : rest.Substring(slash);

            port = 80;
            int colon = authority.LastIndexOf(':');
            if (colon >= 0)
            {
                if (!int.TryParse(authority.Substring(colon + 1), out port) || port < 1 || port > 65535)
                {
                    throw new HttpException("Invalid port in " + url);
                }

                authority = authority.Substring(0, colon);
            }

            if (authority.Length == 0)
            {
                throw new HttpException("No host in " + url);
            }

            host = authority;
        }

        private static int ParseStatus(string headers)
        {
            // "HTTP/1.1 200 OK"
            int space = headers.IndexOf(' ');
            int status;

            if (!headers.StartsWith("HTTP/") || space < 0 || headers.Length < space + 4
                || !int.TryParse(headers.Substring(space + 1, 3), out status))
            {
                throw new HttpException("Malformed HTTP status line.");
            }

            return status;
        }

        private static string GetHeader(string headers, string name)
        {
            foreach (string line in headers.Split("\r\n"))
            {
                int colon = line.IndexOf(':');
                if (colon > 0 && string.Equals(line.Substring(0, colon).Trim(), name, StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(colon + 1).Trim();
                }
            }

            return null;
        }

        private static long GetContentLength(string headers)
        {
            long length;
            return long.TryParse(GetHeader(headers, "Content-Length"), out length) && length >= 0 ? length : -1;
        }

        private static int FindBodyStart(byte[] data, int length)
        {
            for (int i = 0; i + 3 < length; i++)
            {
                if (data[i] == '\r' && data[i + 1] == '\n' && data[i + 2] == '\r' && data[i + 3] == '\n')
                {
                    return i + 4;
                }
            }

            return -1;
        }

        private static byte[] ExtractBody(byte[] raw, string headers, int bodyStart)
        {
            if (string.Equals(GetHeader(headers, "Transfer-Encoding"), "chunked", StringComparison.OrdinalIgnoreCase))
            {
                return DecodeChunked(raw, bodyStart);
            }

            int available = raw.Length - bodyStart;
            long contentLength = GetContentLength(headers);
            int length = contentLength >= 0 && contentLength < available ? (int)contentLength : available;

            byte[] body = new byte[length];
            Buffer.BlockCopy(raw, bodyStart, body, 0, length);
            return body;
        }

        private static byte[] DecodeChunked(byte[] raw, int pos)
        {
            MemoryStream body = new MemoryStream();

            while (pos < raw.Length)
            {
                int lineEnd = pos;
                while (lineEnd + 1 < raw.Length && !(raw[lineEnd] == '\r' && raw[lineEnd + 1] == '\n'))
                {
                    lineEnd++;
                }

                if (lineEnd + 1 >= raw.Length)
                {
                    break;
                }

                string sizeText = Encoding.ASCII.GetString(raw, pos, lineEnd - pos);
                int semicolon = sizeText.IndexOf(';'); // chunk extensions
                if (semicolon >= 0)
                {
                    sizeText = sizeText.Substring(0, semicolon);
                }

                int size;
                if (!int.TryParse(sizeText.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out size))
                {
                    throw new HttpException("Malformed chunk size.");
                }

                pos = lineEnd + 2;
                if (size == 0)
                {
                    break; // last chunk; trailers ignored
                }

                if (pos + size > raw.Length)
                {
                    throw new HttpException("Truncated chunked body.");
                }

                body.Write(raw, pos, size);
                pos += size + 2; // skip the CRLF after the chunk data
            }

            return body.ToArray();
        }
    }
}
