using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshHmacMd5 : MacAlgorithm
    {
        public SshHmacMd5()
            : base()
        {
            _algorithm = new HMACMD5();
        }

        public override string Name
        {
            get { return "hmac-md5"; }
        }

        public override int DigestLength
        {
            get { return _algorithm.HashSize / 8; }
        }

        public override object Clone()
        {
            return new SshHmacMd5();
        }
    }
}
