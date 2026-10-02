/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TextWriter that sends Console output to the serial log
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Cosmos.Kernel.System.Diagnostics;

namespace Aura_OS.System.Utils
{
    /// <summary>
    /// Installed as Console.Out/Error (Kernel.GuiSink) once the GUI owns the canvas: the gen3 KernelConsole
    /// draws on the same canvas, so un-redirected Console output would paint over the desktop.
    /// Everything goes to the serial log. Never throws (an exception escaping a writer installed with
    /// Console.SetOut would leave the SyncTextWriter monitor held).
    /// </summary>
    public sealed class SerialTextWriter : TextWriter
    {
        public override global::System.Text.Encoding Encoding => global::System.Text.Encoding.UTF8;

        public override void Write(char value)
        {
            try
            {
                Log.WriteString(value.ToString());
            }
            catch (Exception)
            {
            }
        }

        public override void Write(string value)
        {
            if (value == null)
            {
                return;
            }

            try
            {
                Log.WriteString(value);
            }
            catch (Exception)
            {
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null || index < 0 || count <= 0 || index + count > buffer.Length)
            {
                return;
            }

            try
            {
                Log.WriteString(new string(buffer, index, count));
            }
            catch (Exception)
            {
            }
        }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            if (buffer.IsEmpty)
            {
                return;
            }

            try
            {
                Log.WriteString(new string(buffer));
            }
            catch (Exception)
            {
            }
        }

        public override void WriteLine(string value)
        {
            try
            {
                if (value != null)
                {
                    Log.WriteString(value);
                }

                Log.WriteString("\n");
            }
            catch (Exception)
            {
            }
        }

        public override void Flush()
        {
        }
    }
}
