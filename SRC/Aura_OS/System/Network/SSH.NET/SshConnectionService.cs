using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public class SshConnectionService : SshService
    {
        protected List<SshChannel> _channels; // List of all currently open channels.

        private bool _isDisposed = false;     // True if object has been disposed.

        public SshConnectionService(SshClient client)
            : base(client)
        {
            _channels = new List<SshChannel>();
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
                        InternalStop();
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

        public event EventHandler<ChannelOpenRequestEventArgs> ChannelOpenRequest;
        
        public event EventHandler<ChannelEventArgs> ChannelOpened;
        
        public event EventHandler<ChannelEventArgs> ChannelClosed;
        
        public event EventHandler<ChannelEventArgs> ChannelUpdated;

        public List<SshChannel> Channels
        {
            get { return _channels; }
        }

        public override string Name
        {
            get { return "ssh-connection"; }
        }

        internal override bool ProcessMessage(byte[] payload)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create memory stream from payload data.
            using (var msgStream = new MemoryStream(payload))
            using (var msgReader = new SshStreamReader(msgStream))
            {
                // Check message ID.
                SshConnectionMessage messageId = (SshConnectionMessage)msgReader.ReadByte();

#if DEBUG
                if (System.Enum.IsDefined(typeof(SshConnectionMessage), messageId))
                    Debug.WriteLine(string.Format(">>> {0}", System.Enum.GetName(
                        typeof(SshConnectionMessage), messageId)));
#endif

                switch (messageId)
                {
                    // Global request messages
                    case SshConnectionMessage.GlobalRequest:
                        ProcessMsgGlobalRequest(msgReader);
                        break;
                    case SshConnectionMessage.RequestSuccess:
                        ProcessMsgRequestSuccess(msgReader);
                        break;
                    case SshConnectionMessage.RequestFailure:
                        ProcessMsgRequestFailure(msgReader);
                        break;
                    // Channel messages
                    case SshConnectionMessage.ChannelOpen:
                        ProcessMsgChannelOpen(msgReader);
                        break;
                    case SshConnectionMessage.ChannelOpenConfirmation:
                        ProcessMsgChannelOpenConfirmation(msgReader);
                        break;
                    case SshConnectionMessage.ChannelOpenFailure:
                        ProcessMsgChannelOpenFailure(msgReader);
                        break;
                    case SshConnectionMessage.ChannelEof:
                        ProcessMsgChannelEof(msgReader);
                        break;
                    case SshConnectionMessage.ChannelClose:
                        ProcessMsgChannelClose(msgReader);
                        break;
                    case SshConnectionMessage.ChannelRequest:
                        ProcessMsgChannelRequest(msgReader);
                        break;
                    case SshConnectionMessage.ChannelWindowAdjust:
                        ProcessMsgChannelWindowAdjust(msgReader);
                        break;
                    case SshConnectionMessage.ChannelData:
                        ProcessMsgChannelData(msgReader);
                        break;
                    case SshConnectionMessage.ChannelExtendedData:
                        ProcessMsgChannelExtendedData(msgReader);
                        break;
                    // Unrecognised message
                    default:
                        return false;
                }
            }

            // Message was recognised.
            return true;
        }

        internal override void Start()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            base.Start();
        }

        protected override void InternalStop()
        {
            // Dispose each channel.
            foreach (var channel in _channels)
            {
                channel.Closed -= new EventHandler<EventArgs>(channel_Closed);
                channel.Dispose();

                // Raise event.
                if (ChannelClosed != null) ChannelClosed(this, new ChannelEventArgs(channel));
            }

            base.InternalStop();
        }

        internal void SendMsgGlobalRequest(string requestName, bool wantReply, byte[] data)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.GlobalRequest);
                msgWriter.Write(requestName);
                msgWriter.Write(wantReply);
                if (data != null) msgWriter.Write(data);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgRequestSuccess(byte[] data)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.RequestSuccess);
                if (data != null) msgWriter.Write(data);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgRequestFailure()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.RequestFailure);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelOpenConfirmation(SshChannel channel)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelOpenConfirmation);
                msgWriter.Write(channel.ClientChannel);
                msgWriter.Write(channel.ServerChannel);
                msgWriter.Write(channel.WindowSize);
                msgWriter.Write(channel.MaxPacketSize);

                // Write channel-specific data.
                channel.WriteChannelOpenConfirmationData();

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelOpenFailure(SshChannel channel, SshChannelOpenFailureReason reason,
            string description, string language)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelOpenFailure);
                msgWriter.Write(channel.ClientChannel);
                msgWriter.Write((uint)reason);
                msgWriter.WriteByteString(Encoding.UTF8.GetBytes(description));
                msgWriter.Write(language);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelEof(SshChannel channel)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelEof);
                msgWriter.Write(channel.ClientChannel);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelClose(SshChannel channel)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelClose);
                msgWriter.Write(channel.ClientChannel);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelWindowAdjust(SshChannel channel, uint bytesToAdd)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelWindowAdjust);
                msgWriter.Write(channel.ClientChannel);
                msgWriter.Write(bytesToAdd);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelData(SshChannel channel, byte[] data)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Check if data is too large to send.
            if (data.Length > Math.Min(channel.MaxPacketSize, channel.WindowSize))
                throw new InvalidOperationException("Data is too large to send in one packet.");

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelData);
                msgWriter.Write(channel.ClientChannel);
                msgWriter.WriteByteString(data);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelExtendedData(SshChannel channel, SshExtendedDataType dataType,
            byte[] data)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Check if data is too large to send.
            if (data.Length > Math.Min(channel.MaxPacketSize, channel.WindowSize))
                throw new InvalidOperationException("Data is too large to send in one packet.");

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelExtendedData);
                msgWriter.Write(channel.ClientChannel);
                msgWriter.Write((uint)dataType);
                msgWriter.WriteByteString(data);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelRequest(SshChannel channel, string requestType, bool wantReply,
            byte[] requestData)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelRequest);
                msgWriter.Write(channel.ClientChannel);
                msgWriter.Write(requestType);
                msgWriter.Write(wantReply);
                msgWriter.Write(requestData);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelSuccess(SshChannel channel)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelSuccess);
                msgWriter.Write(channel.ClientChannel);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        internal void SendMsgChannelFailure(SshChannel channel)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshConnectionMessage.ChannelFailure);
                msgWriter.Write(channel.ClientChannel);

                _client.SendPacket<SshConnectionMessage>(msgStream.ToArray());
            }
        }

        protected void ProcessMsgGlobalRequest(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read request information.
            string requestName = msgReader.ReadString();
            bool wantReply = msgReader.ReadBoolean();

            switch (requestName)
            {
                case "tcpip-forward":
                    throw new NotImplementedException();

                    //if (wantReply) SendMsgRequestSuccess(null);

                    //return;
                default:
                    // Unrecognised request type.
                    break;
            }

            // Request has failed.
            if (wantReply) SendMsgRequestFailure();
        }

        protected void ProcessMsgRequestSuccess(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            //
        }

        protected void ProcessMsgRequestFailure(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            //
        }

        protected void ProcessMsgChannelOpen(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read channel information.
            string channelType = msgReader.ReadString();
            uint senderChannel = msgReader.ReadUInt32();
            uint initialWindowSize = msgReader.ReadUInt32();
            uint maxPacketSize = msgReader.ReadUInt32();

            // Check channel type.
            switch (channelType)
            {
                default:
                    // Raise event to request channel.
                    var channelRequestedEventArgs = new ChannelOpenRequestEventArgs(senderChannel,
                        (uint)_channels.Count, channelType, initialWindowSize, maxPacketSize);

                    if (ChannelOpenRequest != null) ChannelOpenRequest(this, channelRequestedEventArgs);

                    var channel = channelRequestedEventArgs.Channel;

                    // Check if channel was created.
                    if (channel != null)
                    {
                        channel.Opened += new EventHandler<EventArgs>(channel_Opened);
                        channel.Closed += new EventHandler<EventArgs>(channel_Closed);
                        channel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(
                            channel_PropertyChanged);
                        channel.Open(this);

                        _channels.Add(channel);

                        // Send confirmation message.
                        SendMsgChannelOpenConfirmation(channel);
                    }
                    else
                    {
                        string failureDescription = channelRequestedEventArgs.FailureDescription;

                        if (failureDescription != null)
                        {
                            // Create description of failure from reason code.
                            switch (channelRequestedEventArgs.FailureReason)
                            {
                                case SshChannelOpenFailureReason.AdministrativelyProhibited:
                                    failureDescription = "Administratively prohibited.";
                                    break;
                                case SshChannelOpenFailureReason.ConnectFailed:
                                    failureDescription = "Connect attempt failed.";
                                    break;
                                case SshChannelOpenFailureReason.UnknownChannelType:
                                    failureDescription = string.Format("Unknown channel type '{0}'.",
                                        channelType);
                                    break;
                                case SshChannelOpenFailureReason.ResourceShortage:
                                    failureDescription = "Resource shortage on server.";
                                    break;
                            }
                        }

                        // Channel open request has failed.
                        SendMsgChannelOpenFailure(channel, channelRequestedEventArgs.FailureReason,
                            failureDescription, "");
                        return;
                    }

                    break;
            }
        }

        protected void ProcessMsgChannelOpenConfirmation(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            //
        }

        protected void ProcessMsgChannelOpenFailure(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            //
        }

        protected void ProcessMsgChannelEof(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read channel number and get channel object.
            uint channelNum = msgReader.ReadUInt32();
            SshChannel channel;

            try { channel = _channels.SingleOrDefault(item => item.ServerChannel == channelNum); }
            catch (InvalidOperationException) { return; }

            channel.ProcessEof();
        }

        protected void ProcessMsgChannelClose(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read channel number and get channel object.
            uint channelNum = msgReader.ReadUInt32();
            SshChannel channel;

            try { channel = _channels.SingleOrDefault(item => item.ServerChannel == channelNum); }
            catch (InvalidOperationException) { return; }

            channel.Close();
        }

        protected void ProcessMsgChannelRequest(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read channel number and get channel object.
            uint channelNum = msgReader.ReadUInt32();
            SshChannel channel;

            try { channel = _channels.SingleOrDefault(item => item.ServerChannel == channelNum); }
            catch (InvalidOperationException) { return; }

            string requestType = msgReader.ReadString();
            bool wantReply = msgReader.ReadBoolean();

            // Let channel process request.
            channel.ProcessRequest(requestType, wantReply, msgReader);
        }

        protected void ProcessMsgChannelWindowAdjust(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read channel number and get channel object.
            uint channelNum = msgReader.ReadUInt32();
            SshChannel channel;

            try { channel = _channels.SingleOrDefault(item => item.ServerChannel == channelNum); }
            catch (InvalidOperationException) { return; }

            // Let channel adjust window size.
            var bytesToAdd = msgReader.ReadUInt32();

            channel.ProcessWindowAdjust(bytesToAdd);
        }

        protected void ProcessMsgChannelData(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read channel number and get channel object.
            uint channelNum = msgReader.ReadUInt32();
            SshChannel channel;

            try { channel = _channels.SingleOrDefault(item => item.ServerChannel == channelNum); }
            catch (InvalidOperationException) { return; }

            // Let channel read data.
            var data = msgReader.ReadByteString();

            channel.ProcessData(data);
        }

        protected void ProcessMsgChannelExtendedData(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read channel number and get channel object.
            uint channelNum = msgReader.ReadUInt32();
            SshChannel channel;

            try { channel = _channels.SingleOrDefault(item => item.ServerChannel == channelNum); }
            catch (InvalidOperationException) { return; }

            // Let channel read extended data.
            var dataType = (SshExtendedDataType)msgReader.ReadUInt32();
            var data = msgReader.ReadByteString();

            channel.ProcessExtendedData(dataType, data);
        }

        private void channel_Opened(object sender, EventArgs e)
        {
            var channel = sender as SshChannel;

            // Raise event.
            if (ChannelOpened != null) ChannelOpened(this, new ChannelEventArgs(channel));
        }

        private void channel_Closed(object sender, EventArgs e)
        {
            var channel = sender as SshChannel;

            // Remove channel from list of open channels.
            _channels.Remove(channel);

            // Raise event.
            if (ChannelClosed != null) ChannelClosed(this, new ChannelEventArgs(channel));
        }

        private void channel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ChannelUpdated != null)
                ChannelUpdated(this, new ChannelEventArgs((SshChannel)sender));
        }
    }

    public class ChannelEventArgs : EventArgs
    {
        public ChannelEventArgs(SshChannel channel)
            : base()
        {
            this.Channel = channel;
        }

        public SshChannel Channel
        {
            get;
            protected set;
        }
    }

    public class ChannelOpenRequestEventArgs : EventArgs
    {
        public ChannelOpenRequestEventArgs(uint clientChannel, uint serverChannel, string channelType,
            uint initialWindowSize, uint maxPacketSize)
            : base()
        {
            this.ClientChannel = clientChannel;
            this.ServerChannel = serverChannel;
            this.ChannelType = channelType;
            this.InitialWindowSize = initialWindowSize;
            this.MaxPacketSize = maxPacketSize;

            this.Channel = null;
            this.FailureReason = SshChannelOpenFailureReason.UnknownChannelType;
            this.FailureDescription = null;
        }

        public SshChannel Channel
        {
            get;
            set;
        }

        public SshChannelOpenFailureReason FailureReason
        {
            get;
            set;
        }

        public string FailureDescription
        {
            get;
            set;
        }

        public uint ClientChannel
        {
            get;
            protected set;
        }

        public uint ServerChannel
        {
            get;
            protected set;
        }

        public string ChannelType
        {
            get;
            protected set;
        }

        public uint InitialWindowSize
        {
            get;
            protected set;
        }

        public uint MaxPacketSize
        {
            get;
            protected set;
        }
    }

    public enum SshExtendedDataType : uint
    {
        Normal = 0,
        StdErr = 1
    }

    public enum SshChannelOpenFailureReason : uint
    {
        AdministrativelyProhibited = 1,
        ConnectFailed = 2,
        UnknownChannelType = 3,
        ResourceShortage = 4
    }

    internal enum SshConnectionMessage : byte
    {
        GlobalRequest = 80,
        RequestSuccess = 81,
        RequestFailure = 82,
        ChannelOpen = 90,
        ChannelOpenConfirmation = 91,
        ChannelOpenFailure = 92,
        ChannelWindowAdjust = 93,
        ChannelData = 94,
        ChannelExtendedData = 95,
        ChannelEof = 96,
        ChannelClose = 97,
        ChannelRequest = 98,
        ChannelSuccess = 99,
        ChannelFailure = 100,
    }
}
