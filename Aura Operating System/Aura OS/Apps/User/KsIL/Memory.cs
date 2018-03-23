using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL
{
    public class Memory
    {

        public static readonly int MEMORY_MODE = 0;
        public static readonly int PROGRAM_RUNNING = 1;
        public static readonly int CONDITIONAL_RESULT = 2;
        public static readonly int PROGRAM_COUNT = 4;
        public static readonly int RETURN_POINTER = 9;

        byte[] Buffer;
        int Size;

        public Memory(int Size)
        {
            this.Size = Size;
            Buffer = new byte[Size];
        }

        public int GetSize()
        {
            return Size;
        }

        public void Clear()
        {
            for (int i = 11; i < Size; i++)
                Buffer[i] = 0;
        }

        public byte Get(int Addr)
        {
            return Get(Addr, 1)[0];            
        }

        public byte[] Get(int Addr, int Length)
        {

            byte[] temp = new byte[Length];

            for (int i = 0; i < Length; i++)
            {

                temp[i] = Buffer[Addr + i];

            }

            return temp;
        }

        public byte[] GetDataPionter(int Addr)
        {


            if (Get(0) == 0x00)
            {

                byte point = Get(Addr);
                return GetData(point);

            }
            else if (Get(0) == 0x01)
            {

                Int16 point = BitConverter.ToInt16(Get(Addr, 2),0);
                return GetData(point);

            }
            else if (Get(0) == 0x02)
            {

                Int32 point = BitConverter.ToInt32(Get(Addr, 4), 0);
                return GetData(point);

            }
            else if (Get(0) == 0x03)
            {

                Int64 point = BitConverter.ToInt64(Get(Addr, 8), 0);
                return GetData((int) point);

            }

            return null;

        }

        public byte[] GetData(int Addr)
        {


            if (Get(0) == 0x00)
            {

                return Get(Addr + 1, Get(Addr));

            }
            else if (Get(0) == 0x01)
            {

                return Get(Addr + 2, BitConverter.ToInt16(Get(Addr, 2), 0));

            }
            else if (Get(0) == 0x02)
            {

                return Get(Addr + 4, BitConverter.ToInt32(Get(Addr, 4), 0));

            }
            else if (Get(0) == 0x03)
            {

                return Get(Addr + 8, (int) BitConverter.ToInt64(Get(Addr, 8), 0));

            }

            return null;

        }
        

        public void Set(int Addr, byte[] Value)
        {
            for (int i = 0; i < Value.Length; i++)
            {

                Set(Addr + i, Value[i]);

            }
        }

        public void Set(int Addr, byte Value)
        {
            Buffer[Addr] = Value;
        }

        public void SetData(int Addr, byte[] Value)
        {

            if (Get(0) == 0x00)
            {
                             
                Set(Addr, (byte) Value.Length);
                Set(Addr + 1, Value);

            }
            else if (Get(0) == 0x01)
            {
                
                Set(Addr, BitConverter.GetBytes((short) Value.Length));
                Set(Addr + 2, Value);

            }
            else if (Get(0) == 0x02)
            {
                                
                Set(Addr, BitConverter.GetBytes(Value.Length));
                Set(Addr + 4, Value);

            }
            else if (Get(0) == 0x03)
            {

                Set(Addr, BitConverter.GetBytes((long) Value.Length));
                Set(Addr + 8, Value);

            }

        }

        internal void Destroy()
        {
            Buffer = null;
            Size = 0;
        }
    }
}
