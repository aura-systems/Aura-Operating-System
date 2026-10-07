/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Http utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using Cosmos.Network.Http;

namespace Aura_OS.System.Network
{
    /// <summary>
    /// Aura's HTTP requests, through the Cosmos.Network.Http package (.NET nanoFramework's System.Net.Http ported to
    /// Cosmos): http:// and https://, whose TLS is BouncyCastle's (managed), checked against the Mozilla roots the
    /// package embeds.
    /// GEN3-GAP(http-tls): no HttpClient and no SslStream in gen3.
    /// </summary>
    /// <remarks>
    /// Requests run on the calling thread, the UI one included: the package waits by polling its socket, never in
    /// Thread.Sleep (rule C9), and drives TLS without blocking. Failures throw HttpRequestException, whose innermost
    /// exception says why (see <see cref="Describe"/>), or UriFormatException for a malformed URL.
    /// </remarks>
    public static class Http
    {
        /// <summary>
        /// How long a server may stay silent before a request fails: the package waits as long as asked otherwise.
        /// </summary>
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The redirects a request follows, which nanoFramework's HttpClient doesn't.
        /// </summary>
        private const int MaxRedirects = 5;

        /// <summary>
        /// A client that names Aura as its user agent and offers TLS 1.3 and 1.2.
        /// </summary>
        public static HttpClient CreateClient()
        {
            HttpClient client = new HttpClient
            {
                Timeout = Timeout,
                // TLS 1.3 and 1.2: the package's default is 1.2 only, as nanoFramework's.
                SslProtocols = SslProtocols.None,
            };
            client.DefaultRequestHeaders.Add("User-Agent", "AuraOS/" + Kernel.Version);
            return client;
        }

        /// <summary>
        /// A GET request that follows redirects (but not from https:// to http://).
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="completion">ResponseContentRead to have the body read, ResponseHeadersRead to read it from
        /// the content's stream (Content.CopyTo), as a download to a file does.</param>
        /// <returns>The response, an error status included: the caller disposes it.</returns>
        /// <exception cref="HttpRequestException">The request failed: its message names the URL and why.</exception>
        public static HttpResponseMessage Get(string url, HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead)
        {
            HttpClient client = CreateClient();
            Uri uri = new Uri(url);

            for (int redirects = 0; ; redirects++)
            {
                HttpResponseMessage response;
                try
                {
                    response = client.Get(uri.AbsoluteUri, completion);
                }
                catch (HttpRequestException ex)
                {
                    // The cause, which the package wraps in "An error occurred while sending the request", for every
                    // caller (wget, the package manager, the Lua API).
                    throw new HttpRequestException(uri.AbsoluteUri + ": " + Describe(ex), ex);
                }

                int status = (int)response.StatusCode;
                if (status < 300 || status > 399 || status == 304 || redirects == MaxRedirects
                    || !response.Headers.TryGetValues("Location", out IEnumerable<string> locations))
                {
                    return response;
                }

                string location = null;
                foreach (string value in locations)
                {
                    location = value;
                    break;
                }

                // Disposed before the Location is read, which may not be a URL: its connection would stay open.
                response.Dispose();

                Uri next = location == null || !Uri.TryCreate(uri, location, out Uri resolved) ? null : resolved;
                if (next == null || (uri.Scheme == Uri.UriSchemeHttps && next.Scheme != Uri.UriSchemeHttps)
                    || (next.Scheme != Uri.UriSchemeHttp && next.Scheme != Uri.UriSchemeHttps))
                {
                    throw new HttpRequestException(uri.AbsoluteUri + " redirects to " + (next == null ? location ?? "nowhere" : next.AbsoluteUri) + ", which isn't followed.");
                }

                uri = next;
            }
        }

        /// <summary>
        /// Downloads a text file (UTF-8, BOM skipped).
        /// </summary>
        public static string DownloadFile(string url)
        {
            byte[] content = DownloadRawFile(url);

            int bom = content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF ? 3 : 0;
            return Encoding.UTF8.GetString(content, bom, content.Length - bom);
        }

        /// <summary>
        /// Downloads a file as is.
        /// </summary>
        public static byte[] DownloadRawFile(string url)
        {
            HttpResponseMessage response = Get(url);

            // Disposed here rather than by a using, whose finally block a Cosmos kernel skips while an exception
            // unwinds: the connection would stay open.
            byte[] content;
            try
            {
                response.EnsureSuccessStatusCode();
                content = response.Content.ReadAsByteArray();
            }
            catch (HttpRequestException)
            {
                int status = (int)response.StatusCode;
                string reason = response.ReasonPhrase;
                string location = response.RequestMessage.RequestUri.AbsoluteUri;
                response.Dispose();
                throw new HttpRequestException(location + ": " + (status + " " + reason).TrimEnd());
            }

            response.Dispose();
            return content;
        }

        /// <summary>
        /// Why a request failed: the message Get gives its HttpRequestException (the URL and the cause), else the
        /// innermost exception's, as the package wraps the cause ("An error occurred while sending the request").
        /// </summary>
        public static string Describe(Exception ex)
        {
            if (ex is HttpRequestException && ex.InnerException is HttpRequestException)
            {
                // Get's.
                return ex.Message;
            }

            Exception cause = ex;
            while (cause.InnerException != null)
            {
                cause = cause.InnerException;
            }

            return cause.Message;
        }
    }
}
