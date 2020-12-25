/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Mkdir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class CommandMkdir : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandMkdir(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to create a directory";
        }

        /// <summary>
        /// CommandMkdir
        /// </summary>
        public override ReturnInfo Execute()
        {
            L.Text.Display("mkdir");
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandMkdir
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string dir = arguments[0];

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
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - mkdir {directory}");
        }
    }
}
