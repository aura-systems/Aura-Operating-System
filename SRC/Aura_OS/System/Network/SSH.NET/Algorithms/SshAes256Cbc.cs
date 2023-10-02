using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshAes256Cbc : EncryptionAlgorithm
    {
        public SshAes256Cbc()
            : base()
        {
            _algorithm = new AesCryptoServiceProvider();
            _algorithm.Mode = CipherMode.CBC;
            _algorithm.KeySize = 256;
        }

        public override string Name
        {
            get { return "aes256-cbc"; }
        }

        public override object Clone()
        {
            return new SshAes256Cbc();
        }
    }
}
