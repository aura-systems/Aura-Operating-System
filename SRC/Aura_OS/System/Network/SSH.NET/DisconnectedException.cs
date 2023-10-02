using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    // This exception is used to notify the data-receiving thread that the client has disconnected.
    public class DisconnectedException : Exception
    {
        public DisconnectedException()
            : base()
        {
        }
    }
}
