/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CP
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.FileSystem
{
    class CP
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
        public CP() { }

        /// <summary>
        /// c = commnad, c_CP
        /// </summary>
        public static void c_CP_only()
        {
            L.Text.Display("usagecp");
        }

        /// <summary>
        /// c = command, c_CP
        /// </summary>
        /// <param name="cp">The path of the directory/file you wish to pass in</param>
        public static void c_CP(string cp, short startIndex = 0, short count = 3)
        {
            //args commands
            Char cmdargschar = ' ';
            string[] cmdargs = cp.Split(cmdargschar);

            if (!cmdargs[1].StartsWith("-")) //WITHOUT ARGS, NO OVERWRITING
            {
                string sourcefile = cmdargs[1];
                string destfile = cmdargs[2];

                if (cmdargs.Length == 3)
                {
                    if (File.Exists(Kernel.current_directory + sourcefile))
                    {
                        if (!File.Exists(Kernel.current_directory + destfile))
                        {
                            try
                            {
                                File.Copy(Kernel.current_directory + sourcefile, Kernel.current_directory + destfile);

                                Console.ForegroundColor = ConsoleColor.Green;
                                L.Text.Display("filecopied");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            catch (IOException ioEx)
                            {
                                throw new IOException("File Copy", ioEx);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            L.Text.Display("filealreadyexist");
                            Console.ForegroundColor = ConsoleColor.Green;
                            L.Text.Display("docpoover");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        L.Text.Display("sourcefiledoesntexist");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    L.Text.Display("usagecp");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            else
            {
                if (cmdargs[1].Equals("-o")) //FIRST ARGS, OVERWRITING
                {
                    //string sourcefile = cmdargs[0]; args0 is command args "cp"
                    string sourcefile = cmdargs[2];
                    string destfile = cmdargs[3];

                    //code following
                    if (cmdargs.Length == 4)
                    {
                        if (File.Exists(Kernel.current_directory + sourcefile))
                        {
                            if (File.Exists(Kernel.current_directory + destfile))
                            {
                                try
                                {
                                    File.Copy(Kernel.current_directory + sourcefile, Kernel.current_directory + destfile, true);

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    L.Text.Display("filecopied");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                catch (IOException ioEx)
                                {
                                    throw new IOException("File Copy", ioEx);
                                }
                            }
                            else
                            {
                                File.Copy(Kernel.current_directory + sourcefile, Kernel.current_directory + destfile);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            L.Text.Display("sourcefiledoesntexist");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        L.Text.Display("usagecp");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    L.Text.Display("invalidargument");
                }
            }

        }
    }
}
