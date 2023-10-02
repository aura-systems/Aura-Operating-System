using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SshDotNet
{
    public class SshAuthenticationService : SshService
    {
        public static AuthenticationMethod[] AllAuthMethods
        {
            get
            {
                return new[] {
                    AuthenticationMethod.PublicKey,
                    AuthenticationMethod.Password, 
                    AuthenticationMethod.HostBased,
                    AuthenticationMethod.KeyboardInteractive
                };
            }
        }

        protected string _lastUserName;       // Last user name used for auth.
        protected string _lastServiceName;    // Last service name used for auth.

        protected DateTime _startTime;        // Date/time at which service was started.
        protected Timer _authTimeoutTimer;    // Timer to detect when authentication has timed out.
        protected bool _bannerMsgSent;        // True if banner message has already been sent.
        protected IList<AuthenticationMethod>
            _authMethods;                     // List of allowed authentication methods (can change over time).
        protected int _failedAuthAttempts;    // Number of failed authentication attempts made so far.

        private bool _isDisposed = false;     // True if object has been disposed.

        public SshAuthenticationService(SshClient client)
            : base(client)
        {
            _bannerMsgSent = false;
            _authMethods = SshAuthenticationService.AllAuthMethods;
            _failedAuthAttempts = 0;

            // Initialize properties to default values.
            this.Timeout = new TimeSpan(0, 10, 0);
            this.MaximumAuthAttempts = 20;
            this.BannerMessage = null;
            this.BannerMessageLanguage = "";
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
                        if (_authTimeoutTimer != null)
                        {
                            _authTimeoutTimer.Dispose();
                            _authTimeoutTimer = null;
                        }
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

        public event EventHandler<AuthMethodRequestedEventArgs> AuthenticationMethodRequested;
        
        public event EventHandler<AuthUserNoMethodEventArgs> AuthenticateUserNoMethod;
        
        public event EventHandler<AuthUserPublicKeyEventArgs> AuthenticateUserPublicKey;
        
        public event EventHandler<AuthUserPasswordEventArgs> AuthenticateUserPassword;
        
        public event EventHandler<AuthUserHostBasedEventArgs> AuthenticateUserHostBased;

        public event EventHandler<AuthUserKeyboardInteractiveEventArgs> AuthenticateUserKeyboardInteractive;
        
        public event EventHandler<ChangePasswordEventArgs> ChangePassword;
        
        public event EventHandler<PromptInfoRequestedEventArgs> PromptInfoRequested;
        
        public event EventHandler<EventArgs> UserAuthenticated;

        public IList<AuthenticationMethod> AllowedAuthMethods
        {
            get { return _authMethods; }
            set { _authMethods = value; }
        }

        public TimeSpan Timeout
        {
            get;
            set;
        }

        public int MaximumAuthAttempts
        {
            get;
            set;
        }

        public string BannerMessage
        {
            get;
            set;
        }

        public string BannerMessageLanguage
        {
            get;
            set;
        }

        public override string Name
        {
            get { return "ssh-userauth"; }
        }

        internal override void Start()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Set time at which service was started.
            _startTime = DateTime.Now;

            // Create timer to detect timeout.
            _authTimeoutTimer = new Timer(new TimerCallback(AuthTimerCallback), null, 0, 1000);

            base.Start();
        }

        protected override void InternalStop()
        {
            base.InternalStop();
        }

        internal override bool ProcessMessage(byte[] payload)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Check if banner message has not yet been set.
            if (!_bannerMsgSent)
            {
                // Send banner message if one has been specified.
                if (this.BannerMessage != null) SendMsgUserAuthBanner(this.BannerMessage,
                    this.BannerMessageLanguage);

                _bannerMsgSent = true;
            }

            using (var msgReader = new SshStreamReader(new MemoryStream(payload)))
            {
                // Check message ID.
                SshAuthenticationMessage messageId = (SshAuthenticationMessage)msgReader.ReadByte();

#if DEBUG
                if (System.Enum.IsDefined(typeof(SshAuthenticationMessage), messageId))
                    Debug.WriteLine(string.Format(">>> {0}", System.Enum.GetName(
                        typeof(SshAuthenticationMessage), messageId)));
#endif

                switch (messageId)
                {
                    // User auth messages
                    case SshAuthenticationMessage.Request:
                        ProcessMsgUserAuthRequest(msgReader);
                        break;
                    case SshAuthenticationMessage.InfoResponse:
                        ProcessMsgUserInfoResponse(msgReader);
                        break;
                    // Unrecognised message
                    default:
                        return false;
                }
            }

            // Message was recognised.
            return true;
        }

        protected void AuthTimerCallback(object state)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Check if authentication has timed out.
            if (_authTimeoutTimer != null && (DateTime.Now - _startTime) >= this.Timeout)
            {
                // Stop timer.
                _authTimeoutTimer.Dispose();
                _authTimeoutTimer = null;

                // Authentication has timed out.
                _client.Disconnect(false);
            }
        }

        protected void SendMsgUserAuthSuccess()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshAuthenticationMessage.Success);

                _client.SendPacket<SshAuthenticationMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgUserAuthFailure(bool partialSuccess)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshAuthenticationMessage.Failure);
                msgWriter.WriteNameList(this.AllowedAuthMethods.GetSshNames());
                msgWriter.Write(partialSuccess);

                _client.SendPacket<SshAuthenticationMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgUserAuthBanner(string message, string language)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshAuthenticationMessage.Banner);
                msgWriter.WriteByteString(Encoding.UTF8.GetBytes(message));
                msgWriter.Write(language);

                _client.SendPacket<SshAuthenticationMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgUserAuthPkOk(string algName, byte[] keyBlob)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshAuthenticationMessage.PublicKeyOk);

                // Write public key information.
                msgWriter.Write(algName);
                msgWriter.WriteByteString(keyBlob);

                _client.SendPacket<SshAuthenticationMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgUserAuthPasswdChangeReq(string prompt, string language)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshAuthenticationMessage.PasswordChangeRequired);
                msgWriter.WriteByteString(Encoding.UTF8.GetBytes(prompt));
                msgWriter.Write(language);

                _client.SendPacket<SshAuthenticationMessage>(msgStream.ToArray());
            }
        }

        protected void SendMsgUserAuthInfoRequest(string name, string instruction, IList<AuthenticationPrompt> prompts)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Create message to send.
            using (var msgStream = new MemoryStream())
            using (var msgWriter = new SshStreamWriter(msgStream))
            {
                msgWriter.Write((byte)SshAuthenticationMessage.InfoRequest);
                msgWriter.WriteByteString(Encoding.UTF8.GetBytes(name));
                msgWriter.WriteByteString(Encoding.UTF8.GetBytes(instruction));
                msgWriter.Write(""); // language tag (deprecated)
                msgWriter.Write(prompts.Count);

                // Write info for each prompt.
                foreach (var prompt in prompts)
                {
                    msgWriter.WriteByteString(Encoding.UTF8.GetBytes(prompt.Prompt));
                    msgWriter.Write(prompt.Echo);
                }

                _client.SendPacket<SshAuthenticationMessage>(msgStream.ToArray());
            }
        }

        protected void ProcessMsgUserAuthRequest(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read auth information.
            string userName = Encoding.UTF8.GetString(msgReader.ReadByteString());
            string serviceName = msgReader.ReadString();
            string methodName = msgReader.ReadString();

            // Store user name and service name used for this auth.
            _lastUserName = userName;
            _lastServiceName = serviceName;

            // Check if service with specified name exists.
            if (_client.Services.Count(item => item.Name == serviceName) == 0)
            {
                // Service was not found.
                _client.Disconnect(SshDisconnectReason.ServiceNotAvailable, string.Format(
                    "The service with name {0} is not supported by this server."));
                throw new DisconnectedException();
            }

            // Check method of authentication.
            switch (methodName)
            {
                case "none":
                    ProcessMsgUserAuthRequestNone(msgReader);
                    break;
                case "publickey":
                    ProcessMsgUserAuthRequestPublicKey(msgReader);
                    break;
                case "password":
                    ProcessMsgUserAuthRequestPassword(msgReader);
                    break;
                case "hostbased":
                    ProcessMsgUserAuthRequestHostBased(msgReader);
                    break;
                case "keyboard-interactive":
                    ProcessMsgUserAuthRequestKeyboardInteractive(msgReader);
                    break;
                default:
                    // Invalid auth method.
                    _client.Disconnect(false);
                    break;
            }
        }

        protected void ProcessMsgUserAuthRequestNone(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Raise event to get result of auth attempt.
            var authUserEventArgs = new AuthUserNoMethodEventArgs(_lastUserName);

            if (AuthenticateUserNoMethod != null) AuthenticateUserNoMethod(this, authUserEventArgs);

            // Check result of auth attempt.
            switch (authUserEventArgs.Result)
            {
                case AuthenticationResult.Success:
                    // Auth has succeeded.
                    AuthenticateUser(_lastServiceName);

                    break;
                case AuthenticationResult.Failure:
                    // Send list of supported auth methods.
                    SendMsgUserAuthFailure(false);

                    break;
            }
        }

        protected void ProcessMsgUserAuthRequestPublicKey(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Raise event to specify requested auth method.
            if (AuthenticationMethodRequested != null) AuthenticationMethodRequested(this,
                new AuthMethodRequestedEventArgs(AuthenticationMethod.PublicKey));

            // Read request information.
            bool isAuthRequest = msgReader.ReadBoolean();
            string keyAlgName = msgReader.ReadString();
            byte[] keyAndCertsData = msgReader.ReadByteString();

            // Try to find public key algorithm.
            PublicKeyAlgorithm keyAlg = null;

            try
            {
                keyAlg = (PublicKeyAlgorithm)_client.PublicKeyAlgorithms.Single(item =>
                    item.Name == keyAlgName).Clone();
            }
            catch (InvalidOperationException)
            {
                // Public key algorithm is not supported.
                SendMsgUserAuthFailure(false);
            }

            // Load key and certificats data for algorithm.
            keyAlg.LoadKeyAndCertificatesData(keyAndCertsData);

            // Check if request is actual auth request or query of whether specified public key is
            // acceptable.
            if (isAuthRequest)
            {
                // Read client signature.
                var signatureData = msgReader.ReadByteString();
                var signature = keyAlg.GetSignature(signatureData);

                // Verify signature.
                var payloadData = ((MemoryStream)msgReader.BaseStream).ToArray();

                if (VerifyPublicKeySignature(keyAlg, payloadData, 0, payloadData.Length -
                   signatureData.Length - 4, signature))
                {
                    // Raise event to get result of auth attempt.
                    var authUserEventArgs = new AuthUserPublicKeyEventArgs(_lastUserName,
                        keyAlg.ExportPublicKey());

                    AuthenticateUserPublicKey(this, authUserEventArgs);

                    // Check result of auth attempt.
                    switch (authUserEventArgs.Result)
                    {
                        case AuthenticationResult.Success:
                            // Auth has succeeded.
                            AuthenticateUser(_lastServiceName);

                            break;
                        case AuthenticationResult.FurtherAuthRequired:
                            // Auth has succeeded, but further auth is required.
                            SendMsgUserAuthFailure(true);

                            break;
                        case AuthenticationResult.Failure:
                            // Auth has failed.
                            SendMsgUserAuthFailure(false);

                            break;
                    }
                }
                else
                {
                    // Signature is invalid.
                    SendMsgUserAuthFailure(false);
                }
            }
            else
            {
                // Public key is acceptable.
                SendMsgUserAuthPkOk(keyAlgName, keyAndCertsData);
            }
        }

        protected void ProcessMsgUserAuthRequestPassword(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Raise event to specify requested auth method.
            if (AuthenticationMethodRequested != null) AuthenticationMethodRequested(this,
                new AuthMethodRequestedEventArgs(AuthenticationMethod.Password));

            // Check whether client is changing password.
            bool changingPassword = msgReader.ReadBoolean();

            if (changingPassword)
            {
                // Read old and new passwords (in plaintext).
                string oldPassword = Encoding.UTF8.GetString(msgReader.ReadByteString());
                string newPassword = Encoding.UTF8.GetString(msgReader.ReadByteString());

                // Raise event to get result of password change request.
                var changePasswordEventArgs = new ChangePasswordEventArgs(oldPassword, newPassword);

                if (ChangePassword != null) ChangePassword(this, changePasswordEventArgs);

                // Check result of password change request.
                switch (changePasswordEventArgs.Result)
                {
                    case PasswordChangeResult.Success:
                        // Password change and auth have succeeded.
                        AuthenticateUser(_lastServiceName);

                        break;
                    case PasswordChangeResult.FurtherAuthRequired:
                        // Password change has succeeded, but further auth is required.
                        SendMsgUserAuthFailure(true);

                        break;
                    case PasswordChangeResult.Failure:
                        // Password change has failed.
                        SendMsgUserAuthFailure(false);

                        break;
                    case PasswordChangeResult.NewPasswordUnacceptable:
                        // Password was not changed.
                        SendMsgUserAuthPasswdChangeReq(changePasswordEventArgs.ReplyPrompt, "");

                        break;
                }
            }
            else
            {
                // Read password (in plaintext).
                string password = Encoding.UTF8.GetString(msgReader.ReadByteString());

                // Raise event to get result of auth attempt.
                var authUserEventArgs = new AuthUserPasswordEventArgs(_lastUserName, password);

                if (AuthenticateUserPassword != null) AuthenticateUserPassword(this, authUserEventArgs);

                // Check result of auth attempt.
                switch (authUserEventArgs.Result)
                {
                    case AuthenticationResult.Success:
                        // Auth has succeeded.
                        AuthenticateUser(_lastServiceName);

                        break;
                    case AuthenticationResult.FurtherAuthRequired:
                        // Auth has succeeded, but further auth is required.
                        SendMsgUserAuthFailure(true);

                        break;
                    case AuthenticationResult.Failure:
                        // Increment number of failed auth attempts.
                        _failedAuthAttempts++;

                        if (_failedAuthAttempts < this.MaximumAuthAttempts)
                        {
                            // Auth has failed, but allow client to reattempt auth.
                            SendMsgUserAuthFailure(false);
                        }
                        else
                        {
                            // Auth has failed too many times, disconnect.
                            _client.Disconnect(false);
                            throw new DisconnectedException();
                        }

                        break;
                    case AuthenticationResult.PasswordExpired:
                        // Password change is required.
                        SendMsgUserAuthPasswdChangeReq("The specified password has expired.", "");

                        break;
                }
            }
        }

        protected void ProcessMsgUserAuthRequestHostBased(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Raise event to specify requested auth method.
            if (AuthenticationMethodRequested != null) AuthenticationMethodRequested(this,
                new AuthMethodRequestedEventArgs(AuthenticationMethod.HostBased));

            // Read request information.
            string keyAlgName = msgReader.ReadString();
            byte[] keyAndCertsData = msgReader.ReadByteString();
            string clientHostName = msgReader.ReadString();
            string clientUserName = msgReader.ReadString();

            // Try to find public key algorithm.
            PublicKeyAlgorithm keyAlg = null;

            try
            {
                keyAlg = (PublicKeyAlgorithm)_client.PublicKeyAlgorithms.Single(item =>
                    item.Name == keyAlgName).Clone();
            }
            catch (InvalidOperationException)
            {
                // Public key algorithm is not supported.
                SendMsgUserAuthFailure(false);
            }

            // Load key and certificats data for algorithm.
            keyAlg.LoadKeyAndCertificatesData(keyAndCertsData);

            // Read client signature.
            var signatureData = msgReader.ReadByteString();
            var signature = keyAlg.GetSignature(signatureData);

            // Verify signature.
            var payloadData = ((MemoryStream)msgReader.BaseStream).ToArray();

            if (VerifyPublicKeySignature(keyAlg, payloadData, 0, payloadData.Length -
                signatureData.Length - 4, signature))
            {
                // Raise event to get result of auth attempt.
                var authUserEventArgs = new AuthUserHostBasedEventArgs(_lastUserName, clientHostName,
                    clientUserName, keyAlg.ExportPublicKey());

                if (AuthenticateUserHostBased != null) AuthenticateUserHostBased(this, authUserEventArgs);

                // Check result of auth attempt.
                switch (authUserEventArgs.Result)
                {
                    case AuthenticationResult.Success:
                        // Auth has succeeded.
                        AuthenticateUser(_lastServiceName);

                        break;
                    case AuthenticationResult.FurtherAuthRequired:
                        // Auth has succeeded, but further auth is required.
                        SendMsgUserAuthFailure(true);

                        break;
                    case AuthenticationResult.Failure:
                        // Auth has failed.
                        SendMsgUserAuthFailure(false);

                        break;
                }
            }
            else
            {
                // Signature is invalid.
                SendMsgUserAuthFailure(false);
            }
        }

        protected void ProcessMsgUserAuthRequestKeyboardInteractive(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Raise event to specify requested auth method.
            if (AuthenticationMethodRequested != null) AuthenticationMethodRequested(this,
                new AuthMethodRequestedEventArgs(AuthenticationMethod.KeyboardInteractive));

            // Read request information.
            string language = msgReader.ReadString();
            string[] subMethods = Encoding.UTF8.GetString(msgReader.ReadByteString()).Split(
                new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            // Request prompt info from client.
            RequestPromptInfo(subMethods);
        }

        protected void ProcessMsgUserInfoResponse(SshStreamReader msgReader)
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);

            // Read response info.
            int numResponses = msgReader.ReadInt32();
            string[] responses = new string[numResponses];

            for (int i = 0; i < numResponses; i++)
                responses[i] = Encoding.UTF8.GetString(msgReader.ReadByteString());

            // Raise event to get result of auth attempt.
            var authUserEventArgs = new AuthUserKeyboardInteractiveEventArgs(_lastUserName,
                responses);

            if (AuthenticateUserKeyboardInteractive != null) AuthenticateUserKeyboardInteractive(this,
                authUserEventArgs);

            // Check result of auth attempt.
            switch (authUserEventArgs.Result)
            {
                case AuthenticationResult.Success:
                    // Auth has succeeded.
                    AuthenticateUser(_lastServiceName);

                    break;
                case AuthenticationResult.FurtherAuthRequired:
                    // Auth has succeeded, but further auth is required.
                    SendMsgUserAuthFailure(true);

                    break;
                case AuthenticationResult.Failure:
                    // Auth has failed.
                    SendMsgUserAuthFailure(false);

                    break;
                case AuthenticationResult.RequestMoreInfo:
                    // Request more prompt info from client.
                    RequestPromptInfo(null);

                    break;
            }
        }

        protected void RequestPromptInfo(string[] subMethods)
        {
            // Raise event to get prompt info.
            var reqPromptInfoEventArgs = new PromptInfoRequestedEventArgs(subMethods);

            if (PromptInfoRequested != null) PromptInfoRequested(this, reqPromptInfoEventArgs);

            if (reqPromptInfoEventArgs.NoAuthRequired)
            {
                // Auth has succeeded.
                AuthenticateUser(_lastUserName);
            }
            if (reqPromptInfoEventArgs.Prompts != null)
            {
                // Send Info Request message.
                SendMsgUserAuthInfoRequest(reqPromptInfoEventArgs.Name, reqPromptInfoEventArgs.Instruction,
                    reqPromptInfoEventArgs.Prompts);
            }
            else
            {
                // No prompts were provided.
                SendMsgUserAuthFailure(false);
            }
        }

        protected void AuthenticateUser(string requestedService)
        {
            // Tell client that auth has succeeded.
            SendMsgUserAuthSuccess();

            // Start requested service.
            _client.StartService(requestedService);

            // Raise event.
            if (UserAuthenticated != null) UserAuthenticated(this, new EventArgs());
        }

        protected bool VerifyPublicKeySignature(PublicKeyAlgorithm alg, byte[] payload, int payloadOffset,
            int payloadCount, byte[] signature)
        {
            using (var hashInputStream = new MemoryStream())
            using (var hashInputWriter = new SshStreamWriter(hashInputStream))
            {
                // Write input data.
                hashInputWriter.WriteByteString(_client.SessionId);
                hashInputWriter.Write(payload, payloadOffset, payloadCount);

                // Verify signature.
                return alg.VerifyData(hashInputStream.ToArray(), signature);
            }
        }
    }

    public static class SshAuthenticationServiceExtensions
    {
        public static string[] GetSshNames(this IEnumerable<AuthenticationMethod> methods)
        {
            return (from m in methods select m.GetSshName()).ToArray();
        }

        public static string GetSshName(this AuthenticationMethod method)
        {
            switch (method)
            {
                case AuthenticationMethod.PublicKey:
                    return "publickey";
                case AuthenticationMethod.Password:
                    return "password";
                case AuthenticationMethod.HostBased:
                    return "hostbased";
                case AuthenticationMethod.KeyboardInteractive:
                    return "keyboard-interactive";
                case AuthenticationMethod.None:
                    return "none";
            }

            return "";
        }
    }

    #region Event Arguments Types
    public class AuthMethodRequestedEventArgs : EventArgs
    {
        public AuthMethodRequestedEventArgs(AuthenticationMethod authMethod)
            : base()
        {
            this.AuthMethod = authMethod;
        }

        public AuthenticationMethod AuthMethod
        {
            get;
            protected set;
        }
    }

    public class AuthUserNoMethodEventArgs : AuthUserEventArgs
    {
        public AuthUserNoMethodEventArgs(string userName)
            : base(userName)
        {
        }

        public override AuthenticationMethod AuthMethod
        {
            get { return AuthenticationMethod.None; }
        }
    }

    public class AuthUserPublicKeyEventArgs : AuthUserEventArgs
    {
        public AuthUserPublicKeyEventArgs(string userName, SshPublicKey publicKey)
            : base(userName)
        {
            this.PublicKey = publicKey;
            this.Result = AuthenticationResult.Failure;
        }

        public SshPublicKey PublicKey
        {
            get;
            protected set;
        }

        public override AuthenticationMethod AuthMethod
        {
            get { return AuthenticationMethod.PublicKey; }
        }
    }

    public class AuthUserPasswordEventArgs : AuthUserEventArgs
    {
        public AuthUserPasswordEventArgs(string userName, string password)
            : base(userName)
        {
            this.Password = password;
            this.Result = AuthenticationResult.Failure;
        }

        public string Password
        {
            get;
            protected set;
        }

        public override AuthenticationMethod AuthMethod
        {
            get { return AuthenticationMethod.Password; }
        }
    }

    public class AuthUserHostBasedEventArgs : AuthUserEventArgs
    {
        public AuthUserHostBasedEventArgs(string userName, string clientHostName,
            string clientUserName, SshPublicKey publicKey)
            : base(userName)
        {
            this.ClientHostName = clientHostName;
            this.ClientUserName = clientUserName;
            this.PublicKey = publicKey;
        }

        public string ClientHostName
        {
            get;
            protected set;
        }

        public string ClientUserName
        {
            get;
            protected set;
        }

        public SshPublicKey PublicKey
        {
            get;
            protected set;
        }

        public override AuthenticationMethod AuthMethod
        {
            get { return AuthenticationMethod.HostBased; }
        }
    }

    public class AuthUserKeyboardInteractiveEventArgs : AuthUserEventArgs
    {
        public AuthUserKeyboardInteractiveEventArgs(string userName, string[] responses)
            : base(userName)
        {
            this.Responses = responses;
        }

        public string[] Responses
        {
            get;
            protected set;
        }

        public override AuthenticationMethod AuthMethod
        {
            get { return AuthenticationMethod.KeyboardInteractive; }
        }
    }

    public abstract class AuthUserEventArgs : EventArgs
    {
        public AuthUserEventArgs(string userName)
        {
            this.UserName = userName;
            this.Result = AuthenticationResult.Failure;
        }

        public AuthenticationResult Result
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            protected set;
        }

        public abstract AuthenticationMethod AuthMethod
        {
            get;
        }
    }

    public class ChangePasswordEventArgs : EventArgs
    {
        public ChangePasswordEventArgs(string oldPassword, string newPassword)
        {
            this.OldPassword = oldPassword;
            this.NewPassword = newPassword;
            this.Result = PasswordChangeResult.Failure;
            this.ReplyPrompt = null;
        }

        public string OldPassword
        {
            get;
            protected set;
        }

        public string NewPassword
        {
            get;
            protected set;
        }

        public PasswordChangeResult Result
        {
            get;
            set;
        }

        public string ReplyPrompt
        {
            get;
            set;
        }
    }

    public class PromptInfoRequestedEventArgs : EventArgs
    {
        public PromptInfoRequestedEventArgs(string[] subMethods)
        {
            this.SubMethods = subMethods;
            this.Name = null;
            this.Instruction = null;
            this.Prompts = null;
            this.NoAuthRequired = false;
        }

        public bool NoAuthRequired
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Instruction
        {
            get;
            set;
        }

        public IList<AuthenticationPrompt> Prompts
        {
            get;
            set;
        }

        public string[] SubMethods
        {
            get;
            protected set;
        }
    }
    #endregion

    public struct AuthenticationPrompt
    {
        public string Prompt;
        public bool Echo;

        public AuthenticationPrompt(string prompt, bool echo)
        {
            this.Prompt = prompt;
            this.Echo = echo;
        }
    }

    public enum AuthenticationResult
    {
        Success,
        FurtherAuthRequired,
        Failure,
        PasswordExpired,
        RequestMoreInfo,
    }

    public enum PasswordChangeResult
    {
        Success,
        FurtherAuthRequired,
        Failure,
        NewPasswordUnacceptable
    }

    public enum AuthenticationMethod
    {
        PublicKey,
        Password,
        HostBased,
        KeyboardInteractive,
        None
    }

    internal enum SshAuthenticationMessage : byte
    {
        Request = 50,
        Failure = 51,
        Success = 52,
        Banner = 53,
        PublicKeyOk = 60,
        PasswordChangeRequired = 60,
        InfoRequest = 60,
        InfoResponse = 61
    }
}
