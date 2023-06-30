namespace System.Reflection
{
    public abstract class MethodInfo : MethodBase
    {
        public string _internalName;

        public string get_Name()
        {
            return _internalName;
        }
    }
}
