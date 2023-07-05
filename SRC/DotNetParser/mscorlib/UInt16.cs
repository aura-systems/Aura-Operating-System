namespace System
{
    public struct UInt16
    {
        public override string ToString()
        {
            return Internal.NumberFormatUtils.UInt16ToString(this); //this will magicly return our value
        }
    }
}
