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
        public static void WriteLineInfo(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[Info] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
            Kernel.debugger.Send("[Info] " + text);
        }
        public static void WriteLineWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[WARNING] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
            Kernel.debugger.Send("[WARNING] " + text);
        }
        /// <summary>
        /// tipColor's text
        /// [tipText(with tipColor)] text
        /// </summary>
        /// <param name="tipColor"></param>
        /// <param name="text"></param>
        /// <param name="tipText"></param>
        public static void WriteLineCustom(ConsoleColor tipColor,string text,string tipText)
        {
            Console.ForegroundColor = tipColor;
            Console.Write("[" + tipText + "] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
            Kernel.debugger.Send("[" + tipText + "] " + text);
        }
        public static void WriteLineOK(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[OK] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
            Kernel.debugger.Send("[OK] " + text);
        }
        public static void WriteLineError(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("[Error] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
            Kernel.debugger.Send("[Error] " + text);
        }
    }
}
