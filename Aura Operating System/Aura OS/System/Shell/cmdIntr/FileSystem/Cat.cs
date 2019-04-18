/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Cat
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using L = Aura_OS.System.Translation;
namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class Cat
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
        public Cat() { }

        /// <summary>
        /// c = commnad, c_Cat
        /// </summary>
        /// <param name="cat">The directory you wish to pass in</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Cat(string path, short startIndex = 0, short count = 4)
        {
            try
            {
                string file = path.Remove(startIndex, count);
                if (File.Exists(Kernel.current_directory + file))
                {
                    foreach (string line in File.ReadAllLines(Kernel.current_directory + file))
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    L.Text.Display("doesnotexit");
                }
            }
            catch { }
        }
    }
}
