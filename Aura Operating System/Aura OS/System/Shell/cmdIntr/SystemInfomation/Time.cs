/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Time
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.SystemInfomation
{
    class Time
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
        public Time() { }

        /// <summary>
        /// c = command, c_Time
        /// </summary>
        public static void c_Time()
        {
            L.Text.Display("time");
        }
    }
}
