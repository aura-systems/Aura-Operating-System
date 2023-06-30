using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    public sealed class String : IEnumerable, IEnumerable<char>
    { 
        /// <summary>Maximum length allowed for a string.</summary>
      /// <remarks>Keep in sync with AllocateString in gchelpers.cpp.</remarks>
        internal const int MaxLength = 0x3FFFFFDF;

        // The Empty constant holds the empty string value. It is initialized by the EE during startup.
        // It is treated as intrinsic by the JIT as so the static constructor would never run.
        // Leaving it uninitialized would confuse debuggers.
#pragma warning disable CS8618 // compiler sees this non-nullable static string as uninitialized
        [Intrinsic]
        public static readonly string Empty;
#pragma warning restore CS8618

        public int Length { get { return strLen(this); } }
       // public extern string this[int index] { get; set; }

        public char this[int index]
        {
            [Intrinsic]
            get
            {
                if ((uint)index >= (uint)Length)
                    throw new Exception("index out of range");
                return get_Chars(index);
            }
        }
        public object Clone()
        {
            return this;
        }
        // Returns this string.
        public override string ToString()
        {
            return this;
        }


        public CharEnumerator GetEnumerator()
        {
            return new CharEnumerator(this);
        }

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            return new CharEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CharEnumerator(this);
        }

        public char get_Chars(int index)
        {
            return String_get_Chars_1(this, index);
        }
        public static bool IsNullOrEmpty(string s)
        {
            if (s == null)
                return true;

            if (s == "")
                return true;

            return false;
        }
        public string ToUpper()
        {
            return String_ToUpper(this);
        }
        public string ToLower()
        {
            return String_ToLower(this);
        }
        public static bool Equals(string a, string b, StringComparison comparisonType)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (comparisonType == StringComparison.CurrentCulture | comparisonType == StringComparison.CurrentCultureIgnoreCase)
            {
                Console.WriteLine("String.cs:Equals() CurrentCulture not implemented!");
                return false;
            }
            if (comparisonType == StringComparison.InvariantCulture | comparisonType == StringComparison.InvariantCultureIgnoreCase)
            {
                Console.WriteLine("String.cs:Equals() CurrentCulture not implemented!");
                return false;
            }
            if (comparisonType == StringComparison.Ordinal)
                return EqualsHelper(a, b);

            if (comparisonType == StringComparison.OrdinalIgnoreCase)
                return String_EqualsOrdinalIgnoreCaseNoLengthCheck(a, b);
            Console.WriteLine("string.cs: end of flow");
            return false;
        }
        public int IndexOf(char c)
        {
            return String_IndexOf(this, c);
        }
        public bool EndsWith(string s)
        {
            return String_EndsWith(this, s);
        }
        public string Substring(int startIndex, int length)
        {
            return String_SubString(this, startIndex, length);
        }

        private static bool EqualsHelper(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) { return false; }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern string[] Split(params char[] separator);

        public string[] Split(char separator, StringSplitOptions options = StringSplitOptions.None)
        {
            return Split(new char[] { separator });
        }
            //Internal calls

            [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Concat(string a, string b);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Concat(string a, string b, string c);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Concat(string a, string b, string c, string d);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static bool op_Equality(string a, string b);
        public static bool op_Inequality(string a, string b)
        {
            return !op_Equality(a, b);
        }


        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static int String_IndexOf(System.String s, char c);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static int strLen(System.String a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static char String_get_Chars_1(System.String a, int i);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string String_ToUpper(System.String a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string String_ToLower(System.String a);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static bool String_EndsWith(string instance, string b);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string String_SubString(string instance, int startIndex, int length);
        

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static bool String_EqualsOrdinalIgnoreCaseNoLengthCheck(string a, string b);
    }
    public enum StringComparison
    {
        CurrentCulture,
        CurrentCultureIgnoreCase,
        InvariantCulture,
        InvariantCultureIgnoreCase,
        Ordinal,
        OrdinalIgnoreCase
    }
    [Flags]
    public enum StringSplitOptions
    {
        None = 0,
        RemoveEmptyEntries = 1
    }
}
