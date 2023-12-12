/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Console Plug.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS;
using IL2CPU.API.Attribs;
using System;
using System.Text;

namespace Aura_Plugs
{
    [Plug(Target = typeof(global::System.Console))]
    public class ConsoleImpl
    {
        private static Encoding ConsoleOutputEncoding = Encoding.ASCII;

        #region Write

        public static void Write(bool aBool)
        {
            Write(aBool.ToString());
        }

        /*
         * A .Net character can be effectevily more can one byte so calling the low level Console.Write() will be wrong as
         * it accepts only bytes, we need to convert it using the specified OutputEncoding but to do this we have to convert
         * it ToString first
         */
        public static void Write(char aChar) => Write(aChar.ToString());

        public static void Write(char[] aBuffer) => Write(aBuffer, 0, aBuffer.Length);

        /* Decimal type is not working yet... */
        //public static void Write(decimal aDecimal) => Write(aDecimal.ToString());

        public static void Write(double aDouble) => Write(aDouble.ToString());

        public static void Write(float aFloat) => Write(aFloat.ToString());

        public static void Write(int aInt) => Write(aInt.ToString());

        public static void Write(long aLong) => Write(aLong.ToString());

        /* Correct behaviour printing null should not throw NRE or do nothing but should print an empty string */
        public static void Write(object value) => Write((value ?? String.Empty));

        public static void Write(string aText)
        {
            if (Kernel.console != null)
            {
                Kernel.console.Write(aText);
            }
            else if (Kernel.aConsole != null)
            {
                byte[] aTextEncoded = ConsoleOutputEncoding.GetBytes(aText);
                Kernel.aConsole.Write(aTextEncoded);
            }
        }

        public static void Write(uint aInt) => Write(aInt.ToString());

        public static void Write(ulong aLong) => Write(aLong.ToString());

        public static void Write(string format, object arg0) => Write(String.Format(format, arg0));

        public static void Write(string format, object arg0, object arg1) => Write(String.Format(format, arg0, arg1));

        public static void Write(string format, object arg0, object arg1, object arg2) => Write(String.Format(format, arg0, arg1, arg2));

        public static void Write(string format, params object[] arg) => Write(String.Format(format, arg));

        public static void Write(char[] aBuffer, int aIndex, int aCount)
        {
            if (aBuffer == null)
            {
                throw new ArgumentNullException("aBuffer");
            }
            if (aIndex < 0)
            {
                throw new ArgumentOutOfRangeException("aIndex");
            }
            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException("aCount");
            }
            if ((aBuffer.Length - aIndex) < aCount)
            {
                throw new ArgumentException();
            }
            for (int i = 0; i < aCount; i++)
            {
                Write(aBuffer[aIndex + i]);
            }
        }

        //You'd expect this to be on System.Console wouldn't you? Well, it ain't so we just rely on Write(object value)
        //public static void Write(byte aByte) {
        //    Write(aByte.ToString());
        //}

        #endregion

        #region WriteLine

        public static void WriteLine() => Write(Environment.NewLine);

        public static void WriteLine(bool aBool) => WriteLine(aBool.ToString());

        public static void WriteLine(char aChar) => WriteLine(aChar.ToString());

        public static void WriteLine(char[] aBuffer) => WriteLine(new String(aBuffer));

        /* Decimal type is not working yet... */
        //public static void WriteLine(decimal aDecimal) => WriteLine(aDecimal.ToString());

        public static void WriteLine(double aDouble) => WriteLine(aDouble.ToString());

        public static void WriteLine(float aFloat) => WriteLine(aFloat.ToString());

        public static void WriteLine(int aInt) => WriteLine(aInt.ToString());

        public static void WriteLine(long aLong) => WriteLine(aLong.ToString());

        /* Correct behaviour printing null should not throw NRE or do nothing but should print an empty line */
        public static void WriteLine(object value) => Write((value ?? String.Empty) + Environment.NewLine);

        public static void WriteLine(string aText) => Write(aText + Environment.NewLine);

        public static void WriteLine(uint aInt) => WriteLine(aInt.ToString());

        public static void WriteLine(ulong aLong) => WriteLine(aLong.ToString());

        public static void WriteLine(string format, object arg0) => WriteLine(String.Format(format, arg0));

        public static void WriteLine(string format, object arg0, object arg1) => WriteLine(String.Format(format, arg0, arg1));

        public static void WriteLine(string format, object arg0, object arg1, object arg2) => WriteLine(String.Format(format, arg0, arg1, arg2));

        public static void WriteLine(string format, params object[] arg) => WriteLine(String.Format(format, arg));

        public static void WriteLine(char[] aBuffer, int aIndex, int aCount)
        {
            Write(aBuffer, aIndex, aCount);
            WriteLine();
        }

        #endregion

        #region ConsoleColors

        public static ConsoleColor get_ForegroundColor()
        {
            if (Kernel.console != null)
            {
                return Kernel.console.Foreground;
            }
            else if (Kernel.aConsole != null)
            {
                return Kernel.aConsole.Foreground;
            }

            return ConsoleColor.White;
        }

        public static void set_ForegroundColor(ConsoleColor value)
        {
            if (Kernel.console != null)
            {
                Kernel.console.Foreground = value;
            }
            else if (Kernel.aConsole != null)
            {
                Kernel.aConsole.Foreground = value;
            }
        }

        public static void Beep()
        {
            Cosmos.System.PCSpeaker.Beep();
        }

        //TODO: Console uses TextWriter - intercept and plug it instead
        public static void Clear()
        {
            if (Kernel.console != null)
            {
                Kernel.console.Clear();
            }
            else if (Kernel.aConsole != null)
            {
                Kernel.aConsole.Clear();
            }
        }

        #endregion
    }
}