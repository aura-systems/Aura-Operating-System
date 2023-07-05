namespace System
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageAttribute : Attribute
    {
        internal AttributeTargets m_attributeTarget = AttributeTargets.All; // Defaults to all
        internal bool m_allowMultiple = false; // Defaults to false
        internal bool m_inherited = true; // Defaults to true

        internal static AttributeUsageAttribute Default = new AttributeUsageAttribute(AttributeTargets.All);

        //Constructors 
        public AttributeUsageAttribute(AttributeTargets validOn)
        {
            m_attributeTarget = validOn;
        }
        internal AttributeUsageAttribute(AttributeTargets validOn, bool allowMultiple, bool inherited)
        {
            m_attributeTarget = validOn;
            m_allowMultiple = allowMultiple;
            m_inherited = inherited;
        }


        //Properties 
        public AttributeTargets ValidOn
        {
            get { return m_attributeTarget; }
        }

        public bool AllowMultiple
        {
            get { return m_allowMultiple; }
            set { m_allowMultiple = value; }
        }

        public bool Inherited
        {
            get { return m_inherited; }
            set { m_inherited = value; }
        }
    }
    [Flags]
    public enum AttributeTargets
    {
        Assembly = 0x0001,
        Module = 0x0002,
        Class = 0x0004,
        Struct = 0x0008,
        Enum = 0x0010,
        Constructor = 0x0020,
        Method = 0x0040,
        Property = 0x0080,
        Field = 0x0100,
        Event = 0x0200,
        Interface = 0x0400,
        Parameter = 0x0800,
        Delegate = 0x1000,
        ReturnValue = 0x2000,
        //@todo GENERICS: document GenericParameter
        GenericParameter = 0x4000,


        All = Assembly | Module | Class | Struct | Enum | Constructor |
                        Method | Property | Field | Event | Interface | Parameter |
                        Delegate | ReturnValue | GenericParameter,
    }
}
