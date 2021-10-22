// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleHttpServer.Models
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Path { get; set; } // either the Url, or the first regex group
        public string Content { get; set; }
        public Route Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpRequest()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Content))
                if (!this.Headers.ContainsKey("Content-Length"))
                    this.Headers.Add("Content-Length", this.Content.Length.ToString());

            var sb = new StringBuilder();
            sb.Append(this.Method + " " + this.Url + " HTTP/1.0\r\n");
            foreach (var header in Headers)
            {
                sb.Append(header.Key + ": " + header.Value + "\r\n");
            }
            sb.Append("\r\n");
            sb.Append(this.Content);

            return sb.ToString();
        }
    }
}
