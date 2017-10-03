using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.SystemInfomation
{
    class IPConfig
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
        public IPConfig() { }

        /// <summary>
        /// c = command, c_SystemInfomation
        /// </summary>
        public static void c_IPConfig()
        {
            L.Text.Display("MAC");            
        }
    }
}
