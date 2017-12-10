using Aura_OS.System.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.Shell.cmdIntr.FileSystem
{
    class Chdir
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
        public Chdir() { }

        /// <summary>
        /// c = commnad, c_Chdir
        /// </summary>
        /// <param name="directory">The directory you wish to pass in</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Chdir(string directory, short startIndex = 0, short count = 6)
        {
            string dir = directory.Remove(startIndex, count);
            try
            {
                if (Directory.Exists(Kernel.current_directory + dir))
                {
                    Directory.SetCurrentDirectory(Kernel.current_directory + dir);
                    Kernel.current_directory = Kernel.current_directory + dir + @"\";
                    Console.WriteLine(Kernel.current_directory);
                }
                else if (File.Exists(Kernel.current_directory + dir))
                {
                    Text.Display("errorthisisafile");
                }
                else
                {
                    Text.Display("directorydoesntexist");
                }
            }
            catch { }
        }

        public static void CurrentDirectory()
        {
            Console.WriteLine(Kernel.current_directory);
        }
    }
}
