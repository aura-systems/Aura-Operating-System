/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Mkdir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.FileSystem
{
    class Mkdir
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
        public Mkdir() { }

        /// <summary>
        /// c = commnad, c_Mkdir
        /// </summary>
        public static void c_Mkdir()
        {
            L.Text.Display("mkdir");
        }

        /// <summary>
        /// c = command, c_Mkdir
        /// </summary>
        /// <param name="mkdir">The file you wish to create.</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Mkdir(string mkdir, short startIndex = 0, short count = 6)
        {
            string dir = mkdir.Remove(startIndex, count);

            if (dir.Contains("."))
            {
                L.Text.Display("mkdirunsupporteddot");
            }

            else
            {
                if (!Directory.Exists(Kernel.current_directory + dir))
                {
                    Directory.CreateDirectory(Kernel.current_directory + dir);
                }
                else if (Directory.Exists(Kernel.current_directory + dir))
                {
                    Char[] separators = new char[] { '(', ')' };
                    Char[] directories = new char[] { '/', '\\' };

                    //getting last directory.
                    string x1 = Kernel.current_directory + dir;
                    string[] x2 = x1.Split(directories);
                    string x3 = x2[x2.Length - 1];
                    int x4 = 0;

                    //boucle
                    boucle:
                    x4 = x4 + 1;
                    string DirectoryAlreadyExist = x3 + "(" + x4 + ")";

                    //if directory has already been recreated once or more
                    if ((DirectoryAlreadyExist.Contains("(")) && (DirectoryAlreadyExist.Contains(")")))
                    {
                        //get number
                        string[] endName = DirectoryAlreadyExist.Split(separators);
                        int num = int.Parse(endName[1]);

                        //add one to num
                        num = num + 1;

                        //set new directory name with the new number.
                        string newName = dir + "(" + num + ")";

                        if (Directory.Exists(Kernel.current_directory + newName))
                        {
                            goto boucle;
                        }

                        //create directory
                        Directory.CreateDirectory(Kernel.current_directory + newName);

                        //display text to inform the user that the directory has been created with an another name
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        L.Text.Display("mkdirfilealreadyexist", newName);
                    }
                    else
                    {
                        //if not, we create directory with (1)
                        Directory.CreateDirectory(Kernel.current_directory + dir + "(1)");

                        //display text to inform the user that the directory has been created with an another name
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        L.Text.Display("mkdirfilealreadyexist", dir + "(1)");
                    }
                }
            }

        }
    }
}
