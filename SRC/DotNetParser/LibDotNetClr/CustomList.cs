//#define STACK_DEBUG
using LibDotNetParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace libDotNetClr
{
    /// <summary>
    /// This class is used for debugging a List<>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomList<T>
    {
        public List<T> backend;
        public int Count { get { return backend.Count; } }

        public T this[int index]
        {
            get { return backend[index]; }
            set
            {
                backend[index] = value;
                if (value == null)
                {
                    Console.WriteLine("***Setting a value to null on stack***");
                }
            }
        }

        public CustomList()
        {
            backend = new List<T>();
        }
        public CustomList(int maxSize)
        {
            backend = new List<T>(maxSize);
        }
        public void Clear()
        {
#if STACK_DEBUG
            Console.WriteLine("Clearing the stack");
            //throw new Exception();
#endif
            backend.Clear();
        }
        public void Add(T a)
        {
            if (a == null)
                throw new Exception("Cannot add a null item");
            backend.Add(a);
        }

        public void RemoveAt(int m)
        {
            backend.RemoveAt(m);
        }
        public void RemoveRange(int index, int count)
        {
            backend.RemoveRange(index, count);
        }

        public T[] ToArray()
        {
            return backend.ToArray();
        }
        public List<T>.Enumerator GetEnumerator()
        {
            return backend.GetEnumerator();
        }
        public void MoveNext()
        {
            backend.GetEnumerator().MoveNext();

        }
        public T Current { get { return backend.GetEnumerator().Current; } }

        // since this data structure is being used as the type stack it is helpful to include some stack-like methods
        public T Pop()
        {
            if (Count == 0) throw new InvalidOperationException("Stack empty.");

            var item = backend[Count - 1];
            backend.RemoveAt(Count - 1);
            return item;
        }

        public T Peek()
        {
            if (Count == 0) throw new InvalidOperationException("Stack empty.");

            return backend[Count - 1];
        }
    }
}