using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public class CachedStream : Stream
    {
        protected byte[][] _buffers; // Array of most recent buffers into which stream read.

        protected Stream _stream;    // Base stream, which is to be limited.

        public CachedStream(Stream stream)
            : base()
        {
            _stream = stream;

            _buffers = new byte[2][];
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (_stream != null))
                {
                    try
                    {
                        _stream.Flush();
                    }
                    finally
                    {
                        _stream.Close();
                    }
                }
            }
            finally
            {
                _stream = null;
                base.Dispose(disposing);
            }
        }

        public Stream BaseStream
        {
            get { return _stream; }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
            }
        }

        public byte[] GetBufferEnd(int index, int count)
        {
            return GetBuffer(index, _buffers[index].Length - count, count);
        }

        public byte[] GetBuffer(int index)
        {
            return GetBuffer(index, 0, _buffers[index].Length);
        }

        public byte[] GetBuffer(int index, int offset, int count)
        {
            byte[] returnBuffer = new byte[count];

            Buffer.BlockCopy(_buffers[index], offset, returnBuffer, 0, count);
            return returnBuffer;
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Read bytes from stream.
            int bytesRead = _stream.Read(buffer, offset, count);

            _buffers[0] = _buffers[1];
            _buffers[1] = (byte[])buffer.Clone();

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
    }
}
