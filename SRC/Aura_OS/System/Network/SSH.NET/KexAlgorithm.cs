using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet
{
    public abstract class KexAlgorithm : IDisposable, ICloneable
    {
        protected HashAlgorithm _hashAlgorithm; // Algorithm to use for hashing.

        private bool _isDisposed = false;       // True if object has been disposed.

        public KexAlgorithm()
        {
        }

        ~KexAlgorithm()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_hashAlgorithm != null) _hashAlgorithm.Clear();
                }

                // Dispose unmanaged resources.
            }

            _isDisposed = true;
        }

        public abstract string Name
        {
            get;
        }

        public abstract AsymmetricAlgorithm ExchangeAlgorithm
        {
            get;
        }

        public HashAlgorithm HashAlgorithm
        {
            get { return _hashAlgorithm; }
        }

        public abstract byte[] CreateKeyExchange();

        public abstract byte[] DecryptKeyExchange(byte[] exchangeData);

        public byte[] ComputeHash(byte[] input)
        {
            return _hashAlgorithm.ComputeHash(input);
        }

        public abstract object Clone();
    }
}
