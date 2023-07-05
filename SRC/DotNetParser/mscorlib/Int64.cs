namespace System
{
    public struct Int64
    {
        public override string ToString()
        {
            return Internal.NumberFormatUtils.Int64ToString(this);
        }
    }
}
