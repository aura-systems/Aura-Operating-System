/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using TwofishLib;

namespace SshDotNet.Algorithms
{
    public class SshTwofish192Cbc : EncryptionAlgorithm
    {
        public SshTwofish192Cbc()
            : base()
        {
            _algorithm = new Twofish();
            _algorithm.Mode = CipherMode.CBC;
            _algorithm.KeySize = 192;
        }

        public override string Name
        {
            get { return "twofish192-cbc"; }
        }

        public override object Clone()
        {
            return new SshTwofish192Cbc();
        }
    }
}
*/