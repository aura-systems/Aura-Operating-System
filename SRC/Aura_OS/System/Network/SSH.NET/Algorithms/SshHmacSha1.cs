using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshHmacSha1 : MacAlgorithm
    {
        public SshHmacSha1()
            : base()
        {
            _algorithm = new HMACSHA1();
        }

        public override string Name
        {
            get { return "hmac-sha1"; }
        }

        public override int DigestLength
        {
            get { return _algorithm.HashSize / 8; }
        }

        public override object Clone()
        {
            return new SshHmacSha1();
        }
    }
}
