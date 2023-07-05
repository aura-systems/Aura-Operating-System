using System.Runtime.CompilerServices;

namespace System.Reflection
{
    public class FieldInfo : MemberInfo
    {
        public void SetValue(object obj, object value)
        {
            FieldInfo_SetValue(this, obj, value);
        }
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void FieldInfo_SetValue(FieldInfo fieldInfo, object obj, object value);
    }
}
