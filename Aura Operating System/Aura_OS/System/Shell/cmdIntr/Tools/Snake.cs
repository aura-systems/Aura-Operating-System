/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Mkdir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.Apps.User;
using System;
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Tools
{
    class Snake
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
        public Snake() { }

        /// <summary>
        /// c = command, c_Mkdir
        /// </summary>
        /// <param name="Snake">The file you wish to create.</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Snake()
        {
            PrgmSnake prgm = new PrgmSnake();
            prgm.Run();

        }
    }
}
