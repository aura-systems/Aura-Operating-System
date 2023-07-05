using System.Runtime.CompilerServices;

namespace System
{
    public struct Boolean
    {
        public override string ToString()
        {
            if (Boolean_GetValue(this))
            {
                return "True";
            }
            else
            {
                return "False";
            }
        }
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool Boolean_GetValue(Boolean a);
    }
}
