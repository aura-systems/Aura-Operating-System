namespace System
{
    public struct SByte
    {
        public override string ToString()
        {
            return Internal.NumberFormatUtils.SByteToString(this); //this will magicly return our value
        }
    }
}
