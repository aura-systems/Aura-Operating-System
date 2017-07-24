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
        public static void StopKernel(Exception ex)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.WriteLine("SYSTEM ERROR :(");

            string ex_message = ex.Message;
            string inner_message = "<none>";

            if (ex.InnerException != null)
                inner_message = ex.InnerException.Message;
            Console.WriteLine($@"Exception message : {ex_message}
                                 Error : {inner_message}");
            Console.WriteLine("Press any key to reboot.");
            Console.ReadKey();
            Sys.Power.Reboot();
        }
    }
}
