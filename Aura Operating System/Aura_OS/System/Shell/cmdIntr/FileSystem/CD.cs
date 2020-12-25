/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CD
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using L = Aura_OS.System.Translation;
namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class CommandCD : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandCD(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to change current directory";
        }

        /// <summary>
        /// CommandCd
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string dir = arguments[0];
            try
            {
                if (dir == "..")
                {
                    Directory.SetCurrentDirectory(Kernel.current_directory);
                    var root = Kernel.vFS.GetDirectory(Kernel.current_directory);
                    if (Kernel.current_directory == Kernel.current_volume)
                    {
                    }
                    else
                    {
                        Kernel.current_directory = root.mParent.mFullPath;
                    }
                }
                else if (dir == Kernel.current_volume)
                { 
                    Kernel.current_directory = Kernel.current_volume;
                }
                else
                {
                    if (Directory.Exists(Kernel.current_directory + dir))
                    {
                        Directory.SetCurrentDirectory(Kernel.current_directory);
                        Kernel.current_directory = Kernel.current_directory + dir + @"\";
                    }
                    else if (File.Exists(Kernel.current_directory + dir))
                    {
                        L.Text.Display("errorthisisafile");
                    }
                    else
                    {
                        L.Text.Display("directorydoesntexist");
                    }
                }
                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- cd {directory}    change current directory");
            Console.WriteLine("- cd ..             go to last directory");
        }
    }
}
