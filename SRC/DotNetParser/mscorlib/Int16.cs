namespace System
{
    public struct Int16
    {
        public override string ToString()
        {
            return Internal.NumberFormatUtils.Int16ToString(this); //this will magicly return our value
        }
    }
}
