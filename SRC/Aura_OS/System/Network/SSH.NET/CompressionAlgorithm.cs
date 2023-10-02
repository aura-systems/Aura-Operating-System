using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet
{
    public abstract class CompressionAlgorithm : IDisposable, ICloneable
    {
        private bool _isDisposed = false; // True if object has been disposed.

        public CompressionAlgorithm()
        {
        }

        ~CompressionAlgorithm()
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
                }

                // Dispose unmanaged resources.
            }

            _isDisposed = true;
        }

        public abstract string Name
        {
            get;
        }

        public abstract byte[] Compress(byte[] input);

        public abstract byte[] Decompress(byte[] input);

        public abstract object Clone();
    }
}
