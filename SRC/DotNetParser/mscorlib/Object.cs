using System.Runtime.CompilerServices;

namespace System
{
    public class Object
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Type GetObjType(System.Object a);
        public Object()
        {
            
        }
        ~Object()
        {

        }

        public virtual string ToString()
        {
            return "";
        }

        public virtual Type GetType()
        {
            return GetObjType(this);
        }
        public virtual bool Equals(Object obj)
        {
            Console.WriteLine("Object::Equals not implemented!");
            return false;
        }
    }
}
