using System.Runtime.CompilerServices;

namespace System.IO
{
    public static class File
    {
        //public static bool Exists(string path)
        //{
        //    return File__Exists(path);
        //}

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool File__Exists(string path);
    }
}
