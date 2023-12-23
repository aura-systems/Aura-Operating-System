using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                            System.CustomConsole.WriteLineInfo(line);
                            Kernel.CommandManager.Execute(line);
                            System.CustomConsole.WriteLineOK(line);
                        }
                    }
                }
                else
                {
                    System.CustomConsole.WriteLineError("This file is not a valid script.");
                }
            }
            catch (Exception ex)
            {
                System.CustomConsole.WriteLineError(ex.Message);
            }
        }
    }
}
