namespace System.Collections.Generic
{
    public class Dictionary<TKey, TValue>
    {
        private class Entry
        {
            public int next = 0;        // Index of next entry, -1 if last
            public TKey key;           // Key of entry
            public TValue value;         // Value of entry
        }
        private Entry[] entries;
        private int count = 0;
        private IEqualityComparer<TKey> comparer;

        public TValue this[TKey key]
        {
            get
            {
                return _getVal(key);
            }
            set
            {
                _SetVal(key, value, true);
            }
        }

        private void _SetVal(TKey key, TValue value, bool overwrite)
        {
            if (overwrite)
            {
                var i = FindEntry(key);
                if (i != -1)
                {
                    entries[i].value = value;
                }
                else
                {
                    Console.WriteLine("Dictionary: Cannot find target entry");
                }
            }
            else
            {
                Console.WriteLine("Dictionary: Making new entry");
                var e = new Entry();
                e.value = value;
                e.key = key;
                
                entries[count] = e;
                count++;
            }
        }

        private TValue _getVal(TKey key)
        {
            var i = FindEntry(key);
            return entries[i].value;
        }

        public bool TryGetValue(TKey t, out TValue v)
        {
            var i = FindEntry(t);
            if (i != -1)
            {
                v = entries[i].value;
                return true;
            }
            else
            {
                v = default;
                return false;
            }
        }
        public void Add(TKey t, TValue v)
        {
            _SetVal(t, v, false);
        }

        private int FindEntry(TKey key)
        {
            for (int i = 0; i < count; i++)
            {
                var v = entries[i];
                if (key.Equals(v.key))
                {
                    return i;
                }
            }
            return -1;
        }
        public Dictionary() : this(0, null)
        {
            entries = new Entry[100];
        }
        public Dictionary(IEqualityComparer<TKey> comparer)
        {
            this.comparer = comparer;
            entries = new Entry[100];
        }
        public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            this.comparer = comparer;
        }
    }
}
