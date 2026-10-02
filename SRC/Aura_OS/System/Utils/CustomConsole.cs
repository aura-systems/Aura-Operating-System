/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Info / OK / Error in console
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System
{
    public class CustomConsole
    {
        public static System.Graphics.UI.GUI.Components.Console BootConsole;

        // Every line goes to Logs first, which mirrors it to the serial log (before any drawing,
        // so the message survives a fault while drawing).

        public static void WriteLineInfo(string text)
        {
            Logs.DoOSLog("[Info] " + text);

            if (BootConsole != null)
            {
                BootConsole.Foreground = ConsoleColor.Cyan;
                BootConsole.Write("[Info] ");
                BootConsole.Foreground = ConsoleColor.White;
                BootConsole.Write(text + "\n");
                BootConsole.Draw();
                Kernel.Canvas.DrawImage(BootConsole.GetBuffer(), 0, 0);
                Kernel.Canvas.Display();
            }
            else
            {
                WriteConsole(ConsoleColor.Cyan, "[Info] ", text);
            }
        }

        public static void WriteLineWarning(string text)
        {
            Logs.DoOSLog("[WARNING] " + text);

            if (BootConsole != null)
            {
                BootConsole.Foreground = ConsoleColor.Yellow;
                BootConsole.Write("[WARNING] ");
                BootConsole.Foreground = ConsoleColor.White;
                BootConsole.Write(text + "\n");
                BootConsole.Draw();
                Kernel.Canvas.DrawImage(BootConsole.GetBuffer(), 0, 0);
                Kernel.Canvas.Display();
            }
            else
            {
                WriteConsole(ConsoleColor.Yellow, "[WARNING] ", text);
            }
        }

        public static void WriteLineOK(string text)
        {
            Logs.DoOSLog("[OK] " + text);

            if (BootConsole != null)
            {
                BootConsole.Foreground = ConsoleColor.Green;
                BootConsole.Write("[OK] ");
                BootConsole.Foreground = ConsoleColor.White;
                BootConsole.Write(text + "\n");
                BootConsole.Draw();
                Kernel.Canvas.DrawImage(BootConsole.GetBuffer(), 0, 0);
                Kernel.Canvas.Display();
            }
            else
            {
                WriteConsole(ConsoleColor.Green, "[OK] ", text);
            }
        }

        public static void WriteLineError(string text)
        {
            Logs.DoOSLog("[Error] " + text);

            if (BootConsole != null)
            {
                BootConsole.Foreground = ConsoleColor.DarkRed;
                BootConsole.Write("[Error] ");
                BootConsole.Foreground = ConsoleColor.White;
                BootConsole.Write(text + "\n");
                BootConsole.Draw();
                Kernel.Canvas.DrawImage(BootConsole.GetBuffer(), 0, 0);
                Kernel.Canvas.Display();
            }
            else
            {
                WriteConsole(ConsoleColor.DarkRed, "[Error] ", text);
            }
        }

        /// <summary>
        /// Before BootConsole exists: the gen3 KernelConsole (boot text on the framebuffer), or Kernel.GuiSink
        /// once Console.Out is redirected. The plugged Console members throw when the KernelConsole is not
        /// initialized (no display): the line is already on serial, so ignore it.
        /// </summary>
        private static void WriteConsole(ConsoleColor color, string prefix, string text)
        {
            try
            {
                Console.ForegroundColor = color;
                Console.Write(prefix);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text + "\n");
            }
            catch (Exception)
            {
            }
        }
    }
}
