using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Linq
{
    public static class Enumerable
    {
        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            ICollection<TSource> collection = source as ICollection<TSource>;
            if (collection != null) return collection.Contains(value);
            return Contains<TSource>(source, value, null);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            if (source == null) throw new Exception("Error.ArgumentNull(\"source\");");
            foreach (TSource element in source)
                if (comparer.Equals(element, value)) return true;
            return false;
        }
    }
}
