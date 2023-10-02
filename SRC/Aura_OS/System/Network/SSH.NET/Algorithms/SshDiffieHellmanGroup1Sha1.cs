/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Org.Mentalis.Security.Cryptography;

namespace SshDotNet.Algorithms
{
    public class SshDiffieHellmanGroup1Sha1 : KexAlgorithm
    {
        protected DiffieHellman _exchangeAlgorithm; // Exchange algorithm to use.

        public SshDiffieHellmanGroup1Sha1()
            : base()
        {
            _exchangeAlgorithm = new DiffieHellmanManaged(1024, 0, DHKeyGeneration.Static);
            _hashAlgorithm = new SHA1CryptoServiceProvider();
        }

        public override string Name
        {
            get { return "diffie-hellman-group1-sha1"; }
        }

        public override AsymmetricAlgorithm ExchangeAlgorithm
        {
            get { return _exchangeAlgorithm; }
        }

        public override byte[] CreateKeyExchange()
        {
            return _exchangeAlgorithm.CreateKeyExchange();
        }

        public override byte[] DecryptKeyExchange(byte[] exchangeData)
        {
            return _exchangeAlgorithm.DecryptKeyExchange(exchangeData);
        }

        public override object Clone()
        {
            return new SshDiffieHellmanGroup1Sha1();
        }
    }
}
*/