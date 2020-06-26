/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Batch (.bat) implementation.
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr;
using Aura_OS.System.Translation;
using System;
using System.IO;

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
                        if(!(line.StartsWith("|")))// don't read the line if it start with "|" for comment
                        {

                            CommandManager._CommandManger(line);
                        }
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
