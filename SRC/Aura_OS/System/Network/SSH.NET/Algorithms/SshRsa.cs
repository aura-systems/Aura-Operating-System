using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet.Algorithms
{
    public class SshRsa : PublicKeyAlgorithm
    {
        protected new RSACryptoServiceProvider _algorithm; // Algorithm to use.

        public SshRsa()
            : base()
        {
            _algorithm = new RSACryptoServiceProvider();
            base._algorithm = _algorithm;
        }

        public new RSACryptoServiceProvider Algorithm
        {
            get { return _algorithm; }
        }

        public override string Name
        {
            get { return "ssh-rsa"; }
        }

        public override void LoadKeyAndCertificatesData(byte[] data)
        {
            using (var dataStream = new MemoryStream(data))
            using (var dataReader = new SshStreamReader(dataStream))
            {
                // Read parameters from stream.
                var algParams = new RSAParameters();

                if (dataReader.ReadString() != this.Name) throw new CryptographicException(
                   "Key and certificates were not created with this algorithm.");
                algParams.Exponent = dataReader.ReadMPInt();
                algParams.Modulus = dataReader.ReadMPInt();

                // Import parameters for algorithm key.
                _algorithm.ImportParameters(algParams);
            }
        }

        public override byte[] CreateKeyAndCertificatesData()
        {
            using (var dataStream = new MemoryStream())
            using (var dataWriter = new SshStreamWriter(dataStream))
            {
                // Export parameters for algorithm key.
                var algParams = _algorithm.ExportParameters(false);

                // Write parameters to stream.
                dataWriter.Write(this.Name);
                dataWriter.WriteMPint(algParams.Exponent);
                dataWriter.WriteMPint(algParams.Modulus);

                return dataStream.ToArray();
            }
        }

        public override bool VerifyData(byte[] data, byte[] signature)
        {
            return _algorithm.VerifyData(data, "SHA1", signature);
        }

        public override bool VerifyHash(byte[] hash, byte[] signature)
        {
            return _algorithm.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
        }

        public override byte[] SignData(byte[] data)
        {
            return _algorithm.SignData(data, "SHA1");
        }

        public override byte[] SignHash(byte[] hash)
        {
            return _algorithm.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
        }

        public override object Clone()
        {
            return new SshRsa();
        }
    }
}
