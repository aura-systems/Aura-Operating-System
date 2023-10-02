using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using SshDotNet.Algorithms;

namespace SshDotNet
{
    public class SshClient : IDisposable
    {
        static SshClient()
        {
            // Set default software version.
            SshClient.SoftwareVersion = "WindowsSshServer" + 
                Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static string SoftwareVersion
        {
            get;
            set;
        }

        protected const string _protocolVersion = "2.0"; // Implemented version of SSH protocol.
        protected const uint _maxPacketSize = 35000;     // Maximum total size of packet.

        protected List<SshService> _services;            // List of supported services.
        protected SshService _activeService;             // Active service.

        protected EncryptionAlgorithm _encAlgCtoS;       // Algorithm to use for encryption of payloads sent from server to client.
        protected EncryptionAlgorithm _encAlgStoC;       // Algorithm to use for encryption of payloads sent from client to server.
        protected MacAlgorithm _macAlgCtoS;              // Algorithm to use for computing MACs (Message Authentication Codes) sent from server to client.
        protected MacAlgorithm _macAlgStoC;              // Algorithm to use for computing MACs (Message Authentication Codes) sent from client to server.
        protected CompressionAlgorithm _compAlgCtoS;     // Algorithm to use for compression of payloads sent from client to server.
        protected CompressionAlgorithm _compAlgStoC;     // Algorithm to use for compression of payloads sent from server to client.
        protected string _languageCtoS;                  // Language to use for messages sent from client to server.
        protected string _languageStoC;                  // Language to use for messages sent from server to client.
        protected ICryptoTransform _cryptoTransformCtoS; // Encryptor for messages sent from client to server.
        protected ICryptoTransform _cryptoTransformStoC; // Decryptor for messages sent from server to client.

        protected EncryptionAlgorithm _newEncAlgCtoS;    // New algorithm to use for encryption of payloads sent from server to client.
        protected EncryptionAlgorithm _newEncAlgStoC;    // New algorithm to use for encryption of payloads sent from client to server.
        protected MacAlgorithm _newMacAlgCtoS;           // New algorithm to use for computing MACs (Message Authentication Codes) sent from server to client.
        protected MacAlgorithm _newMacAlgStoC;           // New algorithm to use for computing MACs (Message Authentication Codes) sent from client to server.
        protected CompressionAlgorithm _newCompAlgCtoS;  // New algorithm to use for compression of payloads sent from client to server.
        protected CompressionAlgorithm _newCompAlgStoC;  // New algorithm to use for compression of payloads sent from server to client.
        protected string _newLanguageCtoS;               // Language to use for messages sent from client to server.
        protected string _newLanguageStoC;               // Language to use for messages sent from server to client.

        protected RNGCryptoServiceProvider _rng;         // Random number generator.
        protected uint _sendPacketSeqNumber;             // Sequence number of next packet to send.
        protected uint _receivePacketSeqNumber;          // Sequence number of next packet to be received.
        protected string _serverIdString;                // ID string for server.
        protected string _clientIdString;                // ID string for client.
        protected byte[] _serverKexInitPayload;          // Payload of Kex Init message sent by server.
        protected byte[] _clientKexInitPayload;          // Payload of Kex Init message sent by client.
        protected KexAlgorithm _kexAlg;                  // Algorithm to use for kex (key exchange).
        protected PublicKeyAlgorithm _hostKeyAlg;        // Algorithm to use for encrypting host key.
        protected byte[] _exchangeHash;                  // Current exchange hash.
        protected byte[] _sessionId;                     // Session identifier, which is the first exchange hash.

        protected IConnection _connection;               // Connection to client that provides data stream.
        protected Stream _stream;                        // Connection stream, through which data is transmitted.
        protected MeasureableStream _measuredStream;     // Measures data transmission of connection stream.
        protected SshStreamWriter _streamWriter;         // Writes SSH data to stream.
        protected SshStreamReader _streamReader;         // Reads SSH data from stream.
        protected Thread _receiveThread;                 // Thread on which to wait for received data.

        protected object _streamLock = new object();
        
        private bool _isDisposed = false;                // True if object has been disposed.

        public SshClient(IConnection connection)
            : this(connection, true)
        {
        }

        public SshClient(Stream stream)
            : this(stream, true)
        {
        }

        public SshClient(IConnection connection, bool addDefaultAlgorithms)
            : this(connection.GetStream(), addDefaultAlgorithms)
        {
            _connection = connection;
        }

        public SshClient(Stream stream, bool addDefaultAlgorithms)
        {
            _stream = stream;
            _measuredStream = new MeasureableStream(_stream);

            // Create RNG (random number generator).
            _rng = new RNGCryptoServiceProvider();

            // Initialize properties to default values.
            this.ServerComments = null;
            this.Languages = null;
            this.KeyReExchangeByteLimit = 0x40000000; // 1GB

            this.KexAlgorithms = new List<KexAlgorithm>();
            this.PublicKeyAlgorithms = new List<PublicKeyAlgorithm>();
            this.EncryptionAlgorithms = new List<EncryptionAlgorithm>();
            this.MacAlgorithms = new List<MacAlgorithm>();
            this.CompressionAlgorithms = new List<CompressionAlgorithm>();

            // Add default algorithms to lists of supported algorithms.
            if (addDefaultAlgorithms) AddDefaultAlgorithms();

            // Add default services to list of supported services.
            _services = new List<SshService>();
            _services.Add(new SshAuthenticationService(this));
            _services.Add(new SshConnectionService(this));
        }

        ~SshClient()
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
                    Disconnect();
                    if (_connection != null) _connection.Dispose();
                    if (_measuredStream != null) _measuredStream.Dispose();

                    foreach (var service in _services) service.Dispose();
                }

                // Dispose unmanaged resources.
            }

            _isDisposed = true;
        }

        public event EventHandler<EventArgs> Connected;
        
        public event EventHandler<SshDisconnectedEventArgs> Disconnected;
        
        public event EventHandler<EventArgs> Error;
        
        public event EventHandler<SshDebugMessageReceivedEventArgs> DebugMessageReceived;
        
        public event EventHandler<SshKeyExchangeInitializedEventArgs> KeyExchangeInitialized;

        public List<SshService> Services
        {
            get { return _services; }
        }

        public SshAuthenticationService AuthenticationService
        {
            get
            {
                return (SshAuthenticationService)_services.SingleOrDefault(item => item is
                    SshAuthenticationService);
            }
        }

        public SshConnectionService ConnectionService
        {
            get
            {
                return (SshConnectionService)_services.SingleOrDefault(item => item is SshConnectionService);
            }
        }

        public byte[] SessionId
        {
            get
            {
                if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

                return _sessionId;
            }
        }

        public int BytesTransmittedSinceLastKex
        {
            get
            {
                if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

                return _measuredStream.TotalBytesTransmitted;
            }
        }

        public string ClientProtocolVersion
        {
            get;
            protected set;
        }

        public string ClientSoftwareVersion
        {
            get;
            protected set;
        }

        public string ClientComments
        {
            get;
            protected set;
        }

        public List<KexAlgorithm> KexAlgorithms
        {
            get;
            protected set;
        }

        public List<PublicKeyAlgorithm> PublicKeyAlgorithms
        {
            get;
            protected set;
        }

        public List<EncryptionAlgorithm> EncryptionAlgorithms
        {
            get;
            protected set;
        }

        public List<MacAlgorithm> MacAlgorithms
        {
            get;
            protected set;
        }

        public List<CompressionAlgorithm> CompressionAlgorithms
        {
            get;
            protected set;
        }

        public string ServerComments
        {
            get;
            set;
        }

        public IList<string> Languages
        {
            get;
            set;
        }

        public int KeyReExchangeByteLimit
        {
            get;
            set;
        }

        public uint MaxPacketSize
        {
            get { return _maxPacketSize; }
        }

        public IConnection Connection
        {
            get { return _connection; }
        }

        public bool IsConnected
        {
            get { return _stream != null; }
        }

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public void ReexchangeKeys()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Set all new algorithms to null.
            _newEncAlgCtoS = null;
            _newEncAlgStoC = null;
            _newMacAlgCtoS = null;
            _newMacAlgStoC = null;
            _newCompAlgCtoS = null;
            _newCompAlgStoC = null;

            // Send kex initialization message.
            SendMsgKexInit();
        }

        public virtual void ConnectionEstablished()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create network objects.
            _streamWriter = new SshStreamWriter(_measuredStream);
            _streamReader = new SshStreamReader(_measuredStream);

            _sendPacketSeqNumber = 0;
            _receivePacketSeqNumber = 0;
            _serverIdString = null;
            _clientIdString = null;
            _sessionId = null;

            // Create thread on which to receive data from connection.
            _receiveThread = new Thread(new ThreadStart(ReceiveData));
            _receiveThread.Name = "Connection";
            _receiveThread.Start();

            // Notify connection object that connection has been established.
            if (_connection != null) _connection.ConnectionEstablished();

            // Client has connected.
            OnConnected(new EventArgs());
        }

        public void Disconnect()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            Disconnect("");
        }

        public void Disconnect(string message)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            Disconnect(SshDisconnectReason.ByApplication, message);
        }

        internal void Disconnect(SshDisconnectReason reason, string description)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            Disconnect(reason, description, "");
        }

        internal void Disconnect(SshDisconnectReason reason, string description, string language)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            try
            {
                if (this.IsConnected)
                {
                    // Send Disconnect message to client.
                    SendMsgDisconnect(reason, description, language);
                }
            }
            finally
            {
                // Disconnect from client.
                Disconnect(false);
            }
        }

        internal virtual void Disconnect(bool remotely)
        {
            //if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            Disconnect(remotely, SshDisconnectReason.None, null, null);
        }

        internal virtual void Disconnect(bool remotely, SshDisconnectReason reason, string description,
            string language)
        {
            //if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            lock (_streamLock)
            {
                // Check if connection is already closed.
                if (_stream == null) return;

                if (!remotely)
                {
                    // Stop active service.
                    if (_activeService != null) _activeService.Stop();
                    //foreach (var service in _services) service.Stop();
                }

                // Dispose objects used for data transmission.
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
                if (_streamWriter != null)
                {
                    _streamWriter.Close();
                    _streamWriter = null;
                }
                if (_streamReader != null)
                {
                    _streamReader.Close();
                    _streamReader = null;
                }
                if (_receiveThread != null && Thread.CurrentThread != _receiveThread)
                {
                    _receiveThread.Abort();
                    _receiveThread = null;
                }

                // Dispose algorithms (current and new).
                DisposeCurrentAlgorithms();

                if (_newEncAlgCtoS != null)
                {
                    _newEncAlgCtoS.Dispose();
                    _newEncAlgCtoS = null;
                }
                if (_newEncAlgStoC != null)
                {
                    _newEncAlgStoC.Dispose();
                    _newEncAlgStoC = null;
                }
                if (_newMacAlgCtoS != null)
                {
                    _newMacAlgCtoS.Dispose();
                    _newMacAlgCtoS = null;
                }
                if (_newMacAlgStoC != null)
                {
                    _newMacAlgStoC.Dispose();
                    _newMacAlgStoC = null;
                }
                if (_newCompAlgCtoS != null)
                {
                    _newCompAlgCtoS.Dispose();
                    _newCompAlgCtoS = null;
                }
                if (_newCompAlgStoC != null)
                {
                    _newCompAlgStoC.Dispose();
                    _newCompAlgStoC = null;
                }

                // Disconnect using connection object.
                if (_connection != null) _connection.Disconnect(remotely);
            }

            // Client has disconnected.
            OnDisconnected(new SshDisconnectedEventArgs(remotely, reason, description, language));
        }

        internal void StartService(string serviceName)
        {
            // Try to find service in list of supported services.
            SshService service = _services.SingleOrDefault(item => item.Name == serviceName);

            if (service != null)
            {
                // Stop active service, if there is one.
                if (_activeService != null) _activeService.Stop();

                // Activate requested service.
                _activeService = service;
                _activeService.Start();
            }
            else
            {
                // Service was not found.
                Disconnect(SshDisconnectReason.ServiceNotAvailable, string.Format(
                    "The service with name {0} is not supported by this server.", serviceName));
                throw new DisconnectedException();
            }
        }

        internal void SendPacket<T>(byte[] payload)
        {
            Debug.WriteLine(string.Format("<<< {0}", System.Enum.GetName(typeof(T), (byte)payload[0])));

            SendPacket(payload);
        }

        internal void SendPacket(byte[] payload)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Calculate block size.
            byte blockSize = (byte)(_encAlgStoC == null ? 8 : _encAlgStoC.Algorithm.BlockSize / 8);

            // Compress payload data.
            byte[] compressedPayload = (_compAlgStoC == null ? payload : _compAlgStoC.Compress(payload));

            // Calculate random number of extra blocks of padding to add.
            int maxNumExtraPaddingBlocks = (256 - blockSize) / blockSize;
            int numExtraPaddingBlocks = _rng.GetNumber(0, maxNumExtraPaddingBlocks);

            // Calculate packet length information.
            // Packet size (minus MAC) must be multiple of 8 or cipher block size (whichever is larger).
            // Minimum packet size (minus MAC) is 16.
            // Padding length must be between 4 and 255 bytes.
            byte paddingLength = (byte)(blockSize - ((5 + compressedPayload.Length) % blockSize)
                + numExtraPaddingBlocks * blockSize);
            if (paddingLength < 4) paddingLength += blockSize;
            uint packetLength = (uint)compressedPayload.Length + paddingLength + 1;
            byte[] padding;

            if (packetLength < 12)
            {
                paddingLength += blockSize;
                packetLength += blockSize;
            }

            // Create packet data to send.
            // Note: temporary stream is needed so that packet data and MAC are sent at exactly the
            // same time.
            using (var tempStream = new MemoryStream())
            {
                using (var packetStream = new MemoryStream())
                {
                    using (var packetWriter = new SshStreamWriter(packetStream))
                    {
                        // Write length of packet.
                        packetWriter.Write(packetLength);

                        // Write length of random padding.
                        packetWriter.Write(paddingLength);

                        // Write payload data.
                        packetWriter.Write(compressedPayload);

                        // Write bytes of random padding.
                        padding = new byte[paddingLength];
                        _rng.GetBytes(padding);
                        packetWriter.Write(padding);
                    }

                    // Get packet data.
                    var packetData = packetStream.ToArray();

                    if (_encAlgStoC != null)
                    {
                        // Write encrypted packet data to stream.
                        var cryptoStream = new CryptoStream(tempStream, _cryptoTransformStoC,
                            CryptoStreamMode.Write);

                        // Write packet data to crypto stream.
                        cryptoStream.Write(packetData, 0, packetData.Length);
                    }
                    else
                    {
                        // Write plain packet data to stream.
                        tempStream.Write(packetData, 0, packetData.Length);
                    }
                }

                // Write MAC (Message Authentication Code), if MAC algorithm has been agreed on.
                if (_macAlgStoC != null)
                {
                    var mac = ComputeMac(_macAlgStoC, _sendPacketSeqNumber, packetLength, paddingLength,
                        compressedPayload, padding);
                    tempStream.Write(mac, 0, mac.Length);
                }

                lock (_streamLock)
                {
                    // Write packet to stream.
                    _streamWriter.Write(tempStream.ToArray());
                }
            }

            // Increment sequence number of next packet to send.
            unchecked { _sendPacketSeqNumber++; }

            // Consider whether to re-exchange keys now.
            ConsiderKeyReExchange();
        }

        internal void ReadPacket()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            CachedStream cachedStream = null;
            Stream cryptoStream = null;
            uint packetLength;
            byte paddingLength;
            byte[] compressedPayload;
            byte[] mac;

            //lock (_streamLock)
            //{
            try
            {
                if (_encAlgCtoS != null)
                {
                    // Packet is encrypted. Use crypto stream for reading.
                    cachedStream = new CachedStream(_stream);
                    cryptoStream = new CryptoStream(cachedStream, _cryptoTransformCtoS, CryptoStreamMode.Read);
                }
                else
                {
                    // Packet is unencrypted. Use normal network stream for reading.
                    cryptoStream = _stream;
                }

                var cryptoStreamReader = new SshStreamReader(cryptoStream);

                // Read packet length information.
                packetLength = cryptoStreamReader.ReadUInt32();
                paddingLength = cryptoStreamReader.ReadByte();

                if (paddingLength < 4)
                {
                    // Invalid padding length.
                    Disconnect(SshDisconnectReason.MacError, string.Format(
                        "Invalid padding length", _receivePacketSeqNumber));
                    throw new DisconnectedException();
                }

                // Read payload data.
                compressedPayload = new byte[packetLength - 1 - paddingLength];

                cryptoStreamReader.Read(compressedPayload, 0, compressedPayload.Length);

                // Skip bytes of random padding.
                byte[] padding = cryptoStreamReader.ReadBytes(paddingLength);

                // Check if currently using MAC algorithm.
                if (_macAlgCtoS != null)
                {
                    // Read MAC (Message Authentication Code). MAC is always unencrypted.
                    mac = new byte[_macAlgCtoS.DigestLength];
                    int macBytesRead = ReadCryptoStreamBuffer(cachedStream, mac, 0);
                    int macBytesLeft = mac.Length - macBytesRead;

                    if (macBytesLeft > 0) _streamReader.Read(mac, macBytesRead, macBytesLeft);

                    if (macBytesRead > 0)
                    {
                        // HACK: Recreate decryptor with correct IV.
                        _cryptoTransformCtoS.Dispose();
                        _cryptoTransformCtoS = _encAlgCtoS.Algorithm.CreateDecryptor(_encAlgCtoS.Algorithm.Key,
                            cachedStream.GetBufferEnd(0, _cryptoTransformCtoS.InputBlockSize));
                    }

                    // Verify MAC of received packet.
                    var expectedMac = ComputeMac(_macAlgCtoS, _receivePacketSeqNumber, packetLength,
                        paddingLength, compressedPayload, padding);

                    if (!mac.ArrayEquals(expectedMac))
                    {
                        // Invalid MAC.
                        Disconnect(SshDisconnectReason.MacError, string.Format(
                            "Invalid MAC for packet #{0}", _receivePacketSeqNumber));
                        throw new DisconnectedException();
                    }
                }
                else
                {
                    // Set MAC to empty array.
                    mac = new byte[0];
                }
            }
            catch (CryptographicException exCrypto)
            {
                // Check if at end of stream.
                if (_stream.ReadByte() == -1)
                {
                    Disconnect(true);
                    throw new DisconnectedException();
                }
                else
                {
                    throw exCrypto;
                }
            }
            //}

            // Check that packet length does not exceed maximum.
            if (4 + packetLength + mac.Length > _maxPacketSize)
            {
                Disconnect(SshDisconnectReason.ProtocolError, "Packet length exceeds maximum.");
                throw new DisconnectedException();
            }

            // Decompress payload data.
            byte[] payload = (_compAlgCtoS == null ? compressedPayload
                : _compAlgCtoS.Decompress(compressedPayload));

            try
            {
                // Process received message for client and active service.
                bool messageRecognised = false;

                messageRecognised = ProcessMessage(payload);
                if (!messageRecognised && _activeService != null)
                    messageRecognised = _activeService.ProcessMessage(payload);

                if (!messageRecognised)
                {
                    // Tell client that message was unrecognised.
                    SendMsgUnimplemented();
                }
            }
            catch (EndOfStreamException)
            {
                // Received message seems to be malformed.
                Disconnect(SshDisconnectReason.ProtocolError, "Bad message format.");
                throw new DisconnectedException();
            }

            // Increment sequence number of next packet to be received.
            unchecked { _receivePacketSeqNumber++; }

            // Consider whether to re-exchange keys now.
            ConsiderKeyReExchange();
        }

        // This method is just a temporary hack to get around the problem that a CryptoStream reads
        // farther ahead than necessary, which causes problems when reading encrypted and plain data from
        // a stream.
        protected int ReadCryptoStreamBuffer(CachedStream stream, byte[] buffer, int offset)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            //// Get internal buffer of crypto stream.
            //byte[] cryptoStreamBuffer = (byte[])typeof(CryptoStream).GetField("_InputBuffer",
            //    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(stream);
            //int cryptoStreamBufferIndex = (int)typeof(CryptoStream).GetField("_InputBufferIndex",
            //    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(stream);
            //int bytesRead = cryptoStreamBuffer.Length - cryptoStreamBufferIndex;

            //// Copy unread data from stream buffer into output buffer.
            //Buffer.BlockCopy(cryptoStreamBuffer, cryptoStreamBufferIndex, buffer, offset, bytesRead);

            // Copy last buffer from cached stream to output buffer.
            var inputBuffer = stream.GetBuffer(1);

            Buffer.BlockCopy(inputBuffer, 0, buffer, offset, inputBuffer.Length);

            return inputBuffer.Length;
        }

        protected virtual void AddDefaultAlgorithms()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Ajoutez les algorithmes pris en charge à vos listes d'algorithmes.

            // Algorithme d'échange de clés (KexAlgorithm)
            //this.KexAlgorithms.Add(new SshDiffieHellmanGroup14Sha1());

            // Algorithmes de clé publique (PublicKeyAlgorithm)
            this.PublicKeyAlgorithms.Add(new SshRsa());

            // Algorithmes de chiffrement (EncryptionAlgorithm)
            this.EncryptionAlgorithms.Add(new SshAes256Cbc());
            this.EncryptionAlgorithms.Add(new SshAes192Cbc());
            this.EncryptionAlgorithms.Add(new SshAes128Cbc());

            // Algorithmes d'intégrité (MacAlgorithm)
            this.MacAlgorithms.Add(new SshHmacSha1());

            // Algorithmes de compression
            this.CompressionAlgorithms.Add(new SshNoCompression());
        }

        protected void DisposeCurrentAlgorithms()
        {
            // Dispose all current algorithms
            if (_kexAlg != null)
            {
                _kexAlg.Dispose();
                _kexAlg = null;
            }
            if (_hostKeyAlg != null)
            {
                _hostKeyAlg.Dispose();
                _hostKeyAlg = null;
            }
            if (_encAlgCtoS != null)
            {
                _encAlgCtoS.Dispose();
                _encAlgCtoS = null;
            }
            if (_encAlgStoC != null)
            {
                _encAlgStoC.Dispose();
                _encAlgStoC = null;
            }
            if (_macAlgCtoS != null)
            {
                _macAlgCtoS.Dispose();
                _macAlgCtoS = null;
            }
            if (_macAlgStoC != null)
            {
                _macAlgStoC.Dispose();
                _macAlgStoC = null;
            }
            if (_compAlgCtoS != null)
            {
                _compAlgCtoS.Dispose();
                _compAlgCtoS = null;
            }
            if (_compAlgStoC != null)
            {
                _compAlgStoC.Dispose();
                _compAlgStoC = null;
            }
            if (_cryptoTransformCtoS != null)
            {
                _cryptoTransformCtoS.Dispose();
                _cryptoTransformCtoS = null;
            }
            if (_cryptoTransformStoC != null)
            {
                _cryptoTransformStoC.Dispose();
                _cryptoTransformStoC = null;
            }
        }

        protected void SendLine(string value)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Write chars of line to stream.
            byte[] buffer = Encoding.ASCII.GetBytes(value + "\r\n");

            lock (_streamLock)
            {
                _streamWriter.Write(buffer);
            }
        }

        protected string ReadLine()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            var lineBuilder = new StringBuilder();
            bool lineContainsCr = false;

            // Read chars from stream until LF (line feed) is found.
            char curChar;

            do
            {
                curChar = _streamReader.ReadChar();
                lineBuilder.Append(curChar);

                if (curChar == '\r') lineContainsCr = true;
            } while (curChar != '\n');

            // Return line without trailing CR and LF.
            return lineBuilder.ToString(0, lineBuilder.Length - (lineContainsCr ? 2 : 1));
        }

        protected void SendMsgKexDhReply(byte[] hostKeyAndCerts, byte[] clientExchangeValue,
            byte[] serverExchangeValue)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            {
                using (var msgWriter = new SshStreamWriter(msgStream))
                {
                    msgWriter.Write((byte)SshMessage.KexDhReply);
                    msgWriter.WriteByteString(hostKeyAndCerts);
                    msgWriter.WriteMPint(serverExchangeValue);
                    msgWriter.WriteByteString(_hostKeyAlg.CreateSignatureData(_exchangeHash));
                }

                SendPacket<SshMessage>(msgStream.ToArray());
            }
        }

        private bool CheckByteArray(byte[] serverArray, string clientArrayText)
        {
            byte[] clientArray = (from item in clientArrayText.Split(new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries)
                                  select byte.Parse(item)).ToArray();

            return serverArray.ArrayEquals(clientArray);
        }

        protected void SendMsgDisconnect(SshDisconnectReason reason, string description, string language)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            {
                using (var msgWriter = new SshStreamWriter(msgStream))
                {
                    msgWriter.Write((byte)SshMessage.Disconnect);
                    msgWriter.Write((uint)reason);
                    msgWriter.WriteByteString(Encoding.UTF8.GetBytes(description));
                    msgWriter.Write(language);
                }

                SendPacket<SshMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgIgnore()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            {
                using (var msgWriter = new SshStreamWriter(msgStream))
                {
                    msgWriter.Write((byte)SshMessage.Ignore);

                    // Write random data, which is to be ignored.
                    byte[] data = new byte[_rng.GetNumber(0, 16)];

                    _rng.GetBytes(data);
                    msgWriter.WriteByteString(data);
                }

                SendPacket<SshMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgUnimplemented()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            {
                using (var msgWriter = new SshStreamWriter(msgStream))
                {
                    msgWriter.Write((byte)SshMessage.Unimplemented);

                    // Send sequence number of unrecognised packet.
                    msgWriter.Write(_receivePacketSeqNumber);
                }

                SendPacket<SshMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgKexInit()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            {
                using (var msgWriter = new SshStreamWriter(msgStream))
                {
                    msgWriter.Write((byte)SshMessage.KexInit);

                    // Write random cookie.
                    byte[] cookie = new byte[16];

                    _rng.GetBytes(cookie);
                    msgWriter.Write(cookie);

                    // Write list of kex algorithms.
                    msgWriter.WriteNameList((from alg in this.KexAlgorithms select alg.Name).ToArray());

                    // Write list of server host key algorithms.
                    msgWriter.WriteNameList((from alg in this.PublicKeyAlgorithms select alg.Name).ToArray());

                    // Write lists of encryption algorithms.
                    msgWriter.WriteNameList((from alg in this.EncryptionAlgorithms select alg.Name).ToArray());
                    msgWriter.WriteNameList((from alg in this.EncryptionAlgorithms select alg.Name).ToArray());

                    // Write lists of MAC algorithms.
                    msgWriter.WriteNameList((from alg in this.MacAlgorithms select alg.Name).ToArray());
                    msgWriter.WriteNameList((from alg in this.MacAlgorithms select alg.Name).ToArray());

                    // Write lists of compression algorithms.
                    msgWriter.WriteNameList((from alg in this.CompressionAlgorithms select alg.Name).ToArray());
                    msgWriter.WriteNameList((from alg in this.CompressionAlgorithms select alg.Name).ToArray());

                    // Write lists of languages.
                    string[] langTags = this.Languages == null ? new string[0]
                        : (from lang in this.Languages select lang).ToArray();

                    msgWriter.WriteNameList(langTags);
                    msgWriter.WriteNameList(langTags);

                    // Write whether first (guessed) kex packet follows.
                    msgWriter.Write(false);

                    // Write reserved values.
                    msgWriter.Write(0u);
                }

                _serverKexInitPayload = msgStream.ToArray();
                SendPacket<SshMessage>(_serverKexInitPayload);
            }
        }

        protected void SendMsgNewKeys()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            {
                using (var msgWriter = new SshStreamWriter(msgStream))
                {
                    msgWriter.Write((byte)SshMessage.NewKeys);
                }

                SendPacket<SshMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgServiceAccept(string serviceName)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            {
                using (var msgWriter = new SshStreamWriter(msgStream))
                {
                    msgWriter.Write((byte)SshMessage.ServiceAccept);
                    msgWriter.Write(serviceName);
                }

                SendPacket<SshMessage>(msgStream.ToArray());
            }
        }

        protected bool ProcessMessage(byte[] payload)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create memory stream from payload data.
            using (var msgStream = new MemoryStream(payload))
            {
                using (var msgReader = new SshStreamReader(msgStream))
                {
                    // Check message ID.
                    SshMessage messageId = (SshMessage)msgReader.ReadByte();

#if DEBUG
                    if (System.Enum.IsDefined(typeof(SshMessage), messageId))
                        Debug.WriteLine(string.Format(">>> {0}", System.Enum.GetName(
                            typeof(SshMessage), messageId)));
#endif

                    switch (messageId)
                    {
                        // Standard messages
                        case SshMessage.Disconnect:
                            ProcessMsgDisconnect(msgReader);
                            break;
                        case SshMessage.Ignore:
                            ProcessMsgIgnore(msgReader);
                            break;
                        case SshMessage.Unimplemented:
                            ProcessMsgUnimplemented(msgReader);
                            break;
                        case SshMessage.Debug:
                            ProcessMsgDebug(msgReader);
                            break;
                        case SshMessage.ServiceRequest:
                            ProcessMsgServiceRequest(msgReader);
                            break;
                        case SshMessage.KexInit:
                            // Store payload of message.
                            _clientKexInitPayload = payload;

                            ProcessMsgKexInit(msgReader);
                            break;
                        case SshMessage.NewKeys:
                            ProcessMsgNewKeys(msgReader);
                            break;
                        // Diffie-Hellman kex messages
                        case SshMessage.KexDhInit:
                            ProcessMsgDhKexInit(msgReader);
                            break;
                        // Unrecognised message
                        default:
                            return false;
                    }
                }
            }

            // Message was recognised.
            return true;
        }

        protected void ProcessMsgDisconnect(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read disconnection info.
            SshDisconnectReason reason = (SshDisconnectReason)msgReader.ReadUInt32();
            string description = msgReader.ReadString();
            string language = msgReader.ToString();

            // Disconnect remotely.
            Disconnect(true, reason, description, language);
            throw new DisconnectedException();
        }

        protected void ProcessMsgIgnore(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            try
            {
                // Ignore following data.
                msgReader.ReadString();
            }
            catch (EndOfStreamException)
            {
            }
        }

        protected void ProcessMsgUnimplemented(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read sequence number of packet unrecognised by client. 
            uint packetSeqNumber = msgReader.ReadUInt32();
        }

        protected void ProcessMsgDebug(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read debug information.
            bool alwaysDisplay = msgReader.ReadBoolean();
            string message = msgReader.ReadString();
            string language = msgReader.ToString();

            Debug.WriteLine("Debug message received.");
            Debug.WriteLine("Language: {0}", language);
            Debug.WriteLine(message);

            // Debug message has been received.
            OnDebugMessageReceived(new SshDebugMessageReceivedEventArgs(alwaysDisplay, message, language));
        }

        protected void ProcessMsgServiceRequest(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read name of service.
            string serviceName = msgReader.ReadString();

            // Start service with specified name.
            StartService(serviceName);

            // Send message to accept requested service.
            SendMsgServiceAccept(_activeService.Name);
        }

        protected void ProcessMsgDhKexInit(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read client exchange value.
            byte[] clientExchangeValue = msgReader.ReadMPInt();

            // Create server kex value.
            byte[] serverExchangeValue = _kexAlg.CreateKeyExchange();

            // Decrypt shared secret from kex value.
            byte[] sharedSecret = _kexAlg.DecryptKeyExchange(clientExchangeValue);

            // Create data for public host key and certificates.
            byte[] hostKeyAndCerts = _hostKeyAlg.CreateKeyAndCertificatesData();

            // Compute exchange hash.
            _exchangeHash = ComputeExchangeHash(hostKeyAndCerts, clientExchangeValue, serverExchangeValue,
                sharedSecret);

            // Set session identifier if it has not already been set.
            if (_sessionId == null) _sessionId = _exchangeHash;

            // Generate keys from shared secret and exchange hash.
            GenerateKeys(sharedSecret);

            // Send Diffie-Hellman reply message.
            SendMsgKexDhReply(hostKeyAndCerts, clientExchangeValue, serverExchangeValue);

            // Send New Keys message to start using new keys.
            SendMsgNewKeys();
        }

        protected void ProcessMsgKexInit(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read random cookie.
            byte[] cookie = msgReader.ReadBytes(16);

            // Read list of kex algorithms.
            string[] kexAlgorithms = msgReader.ReadNameList();
            string[] serverHostKeyAlgorithms = msgReader.ReadNameList();
            string[] encryptionAlgorithmsCtoS = msgReader.ReadNameList();
            string[] encryptionAlgorithmsStoC = msgReader.ReadNameList();
            string[] macAlgorithmsCtoS = msgReader.ReadNameList();
            string[] macAlgorithmsStoC = msgReader.ReadNameList();
            string[] compAlgorithmsCtoS = msgReader.ReadNameList();
            string[] compAlgorithmsStoC = msgReader.ReadNameList();
            string[] langsCtoS = msgReader.ReadNameList();
            string[] langsStoC = msgReader.ReadNameList();
            bool firstKexPacketFollows = msgReader.ReadBoolean();
            uint reserved0 = msgReader.ReadUInt32();

            // Choose algorithms to use.
            _kexAlg = ChooseKexAlgorithm(kexAlgorithms);
            _hostKeyAlg = ChooseHostKeyAlgorithm(serverHostKeyAlgorithms);

            _newEncAlgCtoS = ChooseEncryptionAlgorithm(encryptionAlgorithmsCtoS);
            _newEncAlgStoC = ChooseEncryptionAlgorithm(encryptionAlgorithmsStoC);
            _newMacAlgCtoS = ChooseMacAlgorithm(macAlgorithmsCtoS);
            _newMacAlgStoC = ChooseMacAlgorithm(macAlgorithmsStoC);
            _newCompAlgCtoS = ChooseCompressionAlgorithm(compAlgorithmsCtoS);
            _newCompAlgStoC = ChooseCompressionAlgorithm(compAlgorithmsStoC);
            _newLanguageCtoS = ChooseLanguage(langsCtoS);
            _newLanguageStoC = ChooseLanguage(langsStoC);

            // Raise event, key exchange has completed.
            if (KeyExchangeInitialized != null) OnKeyExchangeInitialized(
                new SshKeyExchangeInitializedEventArgs(_hostKeyAlg));
        }

        protected void ProcessMsgNewKeys(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Reset byte counts for measured stream.
            _measuredStream.ResetCounts();

            // Dispose current algorithms.
            DisposeCurrentAlgorithms();

            // Start using new keys and algorithms.
            _encAlgCtoS = _newEncAlgCtoS;
            _encAlgStoC = _newEncAlgStoC;
            _compAlgCtoS = _newCompAlgCtoS;
            _compAlgStoC = _newCompAlgStoC;
            _macAlgCtoS = _newMacAlgCtoS;
            _macAlgStoC = _newMacAlgStoC;
            _languageCtoS = _newLanguageCtoS;
            _languageStoC = _newLanguageStoC;

            // Create encryptor and decryptor for algorithms.
            _cryptoTransformCtoS = _encAlgCtoS.Algorithm.CreateDecryptor();
            _cryptoTransformStoC = _encAlgStoC.Algorithm.CreateEncryptor();
        }

        protected void ConsiderKeyReExchange()
        {
            // Check if key re-exchange should be performed.
            if (this.KeyReExchangeByteLimit != -1 && _measuredStream.TotalBytesTransmitted
                >= this.KeyReExchangeByteLimit)
            {
                ReexchangeKeys();
                this.KeyReExchangeByteLimit = 0;
            }
        }

        protected void GenerateKeys(byte[] sharedSecret)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Set initialization vectors for encryption algorithms.
            _newEncAlgCtoS.Algorithm.IV = ComputeEncryptionKey(
                _newEncAlgCtoS.Algorithm.BlockSize / 8, sharedSecret, 'A');
            _newEncAlgStoC.Algorithm.IV = ComputeEncryptionKey(
                _newEncAlgStoC.Algorithm.BlockSize / 8, sharedSecret, 'B');

            // Set keys for encryption algorithms.
            _newEncAlgCtoS.Algorithm.Key = ComputeEncryptionKey(
                _newEncAlgCtoS.Algorithm.KeySize / 8, sharedSecret, 'C');
            _newEncAlgStoC.Algorithm.Key = ComputeEncryptionKey(
                _newEncAlgStoC.Algorithm.KeySize / 8, sharedSecret, 'D');

            // Set keys for MAC algorithms.
            _newMacAlgCtoS.Algorithm.Key = ComputeEncryptionKey(
                _newMacAlgCtoS.Algorithm.HashSize / 8, sharedSecret, 'E');
            _newMacAlgStoC.Algorithm.Key = ComputeEncryptionKey(
                _newMacAlgStoC.Algorithm.HashSize / 8, sharedSecret, 'F');
        }

        protected KexAlgorithm ChooseKexAlgorithm(string[] algorithmNames)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            KexAlgorithm chosenAlg = null;

            // Pick first algorithm in list that satisfies all conditions.
            KexAlgorithm alg;

            foreach (var algName in algorithmNames)
            {
                // Try to find supported algorithm with current name.
                alg = this.KexAlgorithms.SingleOrDefault(item => algName == item.Name);

                if (alg != null)
                {
                    // Set chosen algorithm.
                    chosenAlg = alg;
                    break;
                }
            }

            // Check that algorithm was found.
            if (chosenAlg == null)
            {
                Disconnect(SshDisconnectReason.KeyExchangeFailed,
                    "None of the listed key exchange algorithms are supported.");
                throw new DisconnectedException();
            }

            return (KexAlgorithm)chosenAlg.Clone();
        }

        protected PublicKeyAlgorithm ChooseHostKeyAlgorithm(string[] algorithmNames)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            PublicKeyAlgorithm chosenAlg = null;

            // Pick first algorithm in list that satisfies all conditions.
            PublicKeyAlgorithm alg;

            foreach (var algName in algorithmNames)
            {
                // Try to find supported algorithm with current name.
                alg = this.PublicKeyAlgorithms.SingleOrDefault(item => algName == item.Name);

                if (alg != null)
                {
                    // Set chosen algorithm.
                    chosenAlg = alg;
                    break;
                }
            }

            // Check that algorithm was found.
            if (chosenAlg == null)
            {
                Disconnect(SshDisconnectReason.KeyExchangeFailed,
                    "None of the listed server host key algorithms are supported.");
                throw new DisconnectedException();
            }

            return (PublicKeyAlgorithm)chosenAlg.Clone();
        }

        protected EncryptionAlgorithm ChooseEncryptionAlgorithm(string[] algorithmNames)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            EncryptionAlgorithm chosenAlg = null;

            // Pick first algorithm in list that satisfies all conditions.
            EncryptionAlgorithm alg;

            foreach (var algName in algorithmNames)
            {
                // Try to find supported algorithm with current name.
                alg = this.EncryptionAlgorithms.SingleOrDefault(item => algName == item.Name);

                if (alg != null)
                {
                    // Set chosen algorithm.
                    chosenAlg = alg;
                    break;
                }
            }

            // Check that algorithm was found.
            if (chosenAlg == null)
            {
                Disconnect(SshDisconnectReason.KeyExchangeFailed,
                    "None of the listed encryption algorithms are supported.");
                throw new DisconnectedException();
            }

            return (EncryptionAlgorithm)chosenAlg.Clone();
        }

        protected MacAlgorithm ChooseMacAlgorithm(string[] algorithmNames)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            MacAlgorithm chosenAlg = null;

            // Pick first algorithm in list that satisfies all conditions.
            MacAlgorithm alg;

            foreach (var algName in algorithmNames)
            {
                // Try to find supported algorithm with current name.
                alg = this.MacAlgorithms.SingleOrDefault(item => algName == item.Name);

                if (alg != null)
                {
                    // Set chosen algorithm.
                    chosenAlg = alg;
                    break;
                }
            }

            // Check that algorithm was found.
            if (chosenAlg == null)
            {
                Disconnect(SshDisconnectReason.KeyExchangeFailed,
                    "None of the listed MAC algorithms are supported.");
                throw new DisconnectedException();
            }

            return (MacAlgorithm)chosenAlg.Clone();
        }

        protected CompressionAlgorithm ChooseCompressionAlgorithm(string[] algorithmNames)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            CompressionAlgorithm chosenAlg = null;

            // Pick first algorithm in list that satisfies all conditions.
            CompressionAlgorithm alg;

            foreach (var algName in algorithmNames)
            {
                // Try to find supported algorithm with current name.
                alg = this.CompressionAlgorithms.SingleOrDefault(item => algName == item.Name);

                if (alg != null)
                {
                    // Set chosen algorithm.
                    chosenAlg = alg;
                    break;
                }
            }

            // Check that algorithm was found.
            if (chosenAlg == null)
            {
                Disconnect(SshDisconnectReason.KeyExchangeFailed,
                    "None of the listed compression algorithms are supported.");
                throw new DisconnectedException();
            }

            return (CompressionAlgorithm)chosenAlg.Clone();
        }

        protected string ChooseLanguage(string[] algorithmNames)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            string chosenAlg = null;

            // Pick first algorithm in list that satisfies all conditions.
            string alg;

            foreach (var algName in algorithmNames)
            {
                // Try to find supported algorithm with current name.
                alg = this.Languages.SingleOrDefault(item => algName == item);

                if (alg != null)
                {
                    // Set chosen algorithm.
                    chosenAlg = alg;
                    break;
                }
            }

            return chosenAlg;
        }

        protected byte[] ComputeEncryptionKey(int keySize, byte[] sharedSecret, char letter)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            byte[] keyBuffer = new byte[keySize];
            int keyBufferIndex = 0;
            byte[] currentHash = null;
            int currentHashLength;

            // Continue adding data to key until it reaches the desired size.
            while (keyBufferIndex < keySize)
            {
                using (var hashInputStream = new MemoryStream())
                {
                    using (var hashInputWriter = new SshStreamWriter(hashInputStream))
                    {
                        // Write input data to hash stream.
                        hashInputWriter.WriteMPint(sharedSecret);
                        hashInputWriter.Write(_exchangeHash);

                        if (currentHash == null)
                        {
                            hashInputWriter.Write((byte)letter);
                            hashInputWriter.Write(_sessionId);
                        }
                        else
                        {
                            hashInputWriter.Write(currentHash);
                        }
                    }

                    // Compute hash of input data.
                    currentHash = _kexAlg.ComputeHash(hashInputStream.ToArray());
                }

                // Copy current hash output to key buffer.
                currentHashLength = Math.Min(currentHash.Length, keySize - keyBufferIndex);
                Buffer.BlockCopy(currentHash, 0, keyBuffer, keyBufferIndex, currentHashLength);

                // Advance key buffer index.
                keyBufferIndex += currentHashLength;
            }

            return keyBuffer;
        }

        protected byte[] ComputeExchangeHash(byte[] hostKey, byte[] clientExchangeValue,
            byte[] serverExchangeValue, byte[] sharedSecret)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            using (var hashInputStream = new MemoryStream())
            {
                using (var hashInputWriter = new SshStreamWriter(hashInputStream))
                {
                    // Write kex parameters to stream.
                    hashInputWriter.Write(_clientIdString);
                    hashInputWriter.Write(_serverIdString);
                    hashInputWriter.WriteByteString(_clientKexInitPayload);
                    hashInputWriter.WriteByteString(_serverKexInitPayload);
                    hashInputWriter.WriteByteString(hostKey);
                    hashInputWriter.WriteMPint(clientExchangeValue);
                    hashInputWriter.WriteMPint(serverExchangeValue);
                    hashInputWriter.WriteMPint(sharedSecret);
                }

                // Return hash of input data.
                return _kexAlg.ComputeHash(hashInputStream.ToArray());
            }
        }

        protected byte[] ComputeMac(MacAlgorithm algorithm, uint packetSequenceNumber, uint packetLength,
            byte paddingLength, byte[] payload, byte[] padding)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            using (var macInputStream = new MemoryStream())
            {
                using (var macInputWriter = new SshStreamWriter(macInputStream))
                {
                    // Write sequence number to stream.
                    macInputWriter.Write(packetSequenceNumber);

                    // Write packet data to stream.
                    macInputWriter.Write(packetLength);
                    macInputWriter.Write(paddingLength);
                    macInputWriter.Write(payload);
                    macInputWriter.Write(padding);
                }

                // Return MAC data.
                return algorithm.ComputeHash(macInputStream.ToArray());
            }
        }

        //protected byte[] ComputeMac(MacAlgorithm algorithm, uint packetSequenceNumber,
        //    byte[] unencryptedPacket)
        //{
        //    // Create input data from packet sequence number and unencrypted packet data.
        //    byte[] inputData = new byte[unencryptedPacket.Length + 4];

        //    inputData[0] = (byte)((packetSequenceNumber & 0xFF000000) >> 24);
        //    inputData[1] = (byte)((packetSequenceNumber & 0x00FF0000) >> 16);
        //    inputData[2] = (byte)((packetSequenceNumber & 0x0000FF00) >> 8);
        //    inputData[3] = (byte)(packetSequenceNumber & 0x000000FF);

        //    Buffer.BlockCopy(unencryptedPacket, 0, inputData, 4, unencryptedPacket.Length);

        //    // Return MAC data.
        //    return algorithm.ComputeHash(inputData);
        //}

        protected void FatalErrorOccurred(Exception ex)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Disconnect with reason.
            Disconnect(SshDisconnectReason.ProtocolError, string.Format("Fatal error: {0}", ex.Message));

            OnError(new SshErrorEventArgs(new SshException("A fatal error has occurred.", ex)));
        }

        protected virtual void OnConnected(EventArgs e)
        {
            if (Connected != null) Connected(this, new EventArgs());
        }

        protected virtual void OnDisconnected(SshDisconnectedEventArgs e)
        {
            if (Disconnected != null) Disconnected(this, e);
        }

        protected virtual void OnError(SshErrorEventArgs e)
        {
            if (Error != null) Error(this, new EventArgs());
        }

        protected virtual void OnDebugMessageReceived(SshDebugMessageReceivedEventArgs e)
        {
            if (DebugMessageReceived != null) DebugMessageReceived(this, e);
        }

        protected virtual void OnKeyExchangeInitialized(SshKeyExchangeInitializedEventArgs e)
        {
            if (KeyExchangeInitialized != null) KeyExchangeInitialized(this, e);
        }

        private void ReceiveData()
        {
            try
            {
                // Create and send server identification string.
                _serverIdString = string.Format("SSH-{0}-{1}", _protocolVersion,
                    SshClient.SoftwareVersion) + (this.ServerComments == null ? "" : " "
                    + this.ServerComments);

                SendLine(_serverIdString);

                // Read identification string of client.
                _clientIdString = ReadLine();
                var clientIdParts = _clientIdString.Split(' ');
                var clientIdVersions = clientIdParts[0].Split(new char[] { '-' }, 3);

                this.ClientProtocolVersion = clientIdVersions[1];
                this.ClientSoftwareVersion = clientIdVersions[2];
                this.ClientComments = clientIdParts.Length > 1 ? clientIdParts[1] : null;

                // Verify that client protocol version is compatible.
                if (this.ClientProtocolVersion != "2.0")
                {
                    Disconnect(SshDisconnectReason.ProtocolVersionNotSupported,
                        "This server only supports SSH v2.0.");
                    throw new DisconnectedException();
                }

                // Send kex initialization message.
                SendMsgKexInit();

                // Read packets from network stream until connection is closed.
                byte[] packet = new byte[_maxPacketSize];

                while (true)
                {
                    // Read next packet.
                    ReadPacket();
                }
            }
            catch (DisconnectedException)
            {
                // Ignore.
            }
            catch (EndOfStreamException)
            {
                // Client disconnected.
                Disconnect(true);
            }
            catch (ThreadAbortException)
            {
                // Thread has been aborted.
            }
            //catch (Exception ex)
            //{
            //    // Let connection object handle exception.
            //    var exHandled = _connection.HandleException(this, ex);

            //    if (!exHandled)
            //        FatalErrorOccurred(ex);
            //}
            finally
            {
                if (!_isDisposed)
                {
                    // Disconnect from client.
                    Disconnect(false);
                }
            }
        }
    }

    public class SshDisconnectedEventArgs : EventArgs
    {
        public SshDisconnectedEventArgs(bool disconnectedRemotely, SshDisconnectReason reason,
            string description, string language)
        {
            this.DisconnectedRemotely = disconnectedRemotely;
            this.Reason = reason;
            this.Description = description;
            this.Language = language;
        }

        public SshDisconnectedEventArgs(bool disconnectedRemotely)
        {
            this.DisconnectedRemotely = disconnectedRemotely;
            this.Reason = SshDisconnectReason.None;
            this.Description = null;
            this.Language = null;
        }

        public bool DisconnectedRemotely
        {
            get;
            protected set;
        }

        public SshDisconnectReason Reason
        {
            get;
            protected set;
        }

        public string Description
        {
            get;
            protected set;
        }

        public string Language
        {
            get;
            protected set;
        }
    }

    public class SshErrorEventArgs : EventArgs
    {
        public SshErrorEventArgs(SshException exception)
        {
            this.Exception = exception;
        }

        public SshException Exception
        {
            get;
            protected set;
        }
    }

    public class SshDebugMessageReceivedEventArgs : EventArgs
    {
        public SshDebugMessageReceivedEventArgs(bool alwaysDisplay, string message, string language)
        {
            this.AlwaysDisplay = alwaysDisplay;
            this.Message = message;
            this.Language = language;
        }

        public bool AlwaysDisplay
        {
            get;
            protected set;
        }

        public string Message
        {
            get;
            protected set;
        }

        public string Language
        {
            get;
            protected set;
        }
    }

    public class SshKeyExchangeInitializedEventArgs : EventArgs
    {
        public SshKeyExchangeInitializedEventArgs(PublicKeyAlgorithm hostKeyAlgorithm)
        {
            this.HostKeyAlgorithm = hostKeyAlgorithm;
        }

        public PublicKeyAlgorithm HostKeyAlgorithm
        {
            get;
            protected set;
        }
    }

    public enum SshDisconnectReason : uint
    {
        None = 0, // Not used by protocol
        HostNotAllowedToConnect = 1,
        ProtocolError = 2,
        KeyExchangeFailed = 3,
        Reserved = 4,
        MacError = 5,
        CompressionError = 6,
        ServiceNotAvailable = 7,
        ProtocolVersionNotSupported = 8,
        HostKeyNotVerifiable = 9,
        ConnectionLost = 10,
        ByApplication = 11,
        TooManyConnections = 12,
        AuthCancelledByUser = 13,
        NoMoreAuthMethodsAvailable = 14,
        IllegalUserName = 15
    }

    internal enum SshMessage : byte
    {
        Disconnect = 1,
        Ignore = 2,
        Unimplemented = 3,
        Debug = 4,
        ServiceRequest = 5,
        ServiceAccept = 6,
        KexInit = 20,
        NewKeys = 21,
        KexDhInit = 30,
        KexDhReply = 31
    }
}
