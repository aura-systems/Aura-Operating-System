/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CD
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.Interpreter.Commands.Filesystem
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
                    Directory.SetCurrentDirectory(Kernel.CurrentDirectory);

                    var root = Kernel.VirtualFileSystem.GetDirectory(Kernel.CurrentDirectory);

                    if (Kernel.CurrentDirectory != Kernel.CurrentVolume)
                    {
                        Kernel.CurrentDirectory = root.mParent.mFullPath;
                    }
                }
                else if (dir == Kernel.CurrentVolume)
                { 
                    Kernel.CurrentDirectory = Kernel.CurrentVolume;
                }
                else
                {
                    if (Directory.Exists(Kernel.CurrentDirectory + dir))
                    {
                        Directory.SetCurrentDirectory(Kernel.CurrentDirectory);
                        Kernel.CurrentDirectory = Kernel.CurrentDirectory + dir + @"\";
                    }
                    else if (File.Exists(Kernel.CurrentDirectory + dir))
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Error: This is a file.");
                    }
                    else
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "This directory doesn't exist!");
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