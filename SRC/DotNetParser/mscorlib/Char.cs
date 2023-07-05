namespace System
{
    public struct Char
    {
        public override string ToString()
        {
            return Internal.NumberFormatUtils.CharToString(this);
        }
    }
}
