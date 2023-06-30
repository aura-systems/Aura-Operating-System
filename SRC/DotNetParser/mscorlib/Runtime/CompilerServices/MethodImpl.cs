namespace System.Runtime.CompilerServices
{
    public enum MethodImplOptions
    {
        Unmanaged = System.Reflection.MethodImplAttributes.Unmanaged,
        ForwardRef = System.Reflection.MethodImplAttributes.ForwardRef,
        PreserveSig = System.Reflection.MethodImplAttributes.PreserveSig,
        InternalCall = System.Reflection.MethodImplAttributes.InternalCall,
        Synchronized = System.Reflection.MethodImplAttributes.Synchronized,
        NoInlining = System.Reflection.MethodImplAttributes.NoInlining,
        AggressiveInlining = System.Reflection.MethodImplAttributes.AggressiveInlining,
        NoOptimization = System.Reflection.MethodImplAttributes.NoOptimization,
        SecurityMitigations = System.Reflection.MethodImplAttributes.SecurityMitigations,
    }
    public enum MethodCodeType
    {
        IL = System.Reflection.MethodImplAttributes.IL,
        Native = System.Reflection.MethodImplAttributes.Native,
        OPTIL = System.Reflection.MethodImplAttributes.OPTIL,
        Runtime = System.Reflection.MethodImplAttributes.Runtime
    }
    public sealed class MethodImplAttribute : Attribute
    {
        internal MethodImplOptions _val;
        public MethodCodeType MethodCodeType;

        internal MethodImplAttribute(System.Reflection.MethodImplAttributes methodImplAttributes)
        {
            MethodImplOptions all =
                MethodImplOptions.Unmanaged | MethodImplOptions.ForwardRef | MethodImplOptions.PreserveSig |
                MethodImplOptions.InternalCall | MethodImplOptions.Synchronized |
                MethodImplOptions.NoInlining | MethodImplOptions.AggressiveInlining |
                MethodImplOptions.NoOptimization | MethodImplOptions.SecurityMitigations;
            _val = ((MethodImplOptions)methodImplAttributes) & all;
        }

        public MethodImplAttribute(MethodImplOptions methodImplOptions)
        {
            _val = methodImplOptions;
        }

        public MethodImplAttribute(short value)
        {
            _val = (MethodImplOptions)value;
        }

        public MethodImplAttribute()
        {
        }

        public MethodImplOptions Value { get { return _val; } }
    }

}