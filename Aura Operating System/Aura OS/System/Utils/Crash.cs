/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Crash screen
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Sys = Cosmos.System;

namespace Aura_OS.System
{
    class Crash
    {

        /// <summary>
        /// Stop the kernel and display exception
        /// </summary>
        /// <param name="ex">Exception that stop the kernel</param>
        public static void StopKernel(Exception ex)
        {
            Kernel.running = false;

            string ex_message = ex.Message;
            string inner_message = "";

            Console.BackgroundColor = ConsoleColor.Red;

            Console.Clear();

            Console.WriteLine("An error occured in Aura Operating System:");
            Console.WriteLine(ex);
            if (ex.InnerException != null)
            {
                inner_message = ex.InnerException.Message;
                Console.WriteLine(inner_message);
            }
            Console.WriteLine("Aura Version: " + Kernel.version);
            Console.WriteLine("Aura Revision: " + Kernel.revision);
            Console.WriteLine();
            Console.WriteLine("If this is the first time you've seen this error screen, press any key to restart your computer. If this screen appears again, follow these steps:");
            Console.WriteLine();
            Console.WriteLine("Try to reinstall Aura Operating System on your computer or Virtual Machine. You can also try to reset the filesystem with a blank .vmdk file if you're on a Virtual Machine and if not by formatting your device.");
            Console.WriteLine();
            Console.WriteLine(@"If problems continue, you can contact us at aura-team.com or you can submit your issue on our GitHub repository (github.com/aura-systems/Aura-Operating-System).");
            Console.WriteLine();
            Console.WriteLine("Press any key to reboot...");

            Console.ReadKey();

            Sys.Power.Reboot();
        }

        /// <summary>
        /// Stop the kernel and display exception
        /// </summary>
        /// <param name="ex">Exception that stop the kernel</param>
        public static void StopKernel(string exception, string description, string lastknowaddress, string ctxinterrupt)
        {
            Kernel.running = false;

            Console.BackgroundColor = ConsoleColor.Red;

            Console.Clear();

            Console.WriteLine("CPU Exception x" + ctxinterrupt + " occured in Aura Operating System:");
            Console.WriteLine("Exception: " + exception);
            Console.WriteLine("Description: " + description);
            Console.WriteLine("Aura Version: " + Kernel.version);
            Console.WriteLine("Aura Revision: " + Kernel.revision);
            if (lastknowaddress != "")
            {
                Console.WriteLine("Last known address: 0x" + lastknowaddress);
            }
            Console.WriteLine();
            Console.WriteLine("If this is the first time you've seen this error screen, press any key to restart your computer. If this screen appears again, follow these steps:");
            Console.WriteLine();
            Console.WriteLine("Try to reinstall Aura Operating System on your computer or Virtual Machine. You can also try to reset the filesystem with a blank .vmdk file if you're on a Virtual Machine and if not by formatting your device.");
            Console.WriteLine();
            Console.WriteLine(@"If problems continue, you can contact us at aura-team.com or you can submit your issue on our GitHub repository (github.com/aura-systems/Aura-Operating-System).");
            Console.WriteLine();
            Console.WriteLine("Press any key to reboot...");

            Console.ReadKey();

            Sys.Power.Reboot();
        }
    }
}
