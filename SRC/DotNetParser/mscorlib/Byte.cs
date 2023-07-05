namespace System
{
    public struct Byte
    {
        public override string ToString()
        {
            return Internal.NumberFormatUtils.ByteToString(this);
        }
    }
}
