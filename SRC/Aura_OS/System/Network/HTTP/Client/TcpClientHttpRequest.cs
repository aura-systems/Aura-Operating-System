using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Aura_OS.System.Network.HTTP.Client
{
    public class TcpClientHttpRequest : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private BasicUri _remote;
        private object _close_lock = new object();
        private string _method = "GET";
        private string _action;
        private string _charset = "utf-8";
        private string _head;
        private string _data;
        private string _proxyConnection = "default";
        private int _autoSendError;
        private int _timeout = 20000;
        private int _maximumAutomaticRedirections = 500;
        private IRequestProxy _proxy;
        private HttpResponse _response;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        public TcpClientHttpRequest()
        {
            _headers.Add("Connection", "Keep-Alive");
            _headers.Add("Accept", "*/*");
            _headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2)");
            _headers.Add("Accept-Language", "en-us");
            _headers.Add("Accept-Encoding", "gzip, deflate");
        }
        public void Close()
        {
            if (_client != null)
            {
                lock (_close_lock)
                {
                    if (_client != null)
                    {
                        if (_stream != null)
                        {
                            try
                            {
                                _stream.Close();
                                _stream.Dispose();
                            }
                            finally
                            {
                                _stream = null;
                            }
                        }
                        _client.Close();
                        _client = null;
                    }
                }
            }
        }
        public void Send()
        {
            Send(string.Empty);
        }
        public virtual void Send(string data)
        {
            Send(data, 0);
        }
        private void Send(string data, int redirections)
        {
            _data = data;
            Encoding encode = Encoding.GetEncoding(_charset);
            _headers.Remove("Content-Length");
            if (!string.IsNullOrEmpty(data) && string.Compare(_method, "post", true) == 0)
            {
                _headers["Content-Length"] = string.Concat(Encoding.GetEncoding(_charset).GetBytes(data).Length);
                if (string.IsNullOrEmpty(_headers["Content-Type"]))
                {
                    _headers["Content-Type"] = "application/x-www-form-urlencoded; charset=" + _charset;
                }
                else if (_headers["Content-Type"].IndexOf("multipart/form-data") == -1)
                {
                    if (_headers["Content-Type"].IndexOf("application/x-www-form-urlencoded") == -1)
                    {
                        _headers["Content-Type"] += "; application/x-www-form-urlencoded";
                    }
                    if (_headers["Content-Type"].IndexOf("charset=") == -1)
                    {
                        _headers["Content-Type"] += "; charset=" + _charset;
                    }
                }
                data += "\r\n\r\n";
            }
            BasicUri uri = new BasicUri(_action);
            _headers["Host"] = uri.Authority;
            string http = _method + " " + uri.PathAndQuery + " HTTP/1.1\r\n";
            foreach (string head in _headers.Values)
            {
                http += head + ": " + _headers[head] + "\r\n";
            }
            http += "\r\n" + data;
            _head = http;
            if (_proxy != null)
            {
                RequestProxy pr = new RequestProxy();
                pr.Method = _method;
                pr.Action = _action;
                pr.Charset = _charset;
                pr.Head = _head;
                pr.Data = data;
                pr.Connection = _proxyConnection;
                pr.Timeout = _timeout;
                pr.MaximumAutomaticRedirections = _maximumAutomaticRedirections;
                ResponseProxy response = _proxy.Send(pr);
                Action = response.RequestAction;
                _method = response.RequestMethod;
                _headers = Utils.ParseHttpRequestHeader(response.RequestHead);
                _response = new HttpResponse(this, response.ResponseHead);
                _response.SetStream(response.Response);
            }
            else
            {
                byte[] request = encode.GetBytes(http);
                if (_client == null || _remote == null || string.Compare(_remote.Authority, uri.Authority, true) != 0)
                {
                    _remote = uri;
                    this.Close();
                    _client = new TcpClient(uri.Host, uri.Port);
                }
                try
                {
                    _stream = getStream(uri);
                    _stream.Write(request, 0, request.Length);
                }
                catch
                {
                    this.Close();
                    _client = new TcpClient(uri.Host, uri.Port);
                    _stream = getStream(uri);
                    _stream.Write(request, 0, request.Length);
                }
                receive(_stream, redirections, uri, encode);
            }
        }
        protected void receive(NetworkStream stream, int redirections, BasicUri uri, Encoding encode)
        {
            //stream.ReadTimeout = _timeout; TO PLUG
            _response = null;
            byte[] bytes = new Byte[1024];
            int overs = bytes.Length;
            MemoryStream headStream = new MemoryStream();
            MemoryStream bodyStream = new MemoryStream();
            MemoryStream chunkStream = new MemoryStream();
            Exception exception = null;
            while (overs > 0)
            {
                int idx = -1;
                try
                {
                    overs = stream.Read(bytes, 0, bytes.Length);
                    if (headStream.Length == 0 && overs == 0)
                    {
                        throw new Exception("连接已关闭");
                    }
                }
                catch (Exception e)
                {
                    if (headStream.Length == 0 && _autoSendError++ < 5)
                    {
                        headStream.Close();
                        bodyStream.Close();
                        chunkStream.Close();
                        _remote = null;
                        Send(_data, 0);
                        return;
                    }
                    exception = e;
                    break;
                }
                if (_response == null)
                {
                    headStream.Write(bytes, 0, overs);
                    bytes = headStream.ToArray();
                    idx = Utils.findBytes(bytes, new byte[] { 13, 10, 13, 10 }, 0);
                    if (idx != -1)
                    {
                        headStream.Close();
                        headStream = new MemoryStream();
                        headStream.Write(bytes, 0, idx);
                        chunkStream.Write(bytes, idx + 4, bytes.Length - idx - 4);
                        _response = new HttpResponse(this, headStream.ToArray());
                        _response.Received += bytes.Length - idx - 4;
                        if (_response.StatusCode == HttpStatusCode.Redirect ||
                            _response.StatusCode == HttpStatusCode.Moved)
                        {
                            if (string.Compare(_method, "post", true) == 0)
                            {
                                _headers.Remove("Content-Length");
                                if (!string.IsNullOrEmpty(_headers["Content-Type"]))
                                {
                                    _headers["Content-Type"] = _headers["Content-Type"]
                                        .Replace("; application/x-www-form-urlencoded", string.Empty)
                                        .Replace("application/x-www-form-urlencoded", string.Empty);
                                    if (string.IsNullOrEmpty(_headers["Content-Type"]))
                                    {
                                        _headers.Remove("Content-Type");
                                    }
                                }
                            }
                            headStream.Close();
                            bodyStream.Close();
                            chunkStream.Close();
                            this.closeTcp();
                            if (++redirections > _maximumAutomaticRedirections)
                            {
                                throw new WebException("重定向超过 " + _maximumAutomaticRedirections + " 次。");
                            }
                            string url = _response.Headers["Location"];
                            if (!string.IsNullOrEmpty(url))
                            {
                                if (BasicUri.IsWellFormedUriString(url, UriKind.Relative))
                                {
                                    if (!url.StartsWith("/"))
                                    {
                                        int eidx = Address.AbsolutePath.LastIndexOf('/');
                                        url = Address.AbsolutePath.Remove(eidx) + "/" + url;
                                    }
                                    url = Address.Scheme + "://" + Address.Authority + url;
                                    url = new BasicUri(url).AbsoluteUri;
                                }
                            }
                            Action = url;
                            Method = "get";
                            Headers.Remove("Cookie");
                            Send(null, redirections);
                            return;
                        }
                        else if (_response.StatusCode == HttpStatusCode.Continue)
                        {
                            _response = null;
                            headStream = new MemoryStream();
                        }
                    }
                }
                else
                {
                    _response.Received += overs;
                    chunkStream.Write(bytes, 0, overs);
                }
                if (_response != null)
                {
                    if (_response.ContentLength <= chunkStream.Length)
                    {
                        break;
                    }
                }
            }
            _autoSendError = 0;
            if (_response == null)
            {
                headStream.Close();
                bodyStream.Close();
                chunkStream.Close();
                this.closeTcp();
                List<string> sb = new List<string>();
                sb.Add(_method.ToUpper() + " " + new BasicUri(_action).PathAndQuery + " HTTP/1.1");
                foreach (string header in _headers.Values)
                {
                    sb.Add(header + ": " + _headers[header]);
                }
                if (exception == null)
                {
                    throw new WebException("读取失败。" + string.Join("\r\n", sb.ToArray()));
                }
                else
                {
                    throw new WebException(exception.Message + "\r\n" + string.Join("\r\n", sb.ToArray()), exception);
                }
            }
            if (string.Compare(_response.TransferEncoding, "chunked") == 0)
            {
                _response.SetStream(bodyStream.ToArray());
            }
            else if (_response.ContentLength >= 0)
            {
                _response.SetStream(chunkStream.ToArray());
            }
            else
            {
                _response.SetStream(chunkStream.ToArray());
            }
            headStream.Close();
            bodyStream.Close();
            chunkStream.Close();
            this.closeTcp();
        }
        protected bool closeTcp()
        {
            if (_client != null && _response != null && (
                string.Compare(_headers["Connection"], "keep-alive", true) != 0 ||
                string.Compare(_response.Headers["Connection"], "keep-alive", true) != 0
                ))
            {
                this.Close();
                return true;
            }
            return false;
        }
        protected NetworkStream getStream(BasicUri uri)
        {
            return _client.GetStream();
        }

        public string Method
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value.ToUpper();
            }
        }
        public string Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (string.Compare(_action, value, true) != 0)
                {
                    if (!BasicUri.IsWellFormedUriString(value, UriKind.Absolute))
                    {
                        throw new WebException("不正确的URI“" + value + "”");
                    }
                    BasicUri uri = new BasicUri(value);
                    _action = uri.AbsoluteUri;
                }
            }
        }
        public BasicUri Address
        {
            get
            {
                return new BasicUri(_action);
            }
        }
        public string Charset
        {
            get { return _charset; }
            set { _charset = value; }
        }
        public string Head
        {
            get { return _head; }
        }
        public string ProxyConnection
        {
            get { return _proxyConnection; }
            set { _proxyConnection = value; }
        }
        public int MaximumAutomaticRedirections
        {
            get { return _maximumAutomaticRedirections; }
            set { _maximumAutomaticRedirections = value; }
        }
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
        public IRequestProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }
        public HttpResponse Response
        {
            get { return _response; }
        }
        public Dictionary<string, string> Headers
        {
            get { return _headers; }
        }
        public void SetHeaders(string headers)
        {
            _headers = Utils.ParseHttpRequestHeader(headers);
        }
        public string Accept
        {
            get { return _headers["Accept"]; }
            set { _headers["Accept"] = value; }
        }
        public string AcceptLanguage
        {
            get { return _headers["Accept-Language"]; }
            set { _headers["Accept-Language"] = value; }
        }
        public string Connection
        {
            get { return _headers["Connection"]; }
            set { _headers["Connection"] = value; }
        }
        public int ContentLength
        {
            get { int tryint; int.TryParse(_headers["Content-Length"], out tryint); return tryint; }
            set { _headers["Content-Length"] = string.Concat(value); }
        }
        public string ContentType
        {
            get { return _headers["Content-Type"]; }
            set { _headers["Content-Type"] = value; }
        }
        public string Expect
        {
            get { return _headers["Expect"]; }
            set { _headers["Expect"] = value; }
        }
        public string MediaType
        {
            get { return _headers["Media-Type"]; }
            set { _headers["Media-Type"] = value; }
        }
        public string Referer
        {
            get { return _headers["Referer"]; }
            set { _headers["Referer"] = value; }
        }
        public string TransferEncoding
        {
            get { return _headers["Transfer-Encoding"]; }
            set { _headers["Transfer-Encoding"] = value; }
        }
        public string UserAgent
        {
            get { return _headers["User-Agent"]; }
            set { _headers["User-Agent"] = value; }
        }
        #region IDisposable 成员
        public void Dispose()
        {
            try
            {
                _headers.Clear();
                this.Close();
            }
            catch { }
        }
        #endregion

        #region utils
        public class Utils
        {
            public static Dictionary<string, string> ParseHttpRequestHeader(string head)
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(head))
                {
                    string[] hs = head.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    foreach (string h in hs)
                    {
                        string[] nv = h.Split(new char[] { ':' }, 2);
                        if (nv.Length == 2)
                        {
                            string n = nv[0].Trim();
                            string v = nv[1].Trim();
                            headers.Add(n, v);
                        }
                    }
                }
                return headers;
            }
            public static int findBytes(byte[] source, byte[] find, int startIndex)
            {
                if (find == null) return -1;
                if (find.Length == 0) return -1;
                if (source == null) return -1;
                if (source.Length == 0) return -1;
                if (startIndex < 0) startIndex = 0;
                int idx = -1, idx2 = startIndex - 1;
                do
                {
                    idx2 = idx = Array.FindIndex<byte>(source, Math.Min(idx2 + 1, source.Length), delegate (byte b) {
                        return b == find[0];
                    });
                    if (idx2 != -1)
                    {
                        for (int a = 1; a < find.Length; a++)
                        {
                            if (++idx2 >= source.Length || source[idx2] != find[a])
                            {
                                idx = -1;
                                break;
                            }
                        }
                        if (idx != -1) break;
                    }
                } while (idx2 != -1);
                return idx;
            }
        }
        #endregion

        #region proxy
        public interface IRequestProxy
        {
            ResponseProxy Send(RequestProxy request);
        }
        [Serializable]
        public class RequestProxy
        {
            private string _method;
            public string Method
            {
                get { return _method; }
                set { _method = value; }
            }
            private string _action;
            public string Action
            {
                get { return _action; }
                set { _action = value; }
            }
            private string _charset;
            public string Charset
            {
                get { return _charset; }
                set { _charset = value; }
            }
            private string _head;
            public string Head
            {
                get { return _head; }
                set { _head = value; }
            }
            private string _data;
            public string Data
            {
                get { return _data; }
                set { _data = value; }
            }
            private string _connection;
            public string Connection
            {
                get { return _connection; }
                set { _connection = value; }
            }
            private int _timeout;
            public int Timeout
            {
                get { return _timeout; }
                set { _timeout = value; }
            }
            private int _maximumAutomaticRedirections;
            public int MaximumAutomaticRedirections
            {
                get { return _maximumAutomaticRedirections; }
                set { _maximumAutomaticRedirections = value; }
            }
        }
        [Serializable]
        public class ResponseProxy
        {
            private string _requestMethod;
            public string RequestMethod
            {
                get { return _requestMethod; }
                set { _requestMethod = value; }
            }
            private string _requestAction;
            public string RequestAction
            {
                get { return _requestAction; }
                set { _requestAction = value; }
            }
            private string _requestHead;
            public string RequestHead
            {
                get { return _requestHead; }
                set { _requestHead = value; }
            }
            private byte[] _responseHead;
            public byte[] ResponseHead
            {
                get { return _responseHead; }
                set { _responseHead = value; }
            }
            private byte[] _response;
            public byte[] Response
            {
                get { return _response; }
                set { _response = value; }
            }
        }
        #endregion

        public partial class HttpResponse
        {
            private string _action;
            private string _method;
            private string _charset;
            private string _head;
            private string _contentEncoding = string.Empty;
            private int _contentLength = -1;
            private int _received = 0;
            private string _contentType;
            private string _server;
            private Dictionary<string, string> _headers = new Dictionary<string, string>();
            private HttpStatusCode _statusCode;
            private byte[] _stream = new byte[] { };
            private string _xml;
            public HttpResponse(TcpClientHttpRequest ie, byte[] headBytes)
            {
                _action = ie.Action;
                _method = ie.Method;
                _charset = ie.Charset;
                Encoding encode = Encoding.GetEncoding(_charset);
                string head = encode.GetString(headBytes);
                _head = head = head.Trim();
                int idx = head.IndexOf(' ');
                if (idx != -1)
                {
                    head = head.Substring(idx + 1);
                }
                idx = head.IndexOf(' ');
                if (idx != -1)
                {
                    _statusCode = (HttpStatusCode)int.Parse(head.Remove(idx));
                    head = head.Substring(idx + 1);
                }
                idx = head.IndexOf("\r\n");
                if (idx != -1)
                {
                    head = head.Substring(idx + 2);
                }
                string[] heads = head.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (string h in heads)
                {
                    string[] nv = h.Split(new char[] { ':' }, 2);
                    if (nv.Length == 2)
                    {
                        string n = nv[0].Trim();
                        string v = nv[1].Trim();
                        if (v.EndsWith("; Secure")) v = v.Replace("; Secure", "");
                        if (v.EndsWith("; version=1")) v = v.Replace("; version=1", "");
                        switch (n.ToLower())
                        {
                            case "set-cookie":
                                break;
                            case "content-length":
                                if (!int.TryParse(v, out _contentLength)) _contentLength = -1;
                                break;
                            case "content-type":
                                idx = v.IndexOf("charset=");
                                if (idx != -1)
                                {
                                    string charset = v.Substring(idx + 8);
                                    idx = charset.IndexOf(";");
                                    if (idx != -1) charset = charset.Remove(idx);
                                    if (string.Compare(_charset, charset, true) != 0)
                                    {
                                        try
                                        {
                                            Encoding testEncode = Encoding.GetEncoding(charset);
                                            _charset = charset;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                _contentType = v;
                                break;
                            case "server":
                                _server = v;
                                break;
                            case "content-encoding":
                                _contentEncoding = v;
                                break;
                            default:
                                _headers.Add(n, v);
                                break;
                        }
                    }
                }
            }
            public void SetStream(byte[] bodyBytes)
            {
                _stream = bodyBytes;
                _contentLength = bodyBytes.Length;
            }
            public byte[] GetStream()
            {
                switch (_contentEncoding.ToLower())
                {
                    case "gzip":
                    case "deflate":
                    default:
                        return _stream;
                }
            }
            public string TranslateUrlToAbsolute(string url)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    if (BasicUri.IsWellFormedUriString(url, UriKind.Relative))
                    {
                        if (!url.StartsWith("/"))
                        {
                            int eidx = Address.AbsolutePath.LastIndexOf('/');
                            url = Address.AbsolutePath.Remove(eidx) + "/" + url;
                        }
                        url = Address.Scheme + "://" + Address.Authority + url;
                        url = new BasicUri(url).AbsoluteUri;
                    }
                }
                else
                {
                    int eidx = Address.AbsolutePath.LastIndexOf('/');
                    url = Address.Scheme + "://" + Address.Authority + Address.AbsolutePath.Remove(eidx);
                }
                return url;
            }
            public string Xml
            {
                get
                {
                    if (_xml == null)
                    {
                        switch (_contentEncoding.ToLower())
                        {
                            case "gzip":
                            case "deflate":
                            default:
                                _xml = Encoding.GetEncoding(_charset).GetString(_stream);
                                break;
                        }
                    }
                    return _xml;
                }
            }
            public string Method
            {
                get
                {
                    return _method;
                }
            }
            public string Action
            {
                get
                {
                    return _action;
                }
            }
            public BasicUri Address
            {
                get
                {
                    return new BasicUri(_action);
                }
            }
            public string Charset
            {
                get { return _charset; }
            }
            public string Head
            {
                get { return _head; }
            }
            public string ContentEncoding
            {
                get { return _contentEncoding; }
            }
            public int ContentLength
            {
                get { return _contentLength; }
            }
            public int Received
            {
                get { return _received; }
                internal set { _received = value; }
            }
            public string ContentType
            {
                get { return _contentType; }
            }
            public string Server
            {
                get { return _server; }
            }
            public Dictionary<string, string> Headers
            {
                get { return _headers; }
            }
            public HttpStatusCode StatusCode
            {
                get { return _statusCode; }
            }
            public string TransferEncoding
            {
                get { return _headers["Transfer-Encoding"]; }
                set { _headers["Transfer-Encoding"] = value; }
            }
        }
    }
}