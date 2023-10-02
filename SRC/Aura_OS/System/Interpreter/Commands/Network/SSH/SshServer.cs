using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura_OS.System.Interpreter.Commands.Network.SSH;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using SshDotNet;

namespace Aura_OS.System.Interpreter.Commands.Network.SSH
{
    public sealed class SshTcpServer : IDisposable
    {
        private TcpListener _tcpListener; // Listens for TCP connections from clients.
        private List<SshClient> _clients; // List of connected clients.

        private object _listenerLock;     // Lock for TCP listener.

        private bool _isDisposed = false; // True if object has been disposed.

        public SshTcpServer()
        {
            _listenerLock = new object();

            _clients = new List<SshClient>();
        }

        ~SshTcpServer()
        {
            Dispose(false);
        }

        public event EventHandler<ClientEventArgs> ClientConnected;

        public event EventHandler<ClientEventArgs> ClientDisconnected;

        public List<SshClient> Clients
        {
            get
            {
                if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

                return _clients;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    Stop();
                    CloseAllConnections();

                    _clients = null;
                }

                // Dispose unmanaged resources.
            }

            _isDisposed = true;
        }

        public bool IsRunning
        {
            get
            {
                if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

                //lock (_listenerLock)
                {
                    return (_tcpListener != null);
                }
            }
        }

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public void Start()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            lock (_listenerLock)
            {
                Start(new EndPoint(Address.Zero, 22));

                // Begin accepting first incoming connected attempt.
                var client = _tcpListener.AcceptTcpClient();

                Console.WriteLine("SSH - Client connected!");

                var endpoint = new EndPoint(Address.Zero, 0);
                var data = client.Receive(ref endpoint);

                Console.WriteLine("SSH - Data received!");
            }
        }

        public void Start(EndPoint localEP)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            lock (_listenerLock)
            {
                try
                {
                    // Create TCP listener and start it.
                    _tcpListener = new TcpListener(localEP.Port);
                    _tcpListener.Start();
                }
                catch (Exception exSocket)
                {
                    if (_tcpListener != null) _tcpListener.Stop();
                    _tcpListener = null;

                    // Add local end point to exception data.
                    //if (exSocket.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    //    exSocket.Data.Add("localEndPoint", localEP);

                    throw exSocket;
                }
            }
        }

        public void Stop()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            lock (_listenerLock)
            {
                if (_tcpListener != null)
                {
                    // Stop TCP listener.
                    _tcpListener.Stop();
                    _tcpListener = null;
                }
            }
        }

        public void CloseAllConnections()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Disconnect each client.
            foreach (var client in _clients)
            {
                client.Connected -= client_Connected;
                client.Disconnected -= client_Disconnected;

                client.Dispose();
            }

            // Clear list of clients.
            _clients.Clear();
        }

        private void AcceptTcpClient(IAsyncResult ar)
        {
            if (_isDisposed) return;

            // Check that operation has completed.
            if (!ar.IsCompleted) return;

            //lock (_listenerLock)
            {
                // Check that operation used current TCP listener.
                if (ar.AsyncState != _tcpListener || _tcpListener == null) return;

                TcpClient tcpClient;

                /*

                try
                {
                    // Accept incoming connection attempt.
                    tcpClient = _tcpListener.EndAcceptTcpClient(ar);
                }
                catch (SocketException exSocket)
                {
                    if (exSocket.SocketErrorCode == SocketError.ConnectionReset) return;

                    throw exSocket;
                }

                // Begin accepting next incoming connected attempt.
                _tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClient), _tcpListener);

                // Add new client to list.
                var sshClient = new SshConsoleClient(new TcpConnection(tcpClient));

                sshClient.Connected += client_Connected;
                sshClient.Disconnected += client_Disconnected;
                sshClient.ConnectionEstablished();

                _clients.Add(sshClient);

                */
            }
        }

        private void client_Connected(object sender, EventArgs e)
        {
            var client = (SshClient)sender;

            // Raise event.
            if (ClientConnected != null) ClientConnected(this, new ClientEventArgs(client));
        }

        private void client_Disconnected(object sender, EventArgs e)
        {
            var client = (SshClient)sender;

            client.Dispose();

            // Remove client from list.
            _clients.Remove(client);

            // Raise event.
            if (ClientConnected != null) ClientDisconnected(this, new ClientEventArgs(client));
        }
    }

    public class ClientEventArgs : EventArgs
    {
        public ClientEventArgs(SshClient client)
        {
            this.Client = client;
        }

        public SshClient Client
        {
            get;
            protected set;
        }
    }
}
