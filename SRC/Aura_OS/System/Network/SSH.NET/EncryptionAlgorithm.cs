using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet
{
    public abstract class EncryptionAlgorithm : IDisposable, ICloneable
    {
        protected SymmetricAlgorithm _algorithm; // Algorithm to use.

        private bool _isDisposed = false;        // True if object has been disposed.

        public EncryptionAlgorithm()
        {
        }

        ~EncryptionAlgorithm()
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
                    if (_algorithm != null) _algorithm.Clear();
                }

                // Dispose unmanaged resources.
            }

            _isDisposed = true;
        }

        public abstract string Name
        {
            get;
        }

        public SymmetricAlgorithm Algorithm
        {
            get { return _algorithm; }
        }

        public virtual byte[] Encrypt(byte[] input)
        {
            return _algorithm.Encrypt(input);
        }

        public virtual byte[] Decrypt(byte[] input)
        {
            return _algorithm.Decrypt(input);
        }

        public abstract object Clone();
    }
}
