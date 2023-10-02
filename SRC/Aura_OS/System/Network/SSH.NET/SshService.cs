using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public abstract class SshService : IDisposable
    {
        protected SshClient _client;      // Client for which service is running.

        private bool _isDisposed = false; // True if object has been disposed.

        public SshService(SshClient client)
        {
            _client = client;
        }

        ~SshService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    InternalStop();
                }

                // Dispose unmanaged resources.
            }

            _isDisposed = true;
        }
        
        public event EventHandler<EventArgs> Started;
        
        public event EventHandler<EventArgs> Stopped;

        public SshClient Client
        {
            get { return _client; }
        }

        public abstract string Name
        {
            get;
        }

        internal abstract bool ProcessMessage(byte[] payload);

        internal virtual void Start()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Raise event.
            if (Started != null) Started(this, new EventArgs());
        }

        internal void Stop()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            InternalStop();
        }

        protected virtual void InternalStop()
        {
            // Raise event.
            if (Stopped != null) Stopped(this, new EventArgs());
        }
    }
}
