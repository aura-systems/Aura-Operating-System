using System.Collections;
using System.Collections.Generic;

namespace System
{
    public sealed class CharEnumerator : IEnumerator, IEnumerator<char>, IDisposable, ICloneable
    {
        private string? _str;
        private int _index;
        private char _currentElement;

        internal CharEnumerator(string str)
        {
            _str = str;
            _index = -1;
        }

        public object Clone()
        {
            return this;// MemberwiseClone();
        }

        public bool MoveNext()
        {
            if (_index < (_str!.Length - 1))
            {
                _index++;
                _currentElement = _str[_index];
                return true;
            }
            else
                _index = _str.Length;
            return false;
        }

        public void Dispose()
        {
            if (_str != null)
                _index = _str.Length;
            _str = null;
        }

        object? IEnumerator.Current => Current;

        public char Current
        {
            get
            {
                if (_index == -1)
                    throw new InvalidOperationException("InvalidOperation_EnumNotStarted");
                if (_index >= _str!.Length)
                    throw new InvalidOperationException("InvalidOperation_EnumEnded");
                return _currentElement;
            }
        }

        public void Reset()
        {
            _currentElement = (char)0;
            _index = -1;
        }
    }
}
