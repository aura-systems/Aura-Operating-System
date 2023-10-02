using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshAes128Cbc : EncryptionAlgorithm
    {
        public SshAes128Cbc()
            : base()
        {
            _algorithm = new AesCryptoServiceProvider();
            _algorithm.Mode = CipherMode.CBC;
            _algorithm.KeySize = 128;
        }

        public override string Name
        {
            get { return "aes128-cbc"; }
        }

        public override object Clone()
        {
            return new SshAes128Cbc();
        }
    }
}
