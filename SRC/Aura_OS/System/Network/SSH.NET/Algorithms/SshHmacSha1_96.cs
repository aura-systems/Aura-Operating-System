using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshHmacSha1_96 : SshHmacSha1
    {
        public SshHmacSha1_96()
            : base()
        {
        }

        public override string Name
        {
            get { return "hmac-sha1-96"; }
        }

        public override int DigestLength
        {
            get { return 12; }
        }

        public override byte[] ComputeHash(byte[] input)
        {
            var hash = base.ComputeHash(input);
            Array.Resize(ref hash, this.DigestLength); // 12 bytes = 96 bits

            return hash;
        }

        public override object Clone()
        {
            return new SshHmacSha1_96();
        }
    }
}
