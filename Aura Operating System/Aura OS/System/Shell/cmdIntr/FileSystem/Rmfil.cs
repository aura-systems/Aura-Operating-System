/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rmfil
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class Rmfil
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
        public Rmfil() { }

        /// <summary>
        /// c = commnad, c_Rmfil
        /// </summary>
        public static void c_Rmfil(string rmfil, short startIndex = 0, short count = 6)
        {
            string file = rmfil.Remove(startIndex, count);
            if (File.Exists(Kernel.current_directory + file))
            {
                File.Delete(Kernel.current_directory + file);
            }
            else
            {
                L.Text.Display("doesnotexist", file);
            }
        }
    }
}
