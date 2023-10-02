using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet
{
    public class SshPublicKey
    {
        protected const int _lineMaxByteLength = 72; // Maxmimum length of each line, in bytes.
        protected const string _beginMarker = "---- BEGIN SSH2 PUBLIC KEY ----";
        protected const string _endMarker = "---- END SSH2 PUBLIC KEY ----";

        public static SshPublicKey FromFile(string fileName)
        {
            // Open file stream for reading.
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return FromStream(fileStream);
            }
        }

        public static SshPublicKey FromStream(Stream stream)
        {
            var obj = new SshPublicKey();

            using (var reader = new StreamReader(stream, Encoding.ASCII))
            {
                // Read lines from stream.
                string curLine;
                bool beginMarkerFound = false;
                bool endMarkerFound = false;
                StringBuilder headerBuilder = null;
                bool headerContinued = false;
                StringBuilder keyDataBuilder = new StringBuilder();

                while ((curLine = reader.ReadLine()) != null)
                {
                    // Check if line is begin marker.
                    if (curLine == _beginMarker)
                    {
                        beginMarkerFound = true;
                        headerBuilder = new StringBuilder();
                        continue;
                    }

                    // Check if line is end marker.
                    if (curLine == _endMarker)
                    {
                        endMarkerFound = true;
                        continue;
                    }

                    // Check that line is between begin and end markers.
                    if (!beginMarkerFound || endMarkerFound) continue;

                    // Check if line is start of header.
                    bool isHeaderStart = (curLine.IndexOf(':') >= 0);

                    if (isHeaderStart && headerBuilder.Length > 0)
                    {
                        // Read last header from text.
                        obj.ReadHeader(headerBuilder.ToString());

                        // Begin new header.
                        headerBuilder = new StringBuilder();
                    }

                    // Check if line is part of header.
                    if (isHeaderStart || headerContinued)
                    {
                        // Check if header is continued onto next line.
                        headerContinued = (curLine[curLine.Length - 1] == '\\');

                        // Append current line to current header.
                        headerBuilder.Append(curLine.Substring(0, curLine.Length - (headerContinued ?
                            1 : 0)));
                    }
                    else
                    {
                        // Check if last header has been read yet.
                        if (headerBuilder != null && headerBuilder.Length > 0)
                        {
                            // Read last header from text.
                            obj.ReadHeader(headerBuilder.ToString());

                            headerBuilder = null;
                        }

                        // Append current line to key data.
                        keyDataBuilder.Append(curLine);
                    }
                }

                // Decode key data.
                obj.KeyAndCertificatesData = Convert.FromBase64String(keyDataBuilder.ToString());
            }

            return obj;
        }

        public SshPublicKey(PublicKeyAlgorithm algorithm) : this()
        {
            this.KeyAndCertificatesData = algorithm.CreateKeyAndCertificatesData();
        }

        public SshPublicKey()
        {
            this.GeneratorUserName = null;
            this.Comment = null;
            this.CustomHeaders = new List<SshPublicKeyHeader>();
            this.KeyAndCertificatesData = null;
        }

        public string GeneratorUserName
        {
            get;
            set;
        }

        public string Comment
        {
            get;
            set;
        }

        public List<SshPublicKeyHeader> CustomHeaders
        {
            get;
            protected set;
        }

        public byte[] KeyAndCertificatesData
        {
            get;
            set;
        }

        public void Save(string fileName)
        {
            // Open file stream for writing.
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                Save(fileStream);
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.ASCII))
            {
                // Write begin marker.
                writer.WriteLine(_beginMarker);

                // Write standard headers.
                if (this.GeneratorUserName != null)
                    WriteHeader(writer, new SshPublicKeyHeader("Subject", this.GeneratorUserName));

                if (this.Comment != null)
                    WriteHeader(writer, new SshPublicKeyHeader("Comment", "\"" + this.Comment + "\""));

                // Write custom headers.
                if (this.CustomHeaders != null)
                    foreach (var customHeader in this.CustomHeaders) WriteHeader(writer, customHeader);

                // Writer key identifier and data, base64 encoded.
                var encodedKeyData = Convert.ToBase64String(this.KeyAndCertificatesData);

                for (int i = 0; i < encodedKeyData.Length; i += _lineMaxByteLength)
                    writer.WriteLine(encodedKeyData.Substring(i, Math.Min(_lineMaxByteLength,
                        encodedKeyData.Length - i)));

                // Write end marker.
                writer.WriteLine(_endMarker);
            }
        }

        protected void WriteHeader(StreamWriter writer, SshPublicKeyHeader header)
        {
            using (var headerStream = new MemoryStream())
            {
                // Write header tag.
                var tagData = Encoding.ASCII.GetBytes(header.Tag + ": ");
                headerStream.Write(tagData, 0, tagData.Length);

                // Write header value.
                var valueData = Encoding.UTF8.GetBytes(header.Value);
                headerStream.Write(valueData, 0, valueData.Length);

                // Write header text to output stream.
                var headerText = writer.Encoding.GetString(headerStream.ToArray());
                int bytesRemaining = 0;

                for (int i = 0; i < headerText.Length; i += _lineMaxByteLength - 1)
                {
                    bytesRemaining = headerText.Length - i;

                    // Check if this is last line to write.
                    if (bytesRemaining <= _lineMaxByteLength)
                    {
                        // Write all remaining bytes.
                        writer.WriteLine(headerText.Substring(i, bytesRemaining));
                    }
                    else
                    {
                        // Write current line.
                        writer.WriteLine(headerText.Substring(i, _lineMaxByteLength - 1) + "\\");
                    }
                }
            }
        }

        protected void ReadHeader(string headerText)
        {
            // Split header text into tag and value.
            string[] headerParts = headerText.Split(new string[] { ": " }, 2, StringSplitOptions.None);
            var tag = headerParts[0];
            var value = headerParts[1];

            // Check header tag.
            switch (tag.ToLower())
            {
                case "subject":
                    this.GeneratorUserName = value;
                    break;
                case "comment":
                    if (value[0] == '\"' && value[value.Length - 1] == '\"')
                        this.Comment = value.Substring(1, value.Length - 2);
                    else
                        this.Comment = value;

                    break;
                default:
                    // Add custom header.
                    this.CustomHeaders.Add(new SshPublicKeyHeader(tag, value));
                    break;
            }
        }

        public string GetFingerprint()
        {
            // Compute hash of public key data.
            byte[] hash;

            using (var md5 = new MD5CryptoServiceProvider())
                hash = md5.ComputeHash(this.KeyAndCertificatesData);
            
            // Return text representation of hash.
            return string.Join(":", (from b in hash select b.ToString("x2")).ToArray());
        }
    }

    // Custom headers must have tags that begin with "x-".
    public struct SshPublicKeyHeader
    {
        private string _tag;   // Header tag.
        private string _value; // Header value.

        public SshPublicKeyHeader(string tag, string value)
        {
            _tag = tag;
            _value = value;
        }

        public string Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}
