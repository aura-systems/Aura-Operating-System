/*
* PROJECT:          Aura Operating System Development
* CONTENT:          HttpRequest class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   David Jeske
*                   Barend Erasmus
* LICENSE:          LICENSES\SimpleHttpServer\LICENSE.md
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleHttpServer.Models
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpRequest()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Content))
            {
                if (!Headers.ContainsKey("Content-Length"))
                {
                    Headers.Add("Content-Length", Content.Length.ToString());
                }
                    
            }   

            //make string from fields
            var sb = new StringBuilder();
            sb.Append(Method + " " + Url + " HTTP/1.0\r\n");
            foreach (var header in Headers)
            {
                sb.Append(header.Key + ": " + header.Value + "\r\n");
            }
            sb.Append("\r\n");
            sb.Append(Content);

            return sb.ToString();
        }
    }
}
