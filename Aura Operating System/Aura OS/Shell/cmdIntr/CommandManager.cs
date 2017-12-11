/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.Apps.User;
using System;
using System.IO;

namespace Aura_OS.Shell.cmdIntr
{
    class CommandManager
    {
        //TO-DO: Do for all commands:
        //       Windows like command, Linux like command, Aura original command (optional for the last one)
        //Example: else if ((cmd.Equals("ipconfig")) || (cmd.Equals("ifconfig")) || (cmd.Equals("netconf"))) {

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public CommandManager() { }
        /// <summary>
        /// Shell Interpreter
        /// </summary>
        /// <param name="cmd">Command</param>
        public static void _CommandManger(string cmd)
        {

        #region Power

            if (cmd.Equals("shutdown"))
            {//NOTE: Why isn't it just the constructor? This leaves more room for <package>.<class>.HelpInfo;
                Power.Shutdown.c_Shutdown();
            }
            else if (cmd.Equals("reboot"))
            {
                Power.Reboot.c_Reboot();
            }

        #endregion Power

        #region Console

            else if ((cmd.Equals("clear")) || (cmd.Equals("cls")))
            {
                c_Console.Clear.c_Clear();
            }
            else if (cmd.StartsWith("echo "))
            {
                c_Console.Echo.c_Echo(cmd);
            }
            else if (cmd.Equals("help"))
            {
                System.Translation.Help._Help();
            }

        #endregion Console

        #region FileSystem

            else if (cmd.StartsWith("cd "))
            {
                FileSystem.CD.c_CD(cmd);
            }
            else if (cmd.Equals("cp"))
            {
                FileSystem.CP.c_CP();
            }
            else if (cmd.StartsWith("cp "))
            {
                FileSystem.CP.c_CP();
            }
            else if ((cmd.Equals("dir")) || (cmd.Equals("ls")))
            {
                FileSystem.Dir.c_Dir();
            }
            else if ((cmd.StartsWith("dir ")) || (cmd.StartsWith("ls ")))
            {
                FileSystem.Dir.c_Dir(cmd);
            }
            else if (cmd.Equals("mkdir"))
            {
                FileSystem.Mkdir.c_Mkdir();
            }
            else if (cmd.StartsWith("mkdir "))
            {
                FileSystem.Mkdir.c_Mkdir(cmd);
            }
            else if (cmd.StartsWith("rmdir "))
            {
                FileSystem.Rmdir.c_Rmdir(cmd);
            }//TODO: orgainize
            else if (cmd.StartsWith("rmfil "))
            {
                FileSystem.Rmfil.c_Rmfil(cmd);
            }
            else if (cmd.Equals("mkfil"))
            {
                FileSystem.Mkfil.c_mkfil();
            }
            else if (cmd.StartsWith("mkfil "))
            {
                FileSystem.Mkfil.c_mkfil(cmd);
            }
            else if (cmd.StartsWith("edit "))
            {
                FileSystem.Edit.c_Edit(cmd);
            }
            else if (cmd.Equals("vol"))
            {
                FileSystem.Vol.c_Vol();
            }
            else if (cmd.StartsWith("run "))
            {
                FileSystem.Run.c_Run(cmd);
            }

        #endregion FileSystem

        #region Settings

            else if (cmd.Equals("logout"))
            {
                Settings.Logout.c_Logout();
            }
            else if (cmd.Equals("settings"))
            {
                Settings.Settings.c_Settings();
            }
            else if (cmd.StartsWith("settings "))
            {
                Settings.Settings.c_Settings(cmd);
            }

        #endregion Settings

        #region System Infomation

            else if (cmd.Equals("systeminfo"))
            {
                SystemInfomation.SystemInfomation.c_SystemInfomation();
            }
            else if ((cmd.Equals("ver")) || (cmd.Equals("version")))
            {
                SystemInfomation.Version.c_Version();
            }
            else if ((cmd.Equals("ipconfig")) || (cmd.Equals("ifconfig")) || (cmd.Equals("netconf")))
            {
                SystemInfomation.IPConfig.c_IPConfig();
            }
            else if ((cmd.Equals("time")) || (cmd.Equals("date")))
            {
                SystemInfomation.Time.c_Time();
            }

            #endregion System Infomation

        #region Tests

            else if (cmd.Equals("crash"))
            {
                Tests.Crash.c_Crash();
            }

            else if (cmd.StartsWith("print "))
            {
                string file = cmd.Remove(0, 6);

                foreach (string line in File.ReadAllLines("0:\\" + file))
                {
                    Console.WriteLine(line);
                }
            }

            #endregion Tests

            #region Util

            else if (cmd.Equals("snake"))
            {
                PrgmSnake prgm = new PrgmSnake();
                prgm.Run();
            }

            else
            {
                Util.CmdNotFound.c_CmdNotFound();
            }

        #endregion Util

        }
    }
}
