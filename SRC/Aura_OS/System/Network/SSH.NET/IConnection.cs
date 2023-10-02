using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public interface IConnection : IDisposable
    {
        bool IsConnected
        {
            get;
        }

        Stream GetStream();
        bool HandleException(SshClient client, Exception ex);
        void ConnectionEstablished();
        void Disconnect(bool remotely);
    }
}
