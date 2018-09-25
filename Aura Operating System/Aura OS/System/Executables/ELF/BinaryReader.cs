using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosELFCore
{
    public class BinaryReader
    {
        public Stream BaseStream;

        public BinaryReader(Stream stm)
        {
            BaseStream = stm;
        }

        public byte ReadByte()
        {
            return (byte) BaseStream.Read();
        }

        public string ReadString()
        {
            string ret = "";
            byte c;
            while ((c = ReadByte()) != 0)
            {
                ret += (char)c;
            }
            return ret;
        }
    }
}