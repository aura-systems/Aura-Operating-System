using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public class SshStreamWriter : IDisposable
    {
        protected Stream _stream;

        public SshStreamWriter(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stream stream = _stream;
                _stream = null;
                if (stream != null) stream.Close();
            }

            _stream = null;
        }

        public Stream BaseStream
        {
            get { return _stream; }
        }

        public void WriteNameList(string[] nameList)
        {
            Write(string.Join(",", nameList));
        }

        public void WriteByteString(byte[] value)
        {
            Write((uint)value.Length);
            Write(value);
        }

        //public void WriteMPint(int value)
        //{
        //    // Get array of bytes for specified integer value.
        //    byte[] rawNum = BitConverter.GetBytes(value);
        //    uint leadingZerosCount = 0;

        //    while (rawNum[leadingZerosCount] == 0) leadingZerosCount++;

        //    // Strip leading zeros from byte array.
        //    byte[] num = new byte[rawNum.Length - leadingZerosCount];

        //    Array.Copy(rawNum, leadingZerosCount, num, 0, num.Length);

        //    WriteMPint(num);
        //}

        // Important: method assumes value is always positive integer, and therefore adds leading zero is
        // necessary.
        public void WriteMPint(byte[] value)
        {
            uint strLength = (uint)value.Length;
            
            // Write leading zero if MSB of value is high.
            bool addLeadingZero = ((value[0] & 0x80) != 0);
            if (addLeadingZero) strLength++;

            Write(strLength);
            if (addLeadingZero) Write((byte)0);

            // Write value of integer.
            Write(value);
        }

        public void Write(string value)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(value);

            Write((uint)buffer.Length);
            Write(buffer);
        }

        public void Write(char value)
        {
            _stream.WriteByte(Encoding.ASCII.GetBytes(new char[] { value })[0]);
        }

        public void Write(short value)
        {
            Write(unchecked((ushort)value));
        }

        public void Write(int value)
        {
            Write(unchecked((uint)value));
        }

        public void Write(long value)
        {
            Write(unchecked((ulong)value));
        }

        public void Write(ushort value)
        {
            byte[] buffer = new byte[2];

            buffer[0] = (byte)((value & 0xFF00) >> 8);
            buffer[1] = (byte)(value & 0x00FF);

            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Write(uint value)
        {
            byte[] buffer = new byte[4];

            buffer[0] = (byte)((value & 0xFF000000) >> 24);
            buffer[1] = (byte)((value & 0x00FF0000) >> 16);
            buffer[2] = (byte)((value & 0x0000FF00) >> 8);
            buffer[3] = (byte)(value & 0x000000FF);

            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Write(ulong value)
        {
            byte[] buffer = new byte[8];

            buffer[0] = (byte)((value & 0xFF00000000000000) >> 56);
            buffer[1] = (byte)((value & 0x00FF000000000000) >> 48);
            buffer[2] = (byte)((value & 0x0000FF0000000000) >> 40);
            buffer[3] = (byte)((value & 0x000000FF00000000) >> 32);
            buffer[4] = (byte)((value & 0x00000000FF000000) >> 24);
            buffer[5] = (byte)((value & 0x0000000000FF0000) >> 16);
            buffer[6] = (byte)((value & 0x000000000000FF00) >> 8);
            buffer[7] = (byte)(value & 0x00000000000000FF);

            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Write(bool value)
        {
            _stream.WriteByte(value ? (byte)1 : (byte)0);
        }

        public void Write(byte value)
        {
            _stream.WriteByte(value);
        }

        public void Write(byte[] buffer)
        {
            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public virtual void Close()
        {
            this.Dispose(true);
        }
    }
}
