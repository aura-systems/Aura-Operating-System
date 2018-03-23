/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Run Script
* PROGRAMMER(S):    DA CRUZ Alexy <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*                   zarlo <admin@punksky.xyz>
*/

using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.FileSystem
{
    class Run
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public Run() { }

        /// <summary>
        /// c = command, c_Run
        /// </summary>
        /// <param name="run">The script you wish to start</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Run(string run, short startIndex = 0, short count = 4)
        {
            string file = run.Remove(startIndex, count);
            if (File.Exists(Kernel.current_directory + file))
            {
                if (file.EndsWith(".bat") || file.EndsWith(".BAT"))
                {
                    Apps.System.Batch.Execute(Kernel.current_directory + file);
                }
                else if (file.EndsWith(".ksil") || file.EndsWith(".KSIL"))
                {
                    KsIL.KsILVM KsILVM = new KsIL.KsILVM(1024 * 1024);

                    KsILVM.LoadFile(Kernel.current_directory + file);

                    KsILVM.AutoTick();
                }
                else
                {
                    L.Text.Display("notrecognized");
                }
            }
            else
            {
                L.Text.Display("doesnotexit");
            }
        }

    }
}
