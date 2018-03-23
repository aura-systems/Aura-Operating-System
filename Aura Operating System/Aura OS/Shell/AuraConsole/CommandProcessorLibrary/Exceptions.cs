using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework.Exceptions
{
    public class SyntaxException : Exception
    {
        public SyntaxException(string Message) : base(Message)
        {}
    }

    public class CommandArgumentOutofRangeException : Exception
    {
        public CommandArgumentOutofRangeException(string Message) : base(Message)
        { }
    }
}
