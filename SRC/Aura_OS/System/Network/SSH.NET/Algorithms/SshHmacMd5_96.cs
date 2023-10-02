using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshHmacMd5_96 : SshHmacMd5
    {
        public SshHmacMd5_96()
            : base()
        {
        }

        public override string Name
        {
            get { return "hmac-md5-96"; }
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
            return new SshHmacMd5_96();
        }
    }
}
