using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using System.IO.Compression;

namespace SshDotNet.Algorithms
{
    public class SshZlibCompression : CompressionAlgorithm
    {
        protected DeflateStream compressedStream;       // Compresses data.
        //protected Inflater _decompressor;     // Decompresses data.
        protected MemoryStream _outputStream; // Stream to which to output data.

        private bool _isDisposed = false;     // True if object has been disposed.

        public SshZlibCompression()
            : base()
        {
            //_compressor = new DeflateStream(
            //_compressor.DataAvailable += new DataAvailableHandler(_compressor_DataAvailable);

            //_decompressor = new Inflater();
            //_decompressor.DataAvailable += new DataAvailableHandler(_decompressor_DataAvailable);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        // Dispose managed resources.
                        //if (_compressor != null) _compressor.Dispose();
                        //if (_decompressor != null) _decompressor.Dispose();
                    }

                    // Dispose unmanaged resources.
                }

                _isDisposed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override string Name
        {
            get { return "zlib"; }
        }

        public override byte[] Compress(byte[] input)
        {
            //lock (_compressor)
            {
                using (_outputStream = new MemoryStream())
                {
                    // Write input data to compressor.
                    compressedStream = new DeflateStream(_outputStream, CompressionMode.Compress);
                    compressedStream.Write(input, 0, input.Length);

                    byte[] buffer = null;
                    _outputStream.Read(buffer, 0, (int)_outputStream.Length);
                    // Return compressed data.
                    return buffer;
                }
            }
        }

        public override byte[] Decompress(byte[] input)
        {
            
            int bytesread = 0;
            int totalread = 0;
            byte[] buffer = new byte[65536];
            MemoryStream outStream = new MemoryStream();

            using (DeflateStream uncompressStream = new DeflateStream(new MemoryStream(input), CompressionMode.Decompress))
            {
                do
                {
                    bytesread = uncompressStream.Read(buffer, 0, buffer.Length);
                    totalread += bytesread;
                    outStream.Write(buffer, 0, bytesread);
                } while (bytesread > 0);
                outStream.Flush();
                return outStream.ToArray();
            }
        }

        public override object Clone()
        {
            return new SshZlibCompression();
        }

        private void _compressor_DataAvailable(byte[] data, int startIndex, int count)
        {
            //lock (_compressor)
            {
                // Write compressed data to output stream.
                _outputStream.Write(data, startIndex, count);
            }
        }

        private void _decompressor_DataAvailable(byte[] data, int startIndex, int count)
        {
            //lock (_decompressor)
            {
                // Write decompressed data to output stream.
                _outputStream.Write(data, startIndex, count);
            }
        }
    }
}
