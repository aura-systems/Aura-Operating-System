namespace System
{
    public class Exception
    {
        internal string _message;
        private readonly Exception? _innerException;
        public virtual string Message
        {
            get
            {
                return _message ?? "An exception was thrown";
            }
        }

    
        public Exception()
        {
            _message = "<no message>";
        }
        public Exception(string? message)
        {
            _message = message;
        }

        public Exception(string? message, Exception? innerException)
          : this()
        {
            _message = message;
            _innerException = innerException;
        }
    }
}
