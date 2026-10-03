/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Program package (.pkg)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aura_OS.System.Compression;
using Aura_OS.System.Graphics.UI.GUI.Layout;
using Aura_OS.System.Parser;
using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Processing
{
    /// <summary>
    /// A program package (.pkg): a zip with package.xml at its root, the program's Lua code and, for an
    /// app, its layout file and images. The same file is built into the kernel (SRC/Packages) or
    /// downloaded; SRC/Packages/README.md describes it.
    /// </summary>
    public class Package
    {
        public const string Extension = ".pkg";

        private const string ManifestFile = "package.xml";
        private const string DefaultMain = "main.lua";

        /// <summary>
        /// Program name: "run {name}", and the file name once installed (Programs/{name}.pkg).
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Start menu name, the name when package.xml gives none.
        /// </summary>
        public string DisplayName { get; private set; }

        public string Version { get; private set; }
        public string Author { get; private set; }
        public string Description { get; private set; }

        /// <summary>
        /// Lua file run when the program starts, main.lua when package.xml names none.
        /// </summary>
        public string Main { get; private set; }

        /// <summary>
        /// Layout file of the app's window, null for a console program.
        /// </summary>
        public string Layout { get; private set; }

        /// <summary>
        /// True for an app (a window), false for a console program run in the Terminal.
        /// </summary>
        public bool IsApp => Layout != null;

        /// <summary>
        /// True for a package built into the kernel, which cannot be removed.
        /// </summary>
        public bool BuiltIn { get; internal set; }

        /// <summary>
        /// The .pkg file, as installed.
        /// </summary>
        public byte[] RawData { get; private set; }

        /// <summary>
        /// The package's files by path, '/'-separated ("main.lua", "lib/util.lua").
        /// </summary>
        public Dictionary<string, byte[]> Files { get; private set; }

        private readonly Dictionary<string, Bitmap> _images = new Dictionary<string, Bitmap>();

        /// <exception cref="InvalidDataException">Not a package, or a wrong package.xml.</exception>
        public Package(byte[] data)
        {
            // GEN3-GAP(null-deref): check the data before reading it.
            if (data == null || data.Length == 0)
            {
                throw new InvalidDataException("This is not a package.");
            }

            RawData = data;
            Files = ReadZip(data);
            ReadManifest();
        }

        /// <summary>
        /// The file at that path, false when the package has none.
        /// </summary>
        public bool TryGetFile(string path, out byte[] data)
        {
            data = null;
            return path != null && Files.TryGetValue(Normalize(path), out data) && data != null;
        }

        /// <summary>
        /// A text file, UTF-8 with the BOM stripped.
        /// </summary>
        /// <exception cref="FileNotFoundException">The package has no such file.</exception>
        public string GetText(string path)
        {
            byte[] data;

            if (!TryGetFile(path, out data))
            {
                throw new FileNotFoundException(Name + Extension + " has no file '" + path + "'.", path);
            }

            int skip = data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF ? 3 : 0;
            return Encoding.UTF8.GetString(data, skip, data.Length - skip);
        }

        /// <summary>
        /// A BMP file of the package, decoded on first use and shared afterwards; null when the package
        /// has no such file.
        /// </summary>
        public Bitmap GetImage(string path)
        {
            Bitmap image;

            if (path == null)
            {
                return null;
            }

            if (_images.TryGetValue(path, out image))
            {
                return image;
            }

            byte[] data;

            if (!TryGetFile(path, out data))
            {
                return null;
            }

            image = new Bitmap(data);
            _images.Add(path, image);
            return image;
        }

        /// <summary>
        /// The window layout of an app, its images taken from the package first.
        /// </summary>
        /// <exception cref="InvalidDataException">The layout file is wrong.</exception>
        public AppLayout LoadLayout()
        {
            string name = Path.GetFileNameWithoutExtension(Layout);
            return AppLayout.Parse(name, GetText(Layout), GetImage);
        }

        private void ReadManifest()
        {
            if (!Files.ContainsKey(ManifestFile))
            {
                throw new InvalidDataException("No " + ManifestFile + " in the package.");
            }

            NanoXMLNode root;

            try
            {
                root = new NanoXMLDocument(GetText(ManifestFile)).RootNode;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(ManifestFile + ": " + ex.Message);
            }

            if (root == null || root.Name != "Package")
            {
                throw new InvalidDataException(ManifestFile + ": the root element must be <Package>.");
            }

            Name = LayoutLoader.Attr(root, "name");

            if (!IsValidName(Name))
            {
                throw new InvalidDataException(ManifestFile + ": name must be letters, digits, '-' or '_', not '" + Name + "'.");
            }

            DisplayName = LayoutLoader.Attr(root, "displayName") ?? Name;
            Version = LayoutLoader.Attr(root, "version") ?? "";
            Author = LayoutLoader.Attr(root, "author") ?? "";
            Description = LayoutLoader.Attr(root, "description") ?? "";
            Main = Normalize(LayoutLoader.Attr(root, "main") ?? DefaultMain);
            Layout = LayoutLoader.Attr(root, "layout");

            if (!Main.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) || !Files.ContainsKey(Main))
            {
                throw new InvalidDataException(ManifestFile + ": no Lua file '" + Main + "' in the package.");
            }

            if (Layout != null)
            {
                Layout = Normalize(Layout);

                if (!Files.ContainsKey(Layout))
                {
                    throw new InvalidDataException(ManifestFile + ": no layout file '" + Layout + "' in the package.");
                }
            }
        }

        /// <summary>
        /// Every file of the zip by path; directory entries are left out.
        /// </summary>
        private static Dictionary<string, byte[]> ReadZip(byte[] data)
        {
            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

            // GEN3-GAP(finally): no using blocks, gen3 does not run finally/Dispose when an exception unwinds (C7),
            // so a corrupted archive is caught here, the ZipStorer closed explicitly, then rethrown.
            MemoryStream zipStream = new MemoryStream(data);
            ZipStorer zip = null;
            Exception error = null;

            try
            {
                zip = ZipStorer.Open(zipStream, FileAccess.Read);

                foreach (ZipStorer.ZipFileEntry entry in zip.ReadCentralDir())
                {
                    string path = Normalize(entry.FilenameInZip);

                    if (path.Length == 0 || path.EndsWith("/"))
                    {
                        continue;
                    }

                    MemoryStream fileStream = new MemoryStream();
                    bool extracted = zip.ExtractFile(entry, fileStream);
                    byte[] file = fileStream.ToArray();
                    fileStream.Dispose();

                    if (!extracted)
                    {
                        throw new InvalidDataException("Cannot extract '" + path + "'.");
                    }

                    files[path] = file;
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (zip != null)
            {
                zip.Close();
            }

            zipStream.Dispose();

            if (error != null)
            {
                throw new InvalidDataException("This is not a package (" + error.Message + ")", error);
            }

            return files;
        }

        /// <summary>
        /// "./lib\util.lua" -> "lib/util.lua".
        /// </summary>
        private static string Normalize(string path)
        {
            if (path == null)
            {
                return "";
            }

            path = path.Replace('\\', '/');

            while (path.StartsWith("./"))
            {
                path = path.Substring(2);
            }

            return path.TrimStart('/');
        }

        /// <summary>
        /// Letters, digits, '-' and '_': no '.', which run would take for a file extension.
        /// </summary>
        private static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
