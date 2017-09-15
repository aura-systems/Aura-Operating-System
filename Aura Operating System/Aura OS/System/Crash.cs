/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Crash screen
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Sys = Cosmos.System;

namespace Alve_OS.System
{
    class Crash
    {

        /// <summary>
        /// Stop the kernel and display exception
        /// </summary>
        /// <param name="ex">Exception that stop the kernel</param>
        public static void StopKernel(Exception ex)
        {
            Console.Clear();

            string ex_message = ex.Message;
            string inner_message = "<none>";

            if (ex.InnerException != null)
                inner_message = ex.InnerException.Message;

            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.BackgroundColor = ConsoleColor.Red;

            Console.SetCursorPosition(8, 8);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 9);
            Console.WriteLine("║ AN FATAL ERROR occured in Alve Operating System              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 10);
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 11);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║ Login:                                                       ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 13);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 14);
            Console.WriteLine("║ Password:                                                    ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 15);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 16);
            Console.WriteLine("╚══════════════════Press any key to reboot.════════════════════╝");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║ Exception message : " + ex_message);

            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 13);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 14);
            Console.WriteLine("║ Error : " + inner_message);

            Console.SetCursorPosition(x, y);

            Console.ReadKey();
            Sys.Power.Reboot();
        }

    }
}
