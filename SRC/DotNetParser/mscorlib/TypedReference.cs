using System.Runtime.CompilerServices;

namespace System
{
    public struct TypedReference
    {
        public unsafe static Object ToObject(TypedReference value)
        {
            return InternalToObject(&value);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal unsafe extern static Object InternalToObject(void* value);
    }
}
