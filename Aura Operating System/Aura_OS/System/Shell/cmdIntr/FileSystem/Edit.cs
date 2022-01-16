/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Edit
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

/*
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class Edit
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; }
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public Edit() { }

        /// <summary>
        /// c = command, c_Edit
        /// </summary>
        /// <param name="edit">The file you wish to edit</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Edit(string edit, short startIndex = 0, short count = 5)
        {
            string file = edit.Remove(startIndex, count);
            if (File.Exists(Global.current_directory + file))
            {
                Apps.User.Editor application = new Apps.User.Editor();
                application.Start(file, Global.current_directory);
            }
            else
            {
                L.Text.Display("doesnotexit");
            }
        }

    }
}
*/