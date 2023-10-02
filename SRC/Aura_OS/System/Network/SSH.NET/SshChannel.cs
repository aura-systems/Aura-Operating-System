using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public abstract class SshChannel : IDisposable, INotifyPropertyChanged
    {
        protected bool _open;                        // True if channel is currently open.
        protected bool _eofSent;                     // True if EOF (end of file) message has been sent.
        protected bool _eofReceived;                 // True if EOF (end of file) message has been received.

        protected SshConnectionService _connService; // Connection service.

        private bool _isDisposed = false;            // True if object has been disposed.

        public SshChannel(ChannelOpenRequestEventArgs requestEventArgs)
            : this(requestEventArgs.ClientChannel, requestEventArgs.ServerChannel,
            requestEventArgs.InitialWindowSize, requestEventArgs.MaxPacketSize)
        {
        }

        public SshChannel(uint clientChannel, uint serverChannel, uint windowSize, uint maxPacketSize)
        {
            _open = false;
            _eofSent = false;
            _eofReceived = false;

            this.ClientChannel = clientChannel;
            this.ServerChannel = serverChannel;
            this.WindowSize = windowSize;
            this.MaxPacketSize = maxPacketSize;
        }

        ~SshChannel()
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
                    InternalClose();
                }

                // Dispose unmanaged resources.
            }

            _isDisposed = true;
        }

        public event EventHandler<EventArgs> Opened;

        public event EventHandler<EventArgs> EofSent;

        public event EventHandler<EventArgs> EofReceived;

        public event EventHandler<EventArgs> Closed;

        public event EventHandler<EventArgs> WindowAdjust;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsOpen
        {
            get { return _open; }
        }

        public bool IsEofSent
        {
            get { return _eofSent; }
        }

        public bool IsEofReceived
        {
            get { return _eofReceived; }
        }

        public uint ClientChannel
        {
            get;
            set;
        }

        public uint ServerChannel
        {
            get;
            set;
        }

        public uint WindowSize
        {
            get;
            protected set;
        }

        public uint MaxPacketSize
        {
            get;
            protected set;
        }

        public SshConnectionService ConnectionService
        {
            get { return _connService; }
        }

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public void Close()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            InternalClose();
        }

        public void SendEof()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            _connService.SendMsgChannelEof(this);
            _eofSent = true;

            // Raise event.
            OnEofSent(new EventArgs());
        }

        protected void SendWindowAdjust(uint bytesToAdd)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            _connService.SendMsgChannelWindowAdjust(this, bytesToAdd);
        }

        protected void SendData(byte[] data)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            _connService.SendMsgChannelData(this, data);
        }

        protected void SendExtendedData(byte[] data, SshExtendedDataType dataType)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            _connService.SendMsgChannelExtendedData(this, dataType, data);
        }

        protected internal virtual void Open(SshConnectionService connService)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            if (_open) return;

            _connService = connService;
            _open = true;

            // Raise event.
            OnOpened(new EventArgs());
        }

        protected virtual void InternalClose()
        {
            if (!_open) return;

            // Send close message if client is connected.
            if (_connService.Client.IsConnected) _connService.SendMsgChannelClose(this);

            _open = false;

            // Raise event.
            OnClosed(new EventArgs());
        }

        protected internal virtual void ProcessEof()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            _eofReceived = true;

            // Raise event.
            OnEofReceived(new EventArgs());
        }

        protected internal virtual void ProcessRequest(string requestType, bool wantReply, SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            switch (requestType)
            {
                case "signal":
                    // Process signal.
                    ProcessSignal(msgReader.ReadString());

                    if (wantReply) _connService.SendMsgChannelSuccess(this);
                    return;
                default:
                    // Unrecognised request type.
                    break;
            }

            // Request has failed.
            if (wantReply) _connService.SendMsgChannelFailure(this);
        }

        protected internal virtual void ProcessWindowAdjust(uint bytesToAdd)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Raise event.
            OnWindowAdjust(new DataReceivedEventArgs());
        }

        protected internal virtual void ProcessData(byte[] data)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Raise event.
            OnDataReceived(new DataReceivedEventArgs());
        }

        protected internal virtual void ProcessExtendedData(SshExtendedDataType dataType, byte[] data)
        {
            // Raise event.
            OnDataReceived(new DataReceivedEventArgs(dataType));
        }

        protected internal virtual void ProcessSignal(string signalName)
        {
            // empty
        }

        protected internal virtual void WriteChannelOpenConfirmationData()
        {
            // empty
        }

        protected virtual void OnOpened(EventArgs e)
        {
            if (Opened != null) Opened(this, e);
        }

        protected virtual void OnEofSent(EventArgs e)
        {
            if (EofSent != null) EofSent(this, e);
        }

        protected virtual void OnEofReceived(EventArgs e)
        {
            if (EofReceived != null) EofReceived(this, e);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            if (Closed != null) Closed(this, e);
        }

        protected virtual void OnWindowAdjust(EventArgs e)
        {
            if (WindowAdjust != null) WindowAdjust(this, e);
        }

        protected virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            if (DataReceived != null) DataReceived(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) PropertyChanged(this, e);
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs()
            : this(SshExtendedDataType.Normal)
        {
        }

        public DataReceivedEventArgs(SshExtendedDataType dataType)
            : base()
        {
            this.DataType = dataType;
        }

        public SshExtendedDataType DataType
        {
            get;
            protected set;
        }
    }
}
