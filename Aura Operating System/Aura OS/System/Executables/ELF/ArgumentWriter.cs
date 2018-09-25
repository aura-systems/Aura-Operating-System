using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosELFCore
{
    public unsafe class ArgumentWriter
    {
        private BinaryWriter _writer;

        public ArgumentWriter()
        {
            //clear call stack
            for (int k = 0; k < 1024; k++)
            {
                ((byte*) Invoker.Stack)[k] = 0;
            }

            _writer = new BinaryWriter(new MemoryStream((byte*) Invoker.Stack));
            _writer.BaseStream.Posistion = 50;
        }


        public ArgumentWriter Push(char c)
        {
            _writer.Write(c);
            return this;
        }

        public ArgumentWriter Push(byte c)
        {
             _writer.Write(c);
            return this;
        }

        public ArgumentWriter Push(short c)
        {
            _writer.Write(c);
            return this;
        }

        public ArgumentWriter Push(int c)
        {
            _writer.Write(c);
            return this;
        } 

        public ArgumentWriter Push(uint c)
        {
            _writer.Write(c);
            return this;
        }
        
    }
}