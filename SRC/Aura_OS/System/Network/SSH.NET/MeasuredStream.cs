using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public class MeasureableStream : Stream
    {
        protected Stream _stream;             // Base stream, which is to be measured.
        protected int _totalBytesRead;        // Total number of bytes read.
        protected int _totalBytesWritten;     // Total number of bytes written.
        protected int _totalBytesTransmitted; // Total number of bytes transmitted.

        public MeasureableStream(Stream stream)
            : base()
        {
            _stream = stream;

            ResetCounts();
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

        public int TotalBytesRead
        {
            get { return _totalBytesRead; }
        }

        public int TotalBytesWritten
        {
            get { return _totalBytesWritten; }
        }

        public int TotalBytesTransmitted
        {
            get { return _totalBytesTransmitted; }
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

        public void ResetCounts()
        {
            _totalBytesRead = 0;
            _totalBytesWritten = 0;
            _totalBytesTransmitted = 0;
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

            // Increase number of total bytes read/transmitted.
            _totalBytesRead += bytesRead;
            _totalBytesTransmitted += bytesRead;

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Write bytes to stream.
            _stream.Write(buffer, offset, count);

            // Increase number of total bytes written/transmitted.
            _totalBytesWritten += count;
            _totalBytesTransmitted += count;
        }
    }
}
