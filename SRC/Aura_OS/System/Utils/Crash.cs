/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Crash screen
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Diagnostics;
using CosmosKeyboard = Cosmos.Kernel.System.Keyboard.KeyboardManager;

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
        /// GEN3-GAP(cpu-exception): only managed exceptions reach this screen; CPU faults (#PF, #DE...)
        /// halt in the Cosmos panic handler, whose report is on the serial log.
        /// </summary>
        /// <param name="ex">Exception that stop the kernel</param>
        public static void StopKernel(string exception, string description, string lastknowaddress, string ctxinterrupt)
        {
            Kernel.Running = false;

            // First, so the reason survives in the serial log if drawing faults.
            Log.WriteString("[CRASH] " + exception + ": " + description + "\n");

            string CpuException;
            if (ctxinterrupt == "0")
            {
                CpuException = "A fatal error occurred in Aura Operating System:";
            }
            else
            {
                CpuException = "CPU Exception x" + ctxinterrupt + " occured in Aura Operating System:";
            }

            string Exception = "Exception: " + exception;
            string Description = "Description: " + description;
            string Version = "Aura Version: " + Kernel.Version;
            string Revision = "Aura Revision: " + Kernel.Revision;
            string PressKey = "Press any key to reboot...";

            if (Kernel.Canvas == null || Kernel.errorLogo == null || Kernel.font == null)
            {
                // Boot failure before the canvas or the assets exist (a null dereference is a fatal #PF in gen3):
                // text crash report on the KernelConsole (or on serial once Console.Out is redirected).
                try
                {
                    Console.WriteLine();
                    Console.WriteLine(CpuException);
                    Console.WriteLine(Exception);
                    Console.WriteLine(Description);
                    Console.WriteLine(Version);
                    Console.WriteLine(Revision);
                    Console.WriteLine(PressKey);
                }
                catch
                {
                    // No KernelConsole (no display): the serial log has the reason.
                    // (General catch: the local `Exception` string shadows the type name in this method.)
                }

                WaitKeyAndReboot();
                return;
            }

            int width = Kernel.Canvas.Width;
            int height = Kernel.Canvas.Height;
            int top = (height / 2) - (Kernel.errorLogo.Height / 2);

            Kernel.Canvas.Clear(unchecked((int)0xFFAA0000));

            Kernel.Canvas.DrawImage(Kernel.errorLogo, (width / 2) - (Kernel.errorLogo.Width / 2), top - 89);

            Kernel.Canvas.DrawString(CpuException, Kernel.font, Kernel.WhiteColor, (width / 2) - (CpuException.Length * Kernel.font.Width / 2), top + (89 + 1 * Kernel.font.Height));

            Kernel.Canvas.DrawString(Exception, Kernel.font, Kernel.WhiteColor, (width / 2) - (Exception.Length * Kernel.font.Width / 2), top + (89 + 2 * Kernel.font.Height));

            Kernel.Canvas.DrawString(Description, Kernel.font, Kernel.WhiteColor, (width / 2) - (Description.Length * Kernel.font.Width / 2), top + (89 + 3 * Kernel.font.Height));

            Kernel.Canvas.DrawString(Version, Kernel.font, Kernel.WhiteColor, (width / 2) - (Version.Length * Kernel.font.Width / 2), top + (89 + 4 * Kernel.font.Height));

            Kernel.Canvas.DrawString(Revision, Kernel.font, Kernel.WhiteColor, (width / 2) - (Revision.Length * Kernel.font.Width / 2), top + (89 + 5 * Kernel.font.Height));

            if (ctxinterrupt != "0" && !string.IsNullOrEmpty(lastknowaddress))
            {
                string Lastknownaddress = "Last known address: 0x" + lastknowaddress;
                Kernel.Canvas.DrawString(Lastknownaddress, Kernel.font, Kernel.WhiteColor, (width / 2) - (Lastknownaddress.Length * Kernel.font.Width / 2), top + (89 + 6 * Kernel.font.Height));
            }

            Kernel.Canvas.DrawString(PressKey, Kernel.font, Kernel.WhiteColor, (width / 2) - (PressKey.Length * Kernel.font.Width / 2), top + (89 + 8 * Kernel.font.Height));

            Kernel.Canvas.Display();

            WaitKeyAndReboot();
        }

        /// <summary>
        /// Waits for a fresh key press (keys typed before the crash are flushed; no echo, no KernelConsole
        /// dependency, no tty1 drain), then reboots.
        /// </summary>
        private static void WaitKeyAndReboot()
        {
            try
            {
                if (KernelFeatures.Keyboard)
                {
                    while (CosmosKeyboard.TryReadKey(out _))
                    {
                    }

                    CosmosKeyboard.ReadKey();
                }
            }
            catch (Exception)
            {
            }

            AuraPower.Reboot();
        }
    }
}
