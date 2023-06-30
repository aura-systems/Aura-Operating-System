namespace System.Collections.Generic
{
    public interface IEnumerable<out T> : IEnumerable
    {
        // Returns an IEnumerator for this enumerable Object.  The enumerator provides
        // a simple way to access all the contents of a collection.
        new IEnumerator<T> GetEnumerator();
    }
}
