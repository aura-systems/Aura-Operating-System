/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Org.Mentalis.Security.Cryptography;

namespace SshDotNet.Algorithms
{
    public class SshDiffieHellmanGroup14Sha1 : KexAlgorithm
    {
        protected DiffieHellman _exchangeAlgorithm; // Exchange algorithm to use.

        public SshDiffieHellmanGroup14Sha1()
            : base()
        {
            _exchangeAlgorithm = new DiffieHellmanManaged(2048, 0, DHKeyGeneration.Static);
            _hashAlgorithm = new SHA1CryptoServiceProvider();
        }

        public override string Name
        {
            get { return "diffie-hellman-group14-sha1"; }
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
            return new SshDiffieHellmanGroup14Sha1();
        }
    }
}
*/