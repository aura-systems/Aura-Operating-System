/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Zip
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Compression;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Processing.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Util
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
            string archivePath = AuraPath.Resolve(arguments[1]);
            string extractPath;

            if (arguments.Count > 2)
            {
                extractPath = AuraPath.Resolve(arguments[2]);
            }
            else
            {
                string archiveName = Path.GetFileNameWithoutExtension(archivePath);
                extractPath = Path.Combine(Kernel.CurrentDirectory, archiveName);
            }

            if (!File.Exists(archivePath))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "This file does not exist.");
            }

            // GEN3-GAP(finally): `using` does not dispose when an exception unwinds (a bad archive throws
            // InvalidDataException), so catch inside and close the archive explicitly.
            ZipStorer zip = null;
            string error = null;

            try
            {
                Directory.CreateDirectory(extractPath);

                zip = ZipStorer.Open(archivePath, FileAccess.Read);

                List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                foreach (ZipStorer.ZipFileEntry entry in dir)
                {
                    // Entry names use '/', and gen2 also accepted '\' (a plain character in gen3 names).
                    string name = entry.FilenameInZip.Replace('\\', '/').TrimStart('/');
                    string outputFile = Path.GetFullPath(Path.Combine(extractPath, name));

                    // Never write outside the destination ("../" in an entry name), and skip names the FAT
                    // driver would store as-is with reserved characters (GEN3-GAP(fat-names)).
                    if (!outputFile.StartsWith(AuraPath.AsDirectory(extractPath), StringComparison.Ordinal)
                        || !IsValidEntryName(name))
                    {
                        continue;
                    }

                    zip.ExtractFile(entry, outputFile);
                }
            }
            catch (Exception e)
            {
                error = e.ToString();
            }

            if (zip != null)
            {
                try
                {
                    zip.Dispose();
                }
                catch (Exception e)
                {
                    if (error == null)
                    {
                        error = e.ToString();
                    }
                }
            }

            if (error != null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, error);
            }

            Console.WriteLine("Extraction completed.");
            return new ReturnInfo(this, ReturnCode.OK);
        }

        private static bool IsValidEntryName(string name)
        {
            foreach (string part in name.Split('/'))
            {
                if (part.Length > 0 && !AuraPath.IsValidName(part))
                {
                    return false;
                }
            }

            return true;
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - zip /e {source_archive} {destination_directory}");
            Console.WriteLine(" - zip /c {source_directory} {destination_archive} (Not implemented)");
        }
    }
}
