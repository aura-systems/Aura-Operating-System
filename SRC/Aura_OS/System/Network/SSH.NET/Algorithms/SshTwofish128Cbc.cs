/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using TwofishLib;

namespace SshDotNet.Algorithms
{
    public class SshTwofish128Cbc : EncryptionAlgorithm
    {
        public SshTwofish128Cbc()
            : base()
        {
            _algorithm = new Twofish();
            _algorithm.Mode = CipherMode.CBC;
            _algorithm.KeySize = 128;
        }

        public override string Name
        {
            get { return "twofish128-cbc"; }
        }

        public override object Clone()
        {
            return new SshTwofish128Cbc();
        }
    }
}

*/