/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File interface
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Interpreter.Commands;
using Cosmos.Kernel.System.FileSystem;
using System;
using System.IO;

namespace Aura_OS.System.Filesystem
{
    public class Entries
    {
        public static bool ForceRemove(string fullPath)
        {
            // Never delete a volume (/N) or the volume list (/): a recursive delete would empty it.
            if (IsMountPoint(fullPath))
            {
                Console.WriteLine("Cannot remove a volume!");
                return false;
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                return true;
            }
            else
            {
                Console.WriteLine(fullPath + " does not exist!");
                return false;
            }
        }

        public static bool ForceCopy(string sourcePath, string destPath)
        {
            if (File.Exists(sourcePath))
            {
                // A folder as destination (cp a.txt /0/Docs, Explorer "Paste"): copy into it.
                if (Directory.Exists(destPath))
                {
                    destPath = Path.Combine(destPath, Path.GetFileName(sourcePath));
                }

                if (IsSamePath(sourcePath, destPath))
                {
                    Console.WriteLine("Source and destination are the same file!");
                    return false;
                }

                CopyFileContent(sourcePath, destPath);
                return true;
            }
            else if (Directory.Exists(sourcePath))
            {
                if (Directory.Exists(destPath))
                {
                    destPath = Path.Combine(destPath, Path.GetFileName(sourcePath.TrimEnd('/')));
                }

                // Copying a folder into itself would recurse forever.
                if (IsSamePath(sourcePath, destPath) || IsUnder(destPath, sourcePath))
                {
                    Console.WriteLine("Cannot copy a directory into itself!");
                    return false;
                }

                Entries.CopyDirectory(sourcePath, destPath);
                return true;
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
                return false;
            }
        }

        /// <summary>
        /// Deletes a file, or a folder and everything in it.
        /// </summary>
        /// <exception cref="IOException">There is no such file or folder, it is a volume, or the
        /// filesystem failed.</exception>
        public static void Delete(string fullPath)
        {
            fullPath = Normalize(fullPath);

            if (IsMountPoint(fullPath))
            {
                throw new IOException("A volume cannot be deleted.");
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            else
            {
                throw new IOException(NotFound(fullPath));
            }
        }

        /// <summary>
        /// Copies a file, or a folder and everything in it, to destination, a path that must not exist.
        /// </summary>
        /// <exception cref="IOException">There is no such file or folder, the destination exists or is
        /// in the folder, or the filesystem failed.</exception>
        public static void Copy(string source, string destination)
        {
            source = Normalize(source);
            destination = Normalize(destination);

            if (Exists(destination))
            {
                throw new IOException(AlreadyExists(destination));
            }

            if (File.Exists(source))
            {
                CopyFileContent(source, destination);
            }
            else if (Directory.Exists(source))
            {
                // Copying a folder into itself would recurse forever.
                if (IsUnder(destination, source))
                {
                    throw new IOException("A folder cannot be copied into itself.");
                }

                CopyDirectory(source, destination);
            }
            else
            {
                throw new IOException(NotFound(source));
            }
        }

        /// <summary>
        /// Moves a file or a folder to destination, a path that must not exist: a rename on the same
        /// volume, else a copy, then the source is deleted. A change of case only renames.
        /// </summary>
        /// <exception cref="IOException">There is no such file or folder, it is a volume, the
        /// destination exists or is in the folder, or the filesystem failed.</exception>
        public static void Move(string source, string destination)
        {
            source = Normalize(source);
            destination = Normalize(destination);

            if (IsMountPoint(source))
            {
                throw new IOException("A volume cannot be moved or renamed.");
            }

            bool directory = Directory.Exists(source);

            if (!directory && !File.Exists(source))
            {
                throw new IOException(NotFound(source));
            }

            // FAT names ignore the case: "a.txt" to "A.txt" finds the source itself.
            if (!IsSamePath(source, destination))
            {
                if (Exists(destination))
                {
                    throw new IOException(AlreadyExists(destination));
                }

                if (directory && IsUnder(destination, source))
                {
                    throw new IOException("A folder cannot be moved into itself.");
                }
            }

            if (!ReferenceEquals(Volumes.MountOf(source), Volumes.MountOf(destination)))
            {
                Copy(source, destination);
                Delete(source);
            }
            // The VFS rather than Directory.Move and File.Move, which take a change of case for an
            // existing destination on FAT. It would replace one: checked above.
            else if (!VfsManager.TryRename(source, destination))
            {
                throw new IOException("'" + Path.GetFileName(source) + "' could not be moved to '" + destination + "'.");
            }
        }

        private static bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        private static string NotFound(string path)
        {
            return "'" + Path.GetFileName(path) + "' does not exist.";
        }

        private static string AlreadyExists(string path)
        {
            return "'" + Path.GetFileName(path) + "' already exists.";
        }

        public static void SaveFile(string sourcePath, byte[] file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            // GEN3-GAP(finally): the stream is disposed explicitly on every path (see CopyFileContent).
            FileStream stream = null;
            Exception error = null;

            try
            {
                stream = new FileStream(sourcePath, FileMode.Create, FileAccess.Write, FileShare.Read, UnbufferedSize);
                stream.Write(file, 0, file.Length);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            CloseStream(stream, ref error);

            if (error != null)
            {
                throw error;
            }
        }

        public static void CopyFile(string sourcePath, string destPath)
        {
            if (File.Exists(sourcePath) && !IsSamePath(sourcePath, destPath))
            {
                CopyFileContent(sourcePath, destPath);
            }
        }

        public static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            // gen3 returns full paths: keep only the names.
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(destDir, Path.GetFileName(file));

                // Same as the gen2 File.Copy(file, dest) without overwrite.
                if (File.Exists(dest))
                {
                    throw new IOException("The file '" + dest + "' already exists.");
                }

                CopyFileContent(file, dest);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string dest = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDirectory(directory, dest);
            }
        }

        // bufferSize 1 disables the FileStream buffer: every Write goes straight to the file, so
        // Dispose has nothing left to flush and always releases the descriptor.
        private const int UnbufferedSize = 1;

        /// <summary>
        /// Overwriting file copy. GEN3-GAP(finally): `using`/`finally` do not run when an exception
        /// unwinds, and GEN3-GAP(finalizers): there are no finalizers and only 64 descriptors, so the
        /// streams are opened and disposed here explicitly, on every path, then the error is rethrown.
        /// </summary>
        private static void CopyFileContent(string sourcePath, string destPath)
        {
            FileStream source = null;
            FileStream dest = null;
            Exception error = null;

            try
            {
                source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, UnbufferedSize);
                dest = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.Read, UnbufferedSize);

                byte[] buffer = new byte[16384];
                int read;

                while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    dest.Write(buffer, 0, read);
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            CloseStream(dest, ref error);
            CloseStream(source, ref error);

            if (error != null)
            {
                throw error;
            }
        }

        private static void CloseStream(FileStream stream, ref Exception error)
        {
            if (stream == null)
            {
                return;
            }

            try
            {
                stream.Dispose();
            }
            catch (Exception ex)
            {
                if (error == null)
                {
                    error = ex;
                }
            }
        }

        private static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            try
            {
                path = Path.GetFullPath(path);
            }
            catch (Exception)
            {
            }

            return path.Length > 1 ? path.TrimEnd('/') : path;
        }

        private static bool IsMountPoint(string path)
        {
            string normalized = Normalize(path);
            if (normalized == "/")
            {
                return true;
            }

            VfsMount mount;
            return normalized.Length > 0 && VfsManager.TryGetMount(normalized, out mount);
        }

        // FAT names are case-insensitive, so compare paths the same way.
        private static bool IsSamePath(string a, string b)
        {
            return string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUnder(string path, string directory)
        {
            string parent = AuraPath.AsDirectory(Normalize(directory));
            return AuraPath.AsDirectory(Normalize(path)).StartsWith(parent, StringComparison.OrdinalIgnoreCase);
        }
    }
}
