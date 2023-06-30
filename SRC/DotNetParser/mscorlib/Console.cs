using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Console class
    /// </summary>
    public static class Console
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string ReadLine();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(string str);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(int num);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(long num);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(string str);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(int num);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(long num);
        
        public static void Clear()
        {
            ConsoleClear();
        }
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void ConsoleClear();
    }
}
