using System;
using System.Collections.Generic;

namespace System.Reflection
{
    public abstract class MethodBase : MemberInfo
    {
        public static bool op_Equality(MethodBase a, MethodBase b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }


            Console.WriteLine("Rest of op_Equality() not implemented!");
            return false;
        }
    }
}
