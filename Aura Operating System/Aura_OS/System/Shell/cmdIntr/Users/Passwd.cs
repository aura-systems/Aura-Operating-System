/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CD
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System.IO;
using Aura_OS.System.Users;

namespace Aura_OS.System.Shell.cmdIntr.Users
{
    class Passwd
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
        public Passwd() { }

        /// <summary>
        /// c = commnad, c_CD
        /// </summary>
        /// <param name="user">The directory you wish to pass in</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Passwd(string user, short startIndex = 0, short count = 3)
        {
            string dir = user.Remove(startIndex, count);
            
        }
    }
}
