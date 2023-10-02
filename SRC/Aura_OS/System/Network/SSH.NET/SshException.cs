using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public class SshException : Exception
    {
        public SshException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SshException(string message)
            : base(message)
        {
        }

        public SshException()
            : base()
        {
        }
    }
}
