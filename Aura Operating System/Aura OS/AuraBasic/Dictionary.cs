using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.AuraBasic
{
    public class Dictionary<TKey, TValue> : IEnumerable
    {
        public List<TKey> Keys = new List<TKey>();
        public List<TValue> Values = new List<TValue>();

        public TValue this[TKey key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Values[Keys.IndexOf(key)] = value;
            }
        }

        public int Count
        {
            get
            {
                return Keys.Count;
            }
        }

        public bool ContainsKey(TKey k)
        {
            return Keys.Contains(k);
        }

        public TValue Get(TKey key)
        {
            int index = Keys.IndexOf(key);
            return Values[index];
        }

        public void Add(TKey key, TValue value)
        {
            Keys.Add(key);
            Values.Add(value);
        }

        public void Remove(TKey key)
        {
            int index = Keys.IndexOf(key);
            Keys.RemoveAt(index);
            Values.RemoveAt(index);
        }

        public void Clear()
        {
            Keys = new List<TKey>();
            Values = new List<TValue>();
        }

        /// <summary>
        /// Get Values
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator_V()
        {
            return ((IEnumerable)Values).GetEnumerator();
        }

        /// <summary>
        /// Default GetEnumerator (Keys)
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Keys).GetEnumerator();
        }
    }
}
