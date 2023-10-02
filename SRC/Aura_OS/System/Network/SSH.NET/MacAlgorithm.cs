using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet
{
    public abstract class MacAlgorithm : IDisposable, ICloneable
    {
        protected KeyedHashAlgorithm _algorithm; // Algorithm to use.

        private bool _isDisposed = false;        // True if object has been disposed.

        public MacAlgorithm()
        {
        }

        ~MacAlgorithm()
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

        public abstract int DigestLength
        {
            get;
        }

        public KeyedHashAlgorithm Algorithm
        {
            get { return _algorithm; }
        }

        public virtual byte[] ComputeHash(byte[] input)
        {
            return _algorithm.ComputeHash(input);
        }

        public abstract object Clone();
    }
}
