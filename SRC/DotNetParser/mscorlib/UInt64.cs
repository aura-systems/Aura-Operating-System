namespace System
{
    public struct UInt64
    {
        public override string ToString()
        {
            return Internal.NumberFormatUtils.UInt64ToString(this);
        }
    }
}
