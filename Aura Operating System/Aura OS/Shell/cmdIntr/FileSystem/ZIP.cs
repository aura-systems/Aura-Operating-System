/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - ZIP Script
* PROGRAMMER(S):    DA CRUZ Alexy <dacruzalexy@gmail.com>
*/

using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.FileSystem
{
    class ZIP
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
        public ZIP() { }

        /// <summary>
        /// c = command, c_ZIP
        /// </summary>
        /// <param name="zip">Zip archive that you want to extract</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_ZIP(string zip, short startIndex = 0, short count = 4)
        {
            string file = zip.Remove(startIndex, count);
            if (File.Exists(Kernel.current_directory + file))
            {
                System.Compression.ZIP zipc = new System.Compression.ZIP(Kernel.current_directory + file);
                zipc.ExtractFiles();
            }
            else
            {
                L.Text.Display("doesnotexit");
            }
        }

    }
}
