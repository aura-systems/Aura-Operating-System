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
                DoTree(Kernel.CurrentDirectory + "/" + arguments[0], 0);
                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        private string DoTree(string directory, int depth)
        {
            StringBuilder sb = new StringBuilder();
            var directories = Directory.GetDirectories(directory);

            foreach (string file in Directory.GetFiles(directory))
            {
                for (int i = 0; i < depth; i++)
                {
                    sb.Append(" ");
                }
                sb.AppendLine(file);
            }

            for (int j = 0; j < directories.Length; j++)
            {
                for (int i = 0; i < depth; i++)
                {
                    sb.Append(" ");
                }
                sb.AppendLine(directories[j]);
                sb.Append(DoTree(directory + "/" + directories[j], depth + 4));
            }

            return sb.ToString();
        }
    }
}