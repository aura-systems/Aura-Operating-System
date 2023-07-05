using System.Runtime.CompilerServices;

namespace System.Internal
{
    internal class NumberFormatUtils
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string ByteToString(byte a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string SByteToString(sbyte a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string UInt16ToString(ushort a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Int16ToString(short a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Int32ToString(int a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string UInt32ToString(uint a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string UInt64ToString(ulong a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Int64ToString(long a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string CharToString(char a);
    }
}
