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
using Aura_OS.System.Utils;
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

        /// <summary>
        /// Between a package's name and one of its images' in an icon name ("Explorer:folder").
        /// </summary>
        public const char IconSeparator = ':';

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
        /// False for an app that is not in the start menu (menu="false"): one opened with a file, as the Editor.
        /// </summary>
        public bool InMenu { get; private set; }

        /// <summary>
        /// Start menu icon: the name of one of the package's images (24 x 24), null for the kernel's
        /// program icon.
        /// </summary>
        public string Icon { get; private set; }

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

        // The images package.xml names (<Image name src/>): name -> file of the package.
        private readonly Dictionary<string, string> _imageFiles = new Dictionary<string, string>();

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
        /// <exception cref="InvalidDataException">The file is not a BMP the kernel can show.</exception>
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

            // A downloaded package's file: a wrong BMP header would halt the kernel in Bitmap.
            image = ImageUtils.LoadBmp(data);
            _images.Add(path, image);
            return image;
        }

        /// <summary>
        /// True when package.xml names an image so.
        /// </summary>
        public bool HasIcon(string name)
        {
            return name != null && _imageFiles.ContainsKey(name);
        }

        /// <summary>
        /// The image package.xml names so, decoded on first use (LoadImages decodes them all when an app
        /// of the package opens); false when it names none, or its file is not a BMP.
        /// </summary>
        public bool TryGetIcon(string name, out Bitmap icon)
        {
            icon = null;
            string path;

            if (name == null || !_imageFiles.TryGetValue(name, out path))
            {
                return false;
            }

            try
            {
                icon = GetImage(path);
            }
            catch (Exception)
            {
                icon = null;
            }

            return icon != null;
        }

        /// <summary>
        /// The name another package shows one of this package's images by ("Explorer:folder", as the
        /// Task Manager shows an app's window icon); any other name as it is.
        /// </summary>
        public string IconKey(string name)
        {
            return HasIcon(name) ? Name + IconSeparator + name : name;
        }

        /// <summary>
        /// Decodes every image package.xml names: done when an app of the package opens, so that a wrong
        /// file stops the app there, before its window, rather than leave a control without its icon.
        /// </summary>
        /// <exception cref="InvalidDataException">An image's file is not a BMP the kernel can show.</exception>
        public void LoadImages()
        {
            Exception error = null;
            string failed = null;

            foreach (KeyValuePair<string, string> image in _imageFiles)
            {
                try
                {
                    GetImage(image.Value);
                }
                catch (Exception ex)
                {
                    error = ex;
                    failed = image.Key;
                }

                if (error != null)
                {
                    break;
                }
            }

            // Thrown outside the catch, as ReadZip does.
            if (error != null)
            {
                throw new InvalidDataException(ManifestFile + ": the image '" + failed + "' (" + _imageFiles[failed] + "): " + error.Message, error);
            }
        }

        /// <summary>
        /// The window layout of an app. Its icons are the names of the package's images (LoadImages), or
        /// of the kernel's icons; its Images' src are files of the package first.
        /// </summary>
        /// <exception cref="InvalidDataException">The layout file is wrong.</exception>
        public AppLayout LoadLayout()
        {
            string name = Path.GetFileNameWithoutExtension(Layout);
            return AppLayout.Parse(name, GetText(Layout), GetImage, FindIcon);
        }

        /// <summary>
        /// An icon this package names (ResourceManager.TryGetIcon): one of its images, another
        /// package's ("Explorer:folder") or a kernel icon; null when there is none.
        /// </summary>
        public Bitmap FindIcon(string name)
        {
            Bitmap icon;
            return Kernel.ResourceManager.TryGetIcon(name, this, out icon) ? icon : null;
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

            string menu = LayoutLoader.Attr(root, "menu");

            if (menu != null && menu != "true" && menu != "false")
            {
                throw new InvalidDataException(ManifestFile + ": menu must be true or false, not '" + menu + "'.");
            }

            InMenu = menu != "false";

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

            ReadImages(root);

            Icon = LayoutLoader.Attr(root, "icon");

            if (Icon != null && !_imageFiles.ContainsKey(Icon))
            {
                throw new InvalidDataException(ManifestFile + ": icon must be the name of one of its <Image> elements, not '" + Icon + "'.");
            }
        }

        /// <summary>
        /// The &lt;Image name="folder" src="Icons/16/folder.bmp" /&gt; elements of package.xml: the
        /// images the layout and the code name (an icon attribute, an item's icon). Decoded by LoadImages.
        /// </summary>
        private void ReadImages(NanoXMLNode root)
        {
            foreach (NanoXMLNode element in root.SubNodes)
            {
                if (element.Name != "Image")
                {
                    throw new InvalidDataException(ManifestFile + ": <Package> holds <Image> elements, not <" + element.Name + ">.");
                }

                string name = LayoutLoader.Attr(element, "name");
                string src = LayoutLoader.Attr(element, "src");

                if (!IsValidImageName(name))
                {
                    throw new InvalidDataException(ManifestFile + ": an image's name must be letters, digits, '-', '_' or '.', not '" + name + "'.");
                }

                if (_imageFiles.ContainsKey(name))
                {
                    throw new InvalidDataException(ManifestFile + ": two images are named '" + name + "'.");
                }

                if (src == null)
                {
                    throw new InvalidDataException(ManifestFile + ": the image '" + name + "' has no src.");
                }

                string path = Normalize(src);

                if (!Files.ContainsKey(path))
                {
                    throw new InvalidDataException(ManifestFile + ": no file '" + src + "' in the package for the image '" + name + "'.");
                }

                _imageFiles.Add(name, path);
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

        /// <summary>
        /// A program name's characters, and '.': no IconSeparator, which names another package's image.
        /// </summary>
        private static bool IsValidImageName(string name)
        {
            return name != null && IsValidName(name.Replace('.', '_'));
        }
    }
}
