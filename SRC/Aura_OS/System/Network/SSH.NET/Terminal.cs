using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace SshDotNet
{
    public abstract class Terminal
    {
        protected TerminalBitMode _bitMode; // Bit mode of terminal.

        public Terminal()
        {
            _bitMode = TerminalBitMode.Mode7Bit;
        }

        public TerminalBitMode BitMode
        {
            get;
            set;
        }

        public byte[] EscapeData(byte[] input)
        {
            return EscapeData(from b in input select new KeyData(b, false));
        }

        public abstract byte[] EscapeData(IEnumerable<KeyData> inputKeys);

        public KeyData[] UnescapeData(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            {
                return UnescapeData(inputStream);
            }
        }

        public abstract KeyData[] UnescapeData(Stream inputStream);
    }

    public struct KeyData
    {
        public byte Value;        // Value of key data.
        public bool IsVirtualKey; // True if data represents virtual key, false if data represents char.

        public KeyData(byte value, bool isVirtualKey)
        {
            this.Value = value;
            this.IsVirtualKey = isVirtualKey;
        }
    }

    public enum TerminalBitMode
    {
        Mode7Bit,
        Mode8Bit
    }
}
