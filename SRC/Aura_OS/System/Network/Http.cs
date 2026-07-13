/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Http utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.UDP.DNS;

namespace Aura_OS.System.Network
{
    /// <summary>
    /// Minimal HTTP/1.1 GET client. CosmosHttp has no gen3 equivalent, so the
    /// request is hand-rolled over the plugged System.Net.Sockets
    /// TcpClient / NetworkStream transport.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Maximum time in seconds spent waiting for the HTTP response.
        /// </summary>
        private const int ResponseTimeoutSeconds = 30;

        public static byte[] DownloadRawFile(string url)
        {
            return Get(url);
        }

        public static string DownloadFile(string url)
        {
            return Encoding.ASCII.GetString(Get(url));
        }

        /// <summary>
        /// Performs an HTTP/1.1 GET request and returns the response body.
        /// </summary>
        private static byte[] Get(string url)
        {
            if (url.StartsWith("https://"))
            {
                // GEN3-GAP(https): no TLS support on Cosmos gen3, plain HTTP only.
                throw new WebException("HTTPS currently not supported, please use http://");
            }

            string path = ExtractPathFromUrl(url);
            string domainName = ExtractDomainNameFromUrl(url);

            string host = domainName;
            int port = 80;
            int portSeparator = domainName.IndexOf(':');
            if (portSeparator != -1)
            {
                host = domainName.Substring(0, portSeparator);
                if (!int.TryParse(domainName.Substring(portSeparator + 1), out port))
                {
                    port = 80;
                }
            }

            Address address = ResolveAddress(host);
            if (address == null)
            {
                throw new WebException("Failed to resolve '" + host + "' to an IPv4 address.");
            }

            var tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(IPAddress.Parse(address.ToString()), port);

                NetworkStream stream = tcpClient.GetStream();

                string request = "GET " + path + " HTTP/1.1\r\n" +
                    "Host: " + domainName + "\r\n" +
                    "User-Agent: AuraOS\r\n" +
                    "Connection: close\r\n" +
                    "\r\n";
                byte[] requestBytes = Encoding.ASCII.GetBytes(request);
                stream.Write(requestBytes, 0, requestBytes.Length);

                byte[] response = ReadResponse(stream, tcpClient);

                return ExtractBody(response);
            }
            finally
            {
                try
                {
                    tcpClient.Close();
                }
                catch
                {
                    // GEN3-GAP(tcp-close): gen3 SocketPlug.Close throws if the FIN
                    // handshake does not complete within 5s; with "Connection: close"
                    // the server closes first, so this is only a safety net.
                }
            }
        }

        /// <summary>
        /// Resolves a host name to an IPv4 address using the Cosmos DNS client.
        /// </summary>
        private static Address ResolveAddress(string host)
        {
            // GEN3-GAP(dns): System.Net.Dns is not plugged on gen3 (TcpClient only
            // accepts IP literals), so resolution stays on the Cosmos DnsClient.
            Address ipLiteral = Address.Parse(host);
            if (ipLiteral != null)
            {
                return ipLiteral;
            }

            if (DNSConfig.DNSNameservers.Count == 0)
            {
                throw new WebException("No DNS server configured, please use dhcp first.");
            }

            var dnsClient = new DnsClient();

            dnsClient.Connect(DNSConfig.DNSNameservers[0]);
            dnsClient.SendAsk(host);
            Address address = dnsClient.Receive();
            dnsClient.Close();

            return address;
        }

        /// <summary>
        /// Reads the raw HTTP response (headers + body). Stops once Content-Length
        /// bytes of body arrived, or once the peer closed the connection and the
        /// receive buffer is drained, or after <see cref="ResponseTimeoutSeconds"/>.
        /// </summary>
        private static byte[] ReadResponse(NetworkStream stream, TcpClient tcpClient)
        {
            var response = new MemoryStream();
            byte[] buffer = new byte[4096];
            int headerEnd = -1;
            int contentLength = -1;
            DateTime deadline = DateTime.Now.AddSeconds(ResponseTimeoutSeconds);

            while (DateTime.Now < deadline)
            {
                int read = stream.Read(buffer, 0, buffer.Length);

                if (read > 0)
                {
                    response.Write(buffer, 0, read);

                    if (headerEnd < 0)
                    {
                        headerEnd = FindBodyStart(response.GetBuffer(), (int)response.Length);
                        if (headerEnd >= 0)
                        {
                            contentLength = GetContentLength(Encoding.ASCII.GetString(response.GetBuffer(), 0, headerEnd));
                        }
                    }

                    if (headerEnd >= 0 && contentLength >= 0 && response.Length >= headerEnd + contentLength)
                    {
                        break;
                    }
                }
                else
                {
                    // gen3 Read() returns 0 both on "no data yet" and on "connection
                    // closed and drained": stop only once the peer has closed
                    // (expected with "Connection: close") and nothing is left.
                    if (!tcpClient.Connected && !stream.DataAvailable)
                    {
                        break;
                    }
                }
            }

            if (headerEnd < 0)
            {
                throw new WebException("No HTTP response received.");
            }

            return response.ToArray();
        }

        /// <summary>
        /// Splits the response into headers and body, honoring Content-Length and
        /// decoding chunked transfer encoding when present.
        /// </summary>
        private static byte[] ExtractBody(byte[] response)
        {
            int bodyStart = FindBodyStart(response, response.Length);
            if (bodyStart < 0)
            {
                throw new WebException("Malformed HTTP response.");
            }

            string headers = Encoding.ASCII.GetString(response, 0, bodyStart);

            int bodyLength = response.Length - bodyStart;
            int contentLength = GetContentLength(headers);
            if (contentLength >= 0 && contentLength < bodyLength)
            {
                bodyLength = contentLength;
            }

            byte[] body = new byte[bodyLength];
            Array.Copy(response, bodyStart, body, 0, bodyLength);

            if (IsChunked(headers))
            {
                return DecodeChunked(body);
            }

            return body;
        }

        /// <summary>
        /// Returns the index of the first body byte (right after the blank line
        /// terminating the headers), or -1 if the headers are incomplete.
        /// </summary>
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

        private static int GetContentLength(string headers)
        {
            string[] lines = headers.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(line.Substring("Content-Length:".Length).Trim(), out int length))
                    {
                        return length;
                    }
                }
            }

            return -1;
        }

        private static bool IsChunked(string headers)
        {
            string[] lines = headers.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("Transfer-Encoding:", StringComparison.OrdinalIgnoreCase))
                {
                    return line.IndexOf("chunked", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Decodes an HTTP/1.1 chunked transfer encoded body.
        /// </summary>
        private static byte[] DecodeChunked(byte[] body)
        {
            var decoded = new MemoryStream();
            int position = 0;

            while (position < body.Length)
            {
                int lineEnd = position;
                while (lineEnd + 1 < body.Length && !(body[lineEnd] == '\r' && body[lineEnd + 1] == '\n'))
                {
                    lineEnd++;
                }

                if (lineEnd + 1 >= body.Length)
                {
                    break;
                }

                string sizeLine = Encoding.ASCII.GetString(body, position, lineEnd - position);
                int semicolon = sizeLine.IndexOf(';');
                if (semicolon != -1)
                {
                    sizeLine = sizeLine.Substring(0, semicolon);
                }

                if (!int.TryParse(sizeLine.Trim(), global::System.Globalization.NumberStyles.HexNumber, null, out int chunkSize) || chunkSize <= 0)
                {
                    break;
                }

                position = lineEnd + 2;

                if (position + chunkSize > body.Length)
                {
                    chunkSize = body.Length - position;
                }

                decoded.Write(body, position, chunkSize);
                position += chunkSize + 2; // skip chunk data + trailing CRLF
            }

            return decoded.ToArray();
        }

        private static string ExtractDomainNameFromUrl(string url)
        {
            int start;
            if (url.Contains("://"))
            {
                start = url.IndexOf("://") + 3;
            }
            else
            {
                start = 0;
            }

            int end = url.IndexOf("/", start);
            if (end == -1)
            {
                end = url.Length;
            }

            return url[start..end];
        }


        private static string ExtractPathFromUrl(string url)
        {
            int start;
            if (url.Contains("://"))
            {
                start = url.IndexOf("://") + 3;
            }
            else
            {
                start = 0;
            }

            int indexOfSlash = url.IndexOf("/", start);
            if (indexOfSlash != -1)
            {
                return url.Substring(indexOfSlash);
            }
            else
            {
                return "/";
            }
        }
    }
}
