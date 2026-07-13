/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Cat
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandTree : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandTree(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to display a directory tree";
        }

        /// <summary>
        /// CommandTree
        /// </summary>
        public override ReturnInfo Execute()
        {
            try
            {
                string result = DoTree(Kernel.CurrentDirectory, 0);
                Console.WriteLine(result);

                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        /// <summary>
        /// CommandTree
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                string result = DoTree(Path.Combine(Kernel.CurrentDirectory, arguments[0]), 0);
                Console.WriteLine(result);

                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        private string DoTree(string directory, int depth)
        {
            // gen3's plugged System.IO returns full paths (gen2 Cosmos returned bare
            // names): recurse on the returned path and print only the entry name.
            StringBuilder sb = new StringBuilder();
            var directories = Directory.GetDirectories(directory);

            foreach (string file in Directory.GetFiles(directory))
            {
                for (int i = 0; i < depth; i++)
                {
                    sb.Append(" ");
                }
                sb.AppendLine(Path.GetFileName(file));
            }

            for (int j = 0; j < directories.Length; j++)
            {
                for (int i = 0; i < depth; i++)
                {
                    sb.Append(" ");
                }
                sb.AppendLine(Path.GetFileName(directories[j]));
                sb.Append(DoTree(directories[j], depth + 4));
            }

            return sb.ToString();
        }
    }
}