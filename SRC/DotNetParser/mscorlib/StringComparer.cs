namespace System
{
    public abstract class StringComparer
    {
        public static StringComparer OrdinalIgnoreCase
        {
            get
            {
                return OrdinalIgnoreCaseComparer.Instance;
            }
        }
    }
    internal sealed class OrdinalIgnoreCaseComparer : StringComparer
    {
        internal static readonly OrdinalIgnoreCaseComparer Instance = new OrdinalIgnoreCaseComparer();
        public int Compare(string x, string y)
        {
            Console.WriteLine("TODO: int Compare(string, string) in OrdinalIgnoreCaseComparer");
            return 0;
        }

        public bool Equals(string x, string y)
        {
            return String.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }
    }
}
