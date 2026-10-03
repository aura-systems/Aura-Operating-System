/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Http utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Network.Http;

namespace Aura_OS.System.Network
{
    /// <summary>
    /// Aura's HTTP requests, through the Cosmos.Network.Http package: http:// and https://, whose TLS
    /// is BouncyCastle's (managed), checked against the Mozilla roots the package embeds.
    /// GEN3-GAP(http-tls): no HttpClient and no SslStream in gen3.
    /// </summary>
    /// <remarks>
    /// Requests run on the calling thread, the UI one included: the package waits in Socket.Poll,
    /// never in Thread.Sleep (rule C9), and drives TLS without blocking. Failures throw HttpException
    /// (no address, timeout, TLS handshake, untrusted certificate, error status...) or
    /// NotSupportedException (a scheme other than http:// and https://).
    /// </remarks>
    public static class Http
    {
        /// <summary>
        /// A GET request that names Aura as its user agent.
        /// </summary>
        public static HttpRequest CreateRequest(string url)
        {
            return new HttpRequest(url) { Headers = { ["User-Agent"] = "AuraOS/" + Kernel.Version } };
        }

        /// <summary>
        /// Downloads a text file (UTF-8 unless the server names another charset, BOM skipped).
        /// </summary>
        public static string DownloadFile(string url)
        {
            return CreateRequest(url).Send().EnsureSuccessStatusCode().GetString();
        }

        /// <summary>
        /// Downloads a file as is.
        /// </summary>
        public static byte[] DownloadRawFile(string url)
        {
            return CreateRequest(url).Send().EnsureSuccessStatusCode().Content;
        }
    }
}
