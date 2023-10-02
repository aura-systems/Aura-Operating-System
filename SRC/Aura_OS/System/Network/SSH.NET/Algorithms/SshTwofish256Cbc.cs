/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using TwofishLib;

namespace SshDotNet.Algorithms
{
    public class SshTwofish256Cbc : EncryptionAlgorithm
    {
        public SshTwofish256Cbc()
            : base()
        {
            _algorithm = new Twofish();
            _algorithm.Mode = CipherMode.CBC;
            _algorithm.KeySize = 256;
        }

        public override string Name
        {
            get { return "twofish-cbc"; }
        }

        public override object Clone()
        {
            return new SshTwofish256Cbc();
        }
    }
}
*/