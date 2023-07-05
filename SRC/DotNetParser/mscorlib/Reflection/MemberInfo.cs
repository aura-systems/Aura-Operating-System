namespace System.Reflection
{
    public abstract class MemberInfo
    {
        public string _internalName;

        public string get_Name()
        {
            return _internalName;
        }
    }
}
