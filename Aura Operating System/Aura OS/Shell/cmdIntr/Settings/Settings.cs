/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Settings
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.IO;
using L = Aura_OS.System.Translation;
using Aura_OS.System.Computer;

namespace Aura_OS.Shell.cmdIntr.Settings
{
    class Settings
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
        public Settings() { }

        /// <summary>
        /// c = command, c_Settings
        /// </summary>
        public static void c_Settings()
        {
            L.Help.Settings();
        }

        public static void c_Settings(string settings)
        {
            Char separator = ' ';
            string[] cmdargs = settings.Split(separator);

            if (cmdargs.Length == 2) //No arg
            {
                if (cmdargs[1].Equals("setcomputername"))
                {
                    //method computername
                    Info.AskComputerName();
                }

                else if (cmdargs[1].Equals("setlang"))
                {
                    L.Text.Display("availablelanguage");
                }

                else if (cmdargs[1].Equals("remuser"))
                {
                    L.Text.Display("remuser");
                }

                else if (cmdargs[1].Equals("adduser"))
                {
                    L.Text.Display("adduser");
                }

                else if (cmdargs[1].Equals("passuser"))
                {
                    L.Text.Display("_passuser");
                }
            }
            else if (cmdargs.Length == 3 ) //One arg
            {
                if (cmdargs[1].Equals("remuser"))
                {
                    System.Users.Users users = new System.Users.Users();

                    users.Remove(cmdargs[2]);
                }

                else if (cmdargs[1].Equals("setlang"))
                {
                    if ((cmdargs[2].Equals("en_US")) || cmdargs[2].Equals("en-US"))
                    {
                        Kernel.langSelected = "en_US";
                        L.Keyboard.Init();
                        System.Utils.Settings.LoadValues();
                        System.Utils.Settings.EditValue("language", "en_US");
                        System.Utils.Settings.PushValues();
                    }
                    else if ((cmdargs[2].Equals("fr_FR")) || cmdargs[2].Equals("fr-FR"))
                    {
                        Kernel.langSelected = "fr_FR";
                        L.Keyboard.Init();
                        System.Utils.Settings.LoadValues();
                        System.Utils.Settings.EditValue("language", "fr_FR");
                        System.Utils.Settings.PushValues();
                    }
                    else if ((cmdargs[2].Equals("nl_NL")) || cmdargs[2].Equals("nl-NL"))
                    {
                        Kernel.langSelected = "nl_NL";
                        L.Keyboard.Init();
                        System.Utils.Settings.LoadValues();
                        System.Utils.Settings.EditValue("language", "nl_NL");
                        System.Utils.Settings.PushValues();
                    }
                    else
                    {
                        L.Text.Display("unknownlanguage");
                        L.Text.Display("availablelanguage");
                    }
                }

                else if (cmdargs[1].Equals("adduser"))
                {
                    L.Text.Display("adduser");
                }

                else if (cmdargs[1].Equals("passuser"))
                {
                    L.Text.Display("_passuser");
                }
            }
            else if (cmdargs.Length == 4) //Two args
            {
                if (cmdargs[1].Equals("adduser"))
                {
                    System.Users.Users users = new System.Users.Users();

                    users.Create(cmdargs[2], cmdargs[3]);
                }

                else if (cmdargs[1].Equals("passuser"))
                {
                    System.Users.Users users = new System.Users.Users();

                    users.ChangePassword(cmdargs[2], cmdargs[3]);
                }

                else if (cmdargs[1].Equals("setlang"))
                {
                    L.Text.Display("availablelanguage");
                }

                else if (cmdargs[1].Equals("remuser"))
                {
                    L.Text.Display("remuser");
                }
            }

            //else if (cmdargs[1].Equals("textcolor"))
            //{
            //    bool save = c_Console.TextColor.c_TextColor(cmdargs[2]);
            //    if (save)
            //    {
            //        System.Utils.Settings.LoadValues();
            //        System.Utils.Settings.EditValue("foregroundcolor", cmdargs[2]);
            //        System.Utils.Settings.PushValues();
            //    }
            //}
            //else if (cmdargs[1].Equals("backgroundcolor"))
            //{
            //    bool save = c_Console.BackGroundColor.c_BackGroundColor(cmdargs[2]);
            //    if (save)
            //    {
            //        System.Utils.Settings.LoadValues();
            //        System.Utils.Settings.EditValue("backgroundcolor", cmdargs[2]);
            //        System.Utils.Settings.PushValues();
            //    }
            //}

            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                L.Text.Display("UnknownCommand");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }
    }
}
