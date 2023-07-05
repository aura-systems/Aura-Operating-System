using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
    public abstract class Type
    {
        //TODO: make this abstract
        private string internal__fullname = "Error! value was never assigned.";
        private string internal__name = "Error! value was never assigned.";
        private string internal__namespace = "Error! value was never assigned.";
        public string get_FullName()
        {
            return internal__fullname; //Implemented in the CLR
        }
        public string get_Name()
        {
            return internal__name; //Implemented in the CLR
        }
        public string get_Namespace()
        {
            return internal__namespace; //Implemented in the CLR
        }
        public Assembly get_Assembly()
        {
            return GetAssemblyFromType(this);
        }
        public FieldInfo[] GetFields()
        {
            return InternalGetFields(this);
        }
        public FieldInfo GetField(string s)
        {
            return InternalGetField(this, s);
        }
        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            return Type_FromReference(handle);
        }

        public static Type? GetType(
            string typeName,
            Func<AssemblyName, Assembly?>? assemblyResolver,
            Func<Assembly?, string, bool, Type?>? typeResolver,
            bool throwOnError)
        {
            //StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            //return TypeNameParser.GetType(typeName, assemblyResolver, typeResolver, throwOnError, false, ref stackMark);
            Console.WriteLine("type.gettype notimplemented. type is " + typeName);
            return null;
        }



        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static Type Type_FromReference(RuntimeTypeHandle handle);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static Assembly GetAssemblyFromType(Type t);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static FieldInfo[] InternalGetFields(Type t);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern FieldInfo InternalGetField(Type t, string name);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern MethodInfo GetMethod(string name);

        public static bool op_Equality(System.Type a, System.Type b)
        {
            if(a==null && b != null)
            {
                return false;
            }
            if (a != null && b == null)
            {
                return false;
            }
            if (a == null && b == null)
            {
                return true;
            }
            return a.internal__fullname == b.internal__fullname;
        }
    }
}