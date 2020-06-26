/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command MD5
* PROGRAMMER(S):    DA CRUZ Alexy <dacruzalexy@gmail.com>
*/


namespace Aura_OS.System.Shell.cmdIntr.Tools
{
    class MD5
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
        public MD5() { }

        /// <summary>
        /// c = command, c_MD5
        /// </summary>
        /// <param name="arg">The script you wish to hash</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_MD5(string arg, short startIndex = 0, short count = 4)
        {
            string str = arg.Remove(startIndex, count);
            Apps.User.CryptoTool.HashMD5(str);
        }

    }
}
