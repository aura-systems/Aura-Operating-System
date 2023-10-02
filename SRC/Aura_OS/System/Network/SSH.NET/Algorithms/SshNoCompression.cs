using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public sealed class SshNoCompression : CompressionAlgorithm
    {
        public SshNoCompression()
            : base()
        {
        }

        public override string Name
        {
            get { return "none"; }
        }

        public override byte[] Compress(byte[] input)
        {
            return input;
        }

        public override byte[] Decompress(byte[] input)
        {
            return input;
        }

        public override object Clone()
        {
            return new SshNoCompression();
        }
    }
}
