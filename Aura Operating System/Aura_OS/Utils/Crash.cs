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
        public static void WriteException(Exception ex)
        {
            Kernel.console.WriteLine();
            Kernel.console.WriteLine("An error occured in Aura Operating System:");
            Kernel.console.WriteLine(ex.Message);
            if (ex.InnerException != null)
            {
                Kernel.console.WriteLine(ex.InnerException.Message);
            }
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

            int i = 0;

            Kernel.canvas.DrawString("CPU Exception x" + ctxinterrupt + " occured in Aura Operating System:", Kernel.font, Kernel.WhitePen, 2, i++ * Kernel.font.Height);
            Kernel.canvas.DrawString("Exception: " + exception, Kernel.font, Kernel.WhitePen, 2, i++ * Kernel.font.Height);
            Kernel.canvas.DrawString("Description: " + description, Kernel.font, Kernel.WhitePen, 2, i++ * Kernel.font.Height);
            Kernel.canvas.DrawString("Aura Version: " + Kernel.Version, Kernel.font, Kernel.WhitePen, 2, i++ * Kernel.font.Height);
            Kernel.canvas.DrawString("Aura Revision: " + Kernel.Revision, Kernel.font, Kernel.WhitePen, 2, i++ * Kernel.font.Height);
            if (lastknowaddress != "")
            {
                Kernel.canvas.DrawString("Last known address: 0x" + lastknowaddress, Kernel.font, Kernel.WhitePen, 2, i++ * Kernel.font.Height);
            }

            Kernel.canvas.DrawString("Press any key to reboot...", Kernel.font, Kernel.WhitePen, 2, i++ * Kernel.font.Height);

            Kernel.canvas.Display();

            Console.ReadKey();

            Sys.Power.Reboot();
        }
    }
}