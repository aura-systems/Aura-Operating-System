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
            Console.WriteLine();
            Console.WriteLine("An error occured in Aura Operating System:");
            Console.WriteLine(ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
        }

        /// <summary>
        /// Stop the kernel and display exception
        /// </summary>
        /// <param name="ex">Exception that stop the kernel</param>
        public static void StopKernel(string exception, string description, string lastknowaddress, string ctxinterrupt)
        {
            Kernel.Running = false;

            Kernel.Canvas.Clear(0xAA0000);

            Kernel.Canvas.DrawImageAlpha(Kernel.errorLogo, (int)((Kernel.Canvas.Mode.Width / 2) - (Kernel.errorLogo.Width / 2)), (int)((Kernel.Canvas.Mode.Height / 2) - (Kernel.errorLogo.Height / 2) - 89));

            string CpuException = "CPU Exception x" + ctxinterrupt + " occured in Aura Operating System:";
            Kernel.Canvas.DrawString(CpuException, Kernel.font, Kernel.WhiteColor, (int)((Kernel.Canvas.Mode.Width / 2) - (CpuException.Length * Kernel.font.Width / 2)), (int)((Kernel.Canvas.Mode.Height / 2) - (Kernel.errorLogo.Height / 2)) + (89 + 1 * Kernel.font.Height));

            string Exception = "Exception: " + exception;
            Kernel.Canvas.DrawString(Exception, Kernel.font, Kernel.WhiteColor, (int)((Kernel.Canvas.Mode.Width / 2) - (Exception.Length * Kernel.font.Width / 2)), (int)((Kernel.Canvas.Mode.Height / 2) - (Kernel.errorLogo.Height / 2)) + (89 + 2 * Kernel.font.Height));

            string Description = "Description: " + description;
            Kernel.Canvas.DrawString(Description, Kernel.font, Kernel.WhiteColor, (int)((Kernel.Canvas.Mode.Width / 2) - (Description.Length * Kernel.font.Width / 2)), (int)((Kernel.Canvas.Mode.Height / 2) - (Kernel.errorLogo.Height / 2)) + (89 + 3 * Kernel.font.Height));

            string Version = "Aura Version: " + Kernel.Version;
            Kernel.Canvas.DrawString(Version, Kernel.font, Kernel.WhiteColor, (int)((Kernel.Canvas.Mode.Width / 2) - (Version.Length * Kernel.font.Width / 2)), (int)((Kernel.Canvas.Mode.Height / 2) - (Kernel.errorLogo.Height / 2)) + (89 + 4 * Kernel.font.Height));

            string Revision = "Aura Revision: " + Kernel.Revision;
            Kernel.Canvas.DrawString(Revision, Kernel.font, Kernel.WhiteColor, (int)((Kernel.Canvas.Mode.Width / 2) - (Revision.Length * Kernel.font.Width / 2)), (int)((Kernel.Canvas.Mode.Height / 2) - (Kernel.errorLogo.Height / 2)) + (89 + 5 * Kernel.font.Height));

            if (lastknowaddress != "")
            {
                string Lastknownaddress = "Last known address: 0x" + lastknowaddress;
                Kernel.Canvas.DrawString(Lastknownaddress, Kernel.font, Kernel.WhiteColor, (int)((Kernel.Canvas.Mode.Width / 2) - (Lastknownaddress.Length * Kernel.font.Width / 2)), (int)((Kernel.Canvas.Mode.Height / 2) - (Kernel.errorLogo.Height / 2)) + (89 + 6 * Kernel.font.Height));
            }

            string PressKey = "Press any key to reboot...";
            Kernel.Canvas.DrawString(PressKey, Kernel.font, Kernel.WhiteColor, (int)((Kernel.Canvas.Mode.Width / 2) - (PressKey.Length * Kernel.font.Width / 2)), (int)((Kernel.Canvas.Mode.Height / 2) - (Kernel.errorLogo.Height / 2)) + (89 + 8 * Kernel.font.Height));

            Kernel.Canvas.Display();

            Console.ReadKey();

            Sys.Power.Reboot();
        }
    }
}