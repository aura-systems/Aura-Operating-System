/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Batch
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Interpreter.Commands;
using System;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter
{
    public class Batch
    {
        /// <summary>
        /// Execute batch-like script with ext .aus or .bat
        /// </summary>
        /// <param name="filename">Script file</param>
        public static void Execute(string filename)
        {
            try
            {
                if (filename.EndsWith(".bat"))
                {
                    string[] lines = File.ReadAllLines(filename);
                    foreach (string line in lines)
                    {
                        string processedLine = line.Replace("\r", "").Replace("\n", "");

                        if (!(line.StartsWith(";") || line.Equals("")))
                        {
                            CustomConsole.WriteLineInfo(line);
                            CommandManager commandManager = new();
                            commandManager.Initialize();
                            commandManager.Execute(line);
                            CustomConsole.WriteLineOK(line);
                        }
                    }
                }
                else
                {
                    CustomConsole.WriteLineError("This file is not a valid script.");
                }
            }
            catch (Exception ex)
            {
                CustomConsole.WriteLineError(ex.Message);
            }
        }
    }
}
