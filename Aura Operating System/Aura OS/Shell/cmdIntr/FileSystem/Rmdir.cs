/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rmdir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.FileSystem
{
    class Rmdir
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
        public Rmdir() { }

        /// <summary>
        /// c = commnad, c_Rmdir
        /// </summary>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Rmdir(string rmdir, short startIndex = 0, short count = 6)
        {
            string dir = rmdir.Remove(startIndex, count);
            if (Directory.Exists(Kernel.current_directory + dir))
            {
                Directory.Delete(Kernel.current_directory + dir, true);
            }
            else
            {
                L.Text.Display("doesnotexist");
            }
        }

    }
}
