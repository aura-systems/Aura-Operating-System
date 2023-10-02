using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SshDotNet;

namespace Aura_OS.System.Interpreter.Commands.Network.SSH
{
    public class SshConsoleClient : SshClient
    {
        private bool _isDisposed = false; // True if object has been disposed.

        public SshConsoleClient(byte[] data)
            : base(data)
        {
        }

        public SshConsoleClient(Stream stream)
            : base(stream)
        {
        }

        public SshConsoleClient(IConnection connection, bool addDefaultAlgorithms)
            : base(connection, addDefaultAlgorithms)
        {
        }

        public SshConsoleClient(Stream stream, bool addDefaultAlgorithms)
            : base(stream, addDefaultAlgorithms)
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        // Dispose managed resources.
                    }

                    // Dispose unmanaged resources.
                }

                _isDisposed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
