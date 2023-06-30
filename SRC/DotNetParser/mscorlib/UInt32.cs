namespace System
{
    public struct UInt32
    {
        public const uint MaxValue = (uint)0xffffffff;
        public const uint MinValue = 0U;
        public override string ToString()
        {
            return Internal.NumberFormatUtils.UInt32ToString(this);
        }
    }
}
