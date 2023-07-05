namespace System.Collections
{
    public interface IEqualityComparer
    {
        bool Equals(Object x, Object y);
        int GetHashCode(Object obj);
    }
    public interface IEqualityComparer<in T>
    {
        bool Equals(T x, T y);
        int GetHashCode(T obj);
    }
}
