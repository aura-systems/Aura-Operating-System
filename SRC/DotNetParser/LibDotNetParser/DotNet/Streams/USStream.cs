using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LibDotNetParser.DotNet.Streams
{
    /// <summary>
    /// #USer Stream
    /// </summary>
    public class USStream
    {
        private readonly Dictionary<uint, string> _strings;

        public USStream(Dictionary<uint, string> strings)
        {
            _strings = strings;
        }

        public string GetByOffset(uint offset)
        {
            if (!_strings.ContainsKey(offset))
                throw new Exception("User string not found: " + offset);
            return _strings[offset];
        }

        public IEnumerable<string> GetAll()
        {
            return _strings.Values;
        }
    }

    public class USStreamReader
    {
        private readonly BinaryReader _reader;
        private readonly int _dataSize;

        public USStreamReader(byte[] data)
        {
            _dataSize = data.Length;
            _reader = new BinaryReader(new MemoryStream(data));
        }

        public USStream Read()
        {
            var strings = new Dictionary<uint, string>();
            uint CurrentString = 1;
            _reader.BaseStream.Position = 1;

            //The US Stream starts with a null byte, so skip it
            bool eightByteStart = false;
            for (int i = 1; i < _dataSize; i++)
            {
                var o = _reader.BaseStream.Position;
                var nb = Read7BitInt(_reader);
                _reader.BaseStream.Position = o;
                var len = _reader.ReadByte();
                if (len == 0x80)
                {
                    eightByteStart = true;
                    continue;
                }

                //Ignore zero sized strings
                if (len == 0)
                    continue;

                var ActualStringLen = len % 2 == 0 ? len - 2 : len - 1;

                List<byte> bytes = new List<byte>();
                bool end = false;
                for (int i2 = 0; i2 < ActualStringLen / 2; i2++)
                {
                    try
                    {
                        byte a = _reader.ReadByte();
                        byte b = _reader.ReadByte();
                        if (a + b == 0)
                        {
                            end = true;
                            break;
                        }
                        bytes.Add(a);
                        bytes.Add(b);
                    }
                    catch (EndOfStreamException)
                    {
                        throw new Exception("Error while reading US stream table: End of stream. Please open an issue on github.");
                    }
                }

                var nullBytes = len - ActualStringLen;
                if (!end)
                {
                    for (int i3 = 0; i3 < nullBytes; i3++)
                    {
                        var b = _reader.ReadByte();
                        if (b != 0)
                        {
                            //throw new Exception("Expected null byte.");
                        }
                    }
                }
                i = (int)_reader.BaseStream.Position;

                var x = i - len - 1;
                if (eightByteStart)
                {
                    x--;
                    eightByteStart = false;
                }

                string s = Encoding.Unicode.GetString(bytes.ToArray());
                strings.Add((uint)x, s);
                CurrentString++;
            }
            return new USStream(strings);
        }

        private int Read7BitInt(BinaryReader r)
        {
            ReadInt32Bit7(r, out int result);
            return result;
        }

        public void ReadInt32Bit7(BinaryReader r, out Int32 result)
        {
            // Endianness does not matter, as this value is stored byte by byte.
            // While the highest bit is set, the integer requires another of a maximum of 5 bytes.
            result = 0;
            for (int i = 0; i < sizeof(Int32) + 1; i++)
            {
                byte readByte = r.ReadByte();
                result |= (readByte & 0b01111111) << i * 7;
                if ((readByte & 0b10000000) == 0)
                    return;
            }
            throw new ArgumentException("Invalid 7-bit encoded Int32.");
        }

    }
}
