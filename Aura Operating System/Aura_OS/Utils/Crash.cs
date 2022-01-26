/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Crash screen
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Sys = Cosmos.System;

namespace Aura_OS.System
{
    public class Crash
    {

        /// <summary>
        /// Stop the kernel and display exception
        /// </summary>
        /// <param name="ex">Exception that stop the kernel</param>
        public static void StopKernel(Exception ex)
        {
            Kernel.Running = false;

            Kernel.console.Background = ConsoleColor.Red;

            string ex_message = ex.Message;
            string inner_message = "";

            //Kernel.canvas.Clear(0xAA0000);

            Kernel.console.X = 0;
            Kernel.console.Y = 0;

            Kernel.console.WriteLine("An error occured in Aura Operating System:");
            Kernel.console.WriteLine(ex.ToString());
            if (ex.InnerException != null)
            {
                inner_message = ex.InnerException.Message;
                Kernel.console.WriteLine(inner_message);
            }
            Kernel.console.WriteLine("Aura Version: " + Kernel.Version);
            Kernel.console.WriteLine("Aura Revision: " + Kernel.Revision);
            Kernel.console.WriteLine();
            Kernel.console.WriteLine("If this is the first time you've seen this error screen, press any key to restart your computer. If this screen appears again, follow these steps:");
            Kernel.console.WriteLine();
            Kernel.console.WriteLine("Try to reinstall Aura Operating System on your computer or Virtual Machine. You can also try to reset the filesystem with a blank .vmdk file if you're on a Virtual Machine and if not by formatting your device.");
            Kernel.console.WriteLine();
            Kernel.console.WriteLine(@"If problems continue, you can contact us on our Discord (discord.gg/DFbAtVA) or you can submit your issue on our GitHub repository (github.com/aura-systems/Aura-Operating-System).");
            Kernel.console.WriteLine();
            Kernel.console.WriteLine("Press any key to reboot...");

            Kernel.canvas.Display();

            Console.ReadKey();

            Sys.Power.Reboot();
        }

        /// <summary>
        /// Stop the kernel and display exception
        /// </summary>
        /// <param name="ex">Exception that stop the kernel</param>
        public static void StopKernel(string exception, string description, string lastknowaddress, string ctxinterrupt)
        {
            Kernel.Running = false;

            Kernel.console.Background = ConsoleColor.Red;

            Kernel.canvas.Clear(0xAA0000);

            Kernel.console.X = 0;
            Kernel.console.Y = 0;

            Kernel.console.WriteLine("CPU Exception x" + ctxinterrupt + " occured in Aura Operating System:");
            Kernel.console.WriteLine("Exception: " + exception);
            Kernel.console.WriteLine("Description: " + description);
            Kernel.console.WriteLine("Aura Version: " + Kernel.Version);
            Kernel.console.WriteLine("Aura Revision: " + Kernel.Revision);
            if (lastknowaddress != "")
            {
                Kernel.console.WriteLine("Last known address: 0x" + lastknowaddress);
            }
            Kernel.console.WriteLine();
            Kernel.console.WriteLine("If this is the first time you've seen this error screen, press any key to restart your computer. If this screen appears again, follow these steps:");
            Kernel.console.WriteLine();
            Kernel.console.WriteLine("Try to reinstall Aura Operating System on your computer or Virtual Machine. You can also try to reset the filesystem with a blank .vmdk file if you're on a Virtual Machine and if not by formatting your device.");
            Kernel.console.WriteLine();
            Kernel.console.WriteLine(@"If problems continue, you can contact us on our Discord (discord.gg/DFbAtVA) or you can submit your issue on our GitHub repository (github.com/aura-systems/Aura-Operating-System).");
            Kernel.console.WriteLine();
            Kernel.console.WriteLine("Press any key to reboot...");

            Kernel.canvas.Display();

            Console.ReadKey();

            Sys.Power.Reboot();
        }
    }
}