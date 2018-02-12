/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Run Script
* PROGRAMMER(S):    DA CRUZ Alexy <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Executables;
using Cosmos.Debug.Kernel;
using System;
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

        public static readonly Debugger mDebugger = new Debugger("System", "Run");

        /// <summary>
        /// c = command, c_Run
        /// </summary>
        /// <param name="run">The script you wish to start</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Run(string run, short startIndex = 0, short count = 4)
        {
            if (!Kernel.Safemode)
            {
                string file = run.Remove(startIndex, count);
                if (File.Exists(Kernel.current_directory + file))
                {
                    if (file.EndsWith(".bat") || file.EndsWith(".BAT"))
                    {
                        Apps.System.Batch.Execute(file);
                    }
                    else if (file.EndsWith(".aexe") || file.EndsWith(".AEXE"))
                    {
                        byte[] filearray = File.ReadAllBytes(Kernel.current_directory + file);
                        Console.WriteLine(filearray.Length);
                        Console.ReadKey();
                        PlainBinaryProgram.LoadProgram(filearray);
                    }
                    //else if (file.EndsWith(".exe") || file.EndsWith(".EXE"))
                    //{
                    //PE.LoadProgram(File.ReadAllBytes(Kernel.current_directory + file));
                    //}
                    else
                    {
                        Console.WriteLine("We are currently unable to run " + file);
                    }
                }
                else
                {
                    L.Text.Display("doesnotexit");
                }
            }
            else
            {
                L.Text.Display("safemodedisabledex");
            }
        }

    }
}
