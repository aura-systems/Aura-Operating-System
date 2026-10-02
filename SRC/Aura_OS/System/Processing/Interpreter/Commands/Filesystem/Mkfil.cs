/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Mkfil
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Processing.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandMkfil : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandMkfil(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to create a file";
        }

        /// <summary>
        /// CommandMkfil
        /// </summary>
        public override ReturnInfo Execute()
        {
            PrintHelp();

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandMkfil
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string file = arguments[0];

            try
            {
                string path = AuraPath.Resolve(file);

                // GEN3-GAP(fat-names): the FAT driver writes any character into a long name.
                if (!AuraPath.IsValidName(Path.GetFileName(path)))
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Invalid file name.");
                }

                if (!File.Exists(path))
                {
                    // GEN3-GAP(finalizers): no finalizers, an undisposed FileStream keeps its descriptor forever.
                    File.Create(path).Dispose();
                }
                else
                {
                    Console.WriteLine(file + " already exists!");
                }
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - mkfir {file}");
        }
    }
}