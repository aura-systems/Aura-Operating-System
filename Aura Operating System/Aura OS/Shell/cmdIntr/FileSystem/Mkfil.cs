/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Mkfil
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.FileSystem
{
    class Mkfil
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
        public Mkfil() { }

        /// <summary>
        /// c = commnad, c_mkfil
        /// </summary>
        public static void c_mkfil()
        {
            L.Text.Display("mkfil");
        }
        //TODO: optinal grammar fix add periods '.'
        /// <summary>
        /// c = commnad, c_mkfil
        /// </summary>
        /// <param name="mkfil">The file you wish to create</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_mkfil(string mkfil, short startIndex = 0, short count = 6)
        {
            string file = mkfil.Remove(startIndex, count);
            if (!File.Exists(Kernel.current_directory + file))
            {
                Apps.User.Editor application = new Apps.User.Editor();
                application.Start(file, Kernel.current_directory, false);
            }
            else
            {
                L.Text.Display("alreadyexist");
            }
        }
    }
}
