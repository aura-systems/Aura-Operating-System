/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Info / OK / Error in console
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System
{
    public class CustomConsole
    {
        public static System.Graphics.UI.GUI.Components.Console BootConsole;

        public static void WriteLineInfo(string text)
        {
            if (BootConsole != null)
            {
                BootConsole.Foreground = ConsoleColor.Cyan;
                BootConsole.Write("[Info] ");
                BootConsole.Foreground = ConsoleColor.White;
                BootConsole.Write(text + "\n");
                BootConsole.Draw();
                Kernel.Canvas.Display();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[Info] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text + "\n");
            }
        }

        public static void WriteLineWarning(string text)
        {
            if (BootConsole != null)
            {
                BootConsole.Foreground = ConsoleColor.Yellow;
                BootConsole.Write("[WARNING] ");
                BootConsole.Foreground = ConsoleColor.White;
                BootConsole.Write(text + "\n");
                BootConsole.Draw();
                Kernel.Canvas.Display();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[WARNING] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text + "\n");
            }
        }

        public static void WriteLineOK(string text)
        {
            if (BootConsole != null)
            {
                BootConsole.Foreground = ConsoleColor.Green;
                BootConsole.Write("[OK] ");
                BootConsole.Foreground = ConsoleColor.White;
                BootConsole.Write(text + "\n");
                BootConsole.Draw();
                Kernel.Canvas.Display();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[OK] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text + "\n");
            }
        }

        public static void WriteLineError(string text)
        {
            if (BootConsole != null)
            {
                BootConsole.Foreground = ConsoleColor.DarkRed;
                BootConsole.Write("[Error] ");
                BootConsole.Foreground = ConsoleColor.White;
                BootConsole.Write(text + "\n");
                BootConsole.Draw();
                Kernel.Canvas.Display();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("[Error] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text + "\n");
            } 
        }
    }
}