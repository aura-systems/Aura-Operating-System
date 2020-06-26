/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command SHA256
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System.Shell.cmdIntr.Tools
{
    class SHA256
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
        public SHA256() { }

        /// <summary>
        /// c = command, c_SHA256
        /// </summary>
        /// <param name="arg">The script you wish to hash</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_SHA256(string arg, short startIndex = 0, short count = 7)
        {
            string str = arg.Remove(startIndex, count);
            Apps.User.CryptoTool.HashSHA256(str);
        }

    }
}
