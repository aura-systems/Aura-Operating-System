using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshTripleDesCbc : EncryptionAlgorithm
    {
        public SshTripleDesCbc()
            : base()
        {
            _algorithm = new TripleDESCryptoServiceProvider();
            _algorithm.Mode = CipherMode.CBC;
        }

        public override string Name
        {
            get { return "3des-cbc"; }
        }

        public override object Clone()
        {
            return new SshTripleDesCbc();
        }
    }
}
