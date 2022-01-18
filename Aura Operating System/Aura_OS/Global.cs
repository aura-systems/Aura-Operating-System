using Aura_OS;
using Aura_OS.System;
using Aura_OS.System.Users;
using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;

namespace Aura_OS
{
    public class Global
    {
        #region Global variables

        public static Setup setup = new Setup();
        public static bool running;
        public static string version = "0.5.3";
        public static string revision = VersionInfo.revision;
        public static string current_directory = @"0:\";
        public static string langSelected = "en_US";
        public static string userLogged;
        public static string userLevelLogged;
        public static bool Logged = false;
        public static string ComputerName = "aura-pc";
        public static string UserDir = @"0:\Users\" + userLogged + "\\";
        public static bool SystemExists = false;
        public static bool JustInstalled = false;
        public static CosmosVFS vFS = new CosmosVFS();
        public static Dictionary<string, string> environmentvariables = new Dictionary<string, string>();
        public static string boottime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);
        public static Aura_OS.System.AConsole.Console AConsole;
        public static string Consolemode = "VGATextmode";
        public static string current_volume = @"0:\";

        public static Cosmos.Debug.Kernel.Debugger debugger = new Cosmos.Debug.Kernel.Debugger("Aura", "Kernel");

        #endregion

        #region BeforeCommand
        /// <summary>
        /// Display the line before the user input and set the console color.
        /// </summary>
        public static void BeforeCommand()
        {
            if (current_directory == current_volume)
            {

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(UserLevel.TypeUser());

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(userLogged);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("@");

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(ComputerName);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> ");

                Console.ForegroundColor = ConsoleColor.White;

            }
            else
            {

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(UserLevel.TypeUser());

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(userLogged);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("@");

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(ComputerName);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(current_directory + "~ ");

                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        #endregion

        public static bool ContainsVolumes()
        {
            if (vFS.GetVolumes().Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
