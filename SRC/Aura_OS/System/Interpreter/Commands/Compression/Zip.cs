/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Zip
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UniLua;

namespace Aura_OS.Interpreter.Commands.Util
{
    class CommandZip : ICommand
    {
        public CommandZip(string[] commandvalues) : base(commandvalues)
        {
            Description = "to execute a zip and unzip archives.";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string option = arguments[0].ToLower();

            if (option == "/e") // Extract
            {
                return Extract(arguments);
            }
            else if (option == "/c") // Compress (Not implemented yet)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Compression not implemented yet.");
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
        }

        private ReturnInfo Extract(List<string> arguments)
        {
            string archivePath = ResolvePath(arguments[1]);
            string extractPath;

            if (arguments.Count > 2)
            {
                extractPath = ResolvePath(arguments[2]);
            }
            else
            {
                string archiveName = Path.GetFileNameWithoutExtension(archivePath);
                extractPath = Path.Combine(Kernel.CurrentDirectory, archiveName);
            }

            Directory.CreateDirectory(extractPath);

            try
            {
                using (ZipStorer zip = ZipStorer.Open(archivePath, FileAccess.Read))
                {
                    List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                    foreach (ZipStorer.ZipFileEntry entry in dir)
                    {
                        string outputFile = Path.Combine(extractPath, entry.FilenameInZip);
                        zip.ExtractFile(entry, outputFile);
                    }
                }

                Console.WriteLine("Extraction completed.");
                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception e)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, e.ToString());
            }
        }

        private string ResolvePath(string path)
        {
            if (path.StartsWith("./"))
            {
                path = path.Substring(2);
                path = Kernel.CurrentDirectory + path;
            }

            if (path == ".")
            {
                return Kernel.CurrentDirectory;
            }
            else
            {
                return Path.Combine(Kernel.CurrentDirectory, path);
            }
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - zip /e {source_archive} {destination_directory}");
            Console.WriteLine(" - zip /c {source_directory} {destination_archive} (Not implemented)");
        }
    }
}
