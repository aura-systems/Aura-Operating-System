using Aura_OS.System.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.Apps.System
{
    class Batch
    {
        /// <summary>
        /// Execute batch-like script with ext .aus or .bat
        /// </summary>
        /// <param name="filename">Script file</param>
        public static void Execute(string filename)
        {
            try
            {
                if ((filename.EndsWith(".aus")) || (filename.EndsWith(".bat")))
                {
                    string[] lines = File.ReadAllLines(filename);
                    foreach (string line in lines)
                    {
                        Shell.cmdIntr.CommandManager._CommandManger(line);
                    }
                }
                else
                {
                    Text.Display("notavalidscript");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
