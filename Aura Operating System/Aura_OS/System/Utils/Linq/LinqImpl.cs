/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Linq Implementation
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Utils.Linq
{
    public static class Linq
    {
        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Linq - All: source null");
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("Linq - All: predicate null");
            }

            foreach (TSource element in source)
            {
                if (!predicate(element))
                {
                    return false;
                }
            }

            return true;
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Linq - First: source null");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("Linq - predicate: source null");
            }
            foreach (TSource element in source)
            {
                if (predicate(element)) return element;
            }
            throw new Exception("Linq - First: no match");
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Linq - FirstOrDefault: source null");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current;
                    }
                }
            }
            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Linq - FirstOrDefault: source null");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("Linq - FirstOrDefault: predicate null");
            }
            foreach (TSource local in source)
            {
                if (predicate(local))
                {
                    return local;
                }
            }
            return default(TSource);
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Linq - Count: source null");
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("Linq - Count: predicate null");
            }

            int count = 0;
            foreach (TSource element in source)
            {
                checked
                {
                    if (predicate(element))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /*public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return null;
        }*/

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return WhereImpl(source, predicate);
        }

        private static IEnumerable<TSource> WhereImpl<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            int index = 0;
            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
                index++;
            }
        }

    }
}
