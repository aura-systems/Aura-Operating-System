using System.Runtime.CompilerServices;

namespace System
{
    public abstract class Array
    {
        internal const int MaxArrayLength = 0X7FEFFFFF;
        internal const int MaxByteArrayLength = 0x7FFFFFC7;
        public extern int Length
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
        /// <summary>
        /// How many dimensions are in the array
        /// </summary>
        public extern int Rank
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }


        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, false);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable);
        public static void ConstrainedCopy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, true);
        }
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int GetLowerBound(int dimension);
        public static void Copy(Array sourceArray, Array destinationArray, long length)
        {
            if (length > Int32.MaxValue || length < Int32.MinValue)
                throw new Exception("length > Int32.MaxValue || length < Int32.MinValue at Array::Copy");

            Array.Copy(sourceArray, destinationArray, (int)length);
        }


        public void CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
                throw new Exception("Arg_RankMultiDimNotSupported");
            // Note: Array.Copy throws a RankException and we want a consistent ArgumentException for all the IList CopyTo methods.
            Array.Copy(this, GetLowerBound(0), array, index, Length);
        }
        public void CopyTo(Array array, long index)
        {
            if (index > Int32.MaxValue || index < Int32.MinValue)
                throw new Exception("ArgumentOutOfRange_HugeArrayNotSupported");

            this.CopyTo(array, (int)index);
        }

        public static void Clear(Array array, int index, int length)
        {
            ArrayClear(array, index, length);
        }
        [MethodImplAttribute(MethodImplOptions.InternalCall)]

        private static extern void ArrayClear(Array array, int index, int length);

        // Returns the index of the first occurrence of a given value in an array.
        // The array is searched forwards, and the elements of the array are
        // compared to the given value using the Object.Equals method.
        // 
        public static int IndexOf(Array array, Object value)
        {
            if (array == null)
                throw new Exception("array is null in Array::IndexOf");
            int lb = array.GetLowerBound(0);
            return IndexOf(array, value, lb, array.Length);
        }

        // Returns the index of the first occurrence of a given value in a range of
        // an array. The array is searched forwards, starting at index
        // startIndex and ending at the last element of the array. The
        // elements of the array are compared to the given value using the
        // Object.Equals method.
        // 
        public static int IndexOf(Array array, Object value, int startIndex)
        {
            if (array == null)
                throw new Exception("array is null in Array::IndexOf");
            int lb = array.GetLowerBound(0);
            return IndexOf(array, value, startIndex, array.Length - startIndex + lb);
        }

        // Returns the index of the first occurrence of a given value in a range of
        // an array. The array is searched forwards, starting at index
        // startIndex and upto count elements. The
        // elements of the array are compared to the given value using the
        // Object.Equals method.
        // 
        public static int IndexOf(Array array, Object value, int startIndex, int count)
        {
            if (array == null)
                throw new Exception("array is null in Array::IndexOf");
            if (array.Rank != 1)
                throw new Exception("Rank_MultiDimNotSupported");

            int lb = array.GetLowerBound(0);
            if (startIndex < lb || startIndex > array.Length + lb)
                throw new Exception("start index is invaild");
            if (count < 0 || count > array.Length - startIndex + lb)
                throw new Exception("count is invaild");

            // Try calling a quick native method to handle primitive types.
            int retVal;
            bool r = TrySZIndexOf(array, startIndex, count, value, out retVal);
            if (r)
                return retVal;

            Object[] objArray = array as Object[];
            int endIndex = startIndex + count;
            if (objArray != null)
            {
                if (value == null)
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        if (objArray[i] == null) return i;
                    }
                }
                else
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        Object obj = objArray[i];
                        if (obj != null && obj.Equals(value)) return i;
                    }
                }
            }
            else
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    Object obj = array.GetValue(i);
                    if (obj == null)
                    {
                        if (value == null) return i;
                    }
                    else
                    {
                        if (obj.Equals(value)) return i;
                    }
                }
            }
            // Return one less than the lower bound of the array.  This way,
            // for arrays with a lower bound of -1 we will not return -1 when the
            // item was not found.  And for SZArrays (the vast majority), -1 still
            // works for them.
            return lb - 1;
        }
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool TrySZIndexOf(Array sourceArray, int sourceIndex, int count, Object value, out int retVal);

        public unsafe Object GetValue(int index)
        {
            if (Rank != 1)
                throw new Exception("Arg_Need1DArray");

            TypedReference elemref = new TypedReference();
            InternalGetReference(&elemref, 1, &index);
            return TypedReference.InternalToObject(&elemref);
        }
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        // reference to TypedReference is banned, so have to pass result as pointer
        private unsafe extern void InternalGetReference(void* elemRef, int rank, int* pIndices);
    }
    // Useful in number of places that return an empty byte array to avoid unnecessary memory allocation.
    internal static class EmptyArray<T>
    {
        public static readonly T[] Value = new T[0];
    }
}
