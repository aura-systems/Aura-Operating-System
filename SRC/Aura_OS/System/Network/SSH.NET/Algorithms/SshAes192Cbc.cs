using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshAes192Cbc : EncryptionAlgorithm
    {
        public SshAes192Cbc()
            : base()
        {
            _algorithm = new AesCryptoServiceProvider();
            _algorithm.Mode = CipherMode.CBC;
            _algorithm.KeySize = 192;
        }

        public override string Name
        {
            get { return "aes192-cbc"; }
        }

        public override object Clone()
        {
            return new SshAes192Cbc();
        }
    }
}
