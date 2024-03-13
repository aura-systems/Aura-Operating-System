using Aura_OS.System;
using Cosmos.System.Graphics.Fonts;
using Cosmos.System.Graphics;
using IL2CPU.API.Attribs;
using System.IO;

namespace Aura_OS
{
    public class Files
    {
        //200x178 .bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.error.bmp")]
        public static byte[] ErrorImage;

        //.gb
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.Tetris.gb")]
        public static byte[] TetrisRom;

        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.wallpaper-1.bmp")]
        public static byte[] Wallpaper;

        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.wallpaper-1.png")]
        public static byte[] WallpaperPng;

        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.wallpaper-2.bmp")]
        public static byte[] Wallpaper2;

        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.auralogo_white.bmp")]
        public static byte[] auralogo_white;

        public static string IsoVolume;

        public static void LoadFiles()
        {
            CustomConsole.WriteLineInfo("Checking for ISO9660 volume...");

            var vols = Kernel.VirtualFileSystem.GetVolumes();
            IsoVolume = Kernel.CurrentVolume;

            foreach (var vol in vols)
            {
                if (Kernel.VirtualFileSystem.GetFileSystemType(vol.mName).Equals("ISO9660"))
                {
                    IsoVolume = vol.mName;

                    CustomConsole.WriteLineOK("Volume is " + IsoVolume);
                }
            }

            // Files
            Kernel.errorLogo = new Bitmap(Files.ErrorImage);
            CustomConsole.WriteLineOK("error.bmp image loaded.");

            // Wallpapers
            Kernel.wallpaper1 = new Bitmap(Files.Wallpaper);
            CustomConsole.WriteLineOK("wallpaper-1.bmp wallpaper loaded.");

            Kernel.wallpaper1png = new Png(Files.WallpaperPng);
            CustomConsole.WriteLineOK("wallpaper-1.png wallpaper loaded.");

            Kernel.wallpaper2 = new Bitmap(Files.Wallpaper2);
            CustomConsole.WriteLineOK("wallpaper-2.bmp wallpaper loaded.");

            // Logo
            Kernel.auralogo_white = new Bitmap(Files.auralogo_white);
            CustomConsole.WriteLineOK("auralogo_white.bmp wallpaper loaded.");

            // Images
            Kernel.AuraLogo = new Bitmap(File.ReadAllBytes(IsoVolume + "UI\\Images\\AuraLogo.bmp"));
            CustomConsole.WriteLineOK("AuraLogo.bmp image loaded.");

            Kernel.AuraLogoWhite = new Bitmap(File.ReadAllBytes(IsoVolume + "UI\\Images\\AuraLogoWhite.bmp"));
            CustomConsole.WriteLineOK("AuraLogoWhite.bmp image loaded.");

            Kernel.AuraLogo2 = new Bitmap(File.ReadAllBytes(IsoVolume + "UI\\Images\\aura.bmp"));
            CustomConsole.WriteLineOK("aura.bmp image loaded.");

            Kernel.CosmosLogo = new Bitmap(File.ReadAllBytes(IsoVolume + "UI\\Images\\CosmosLogo.bmp"));
            CustomConsole.WriteLineOK("CosmosLogo.bmp image loaded.");

            // Fonts
            Kernel.font = PCScreenFont.LoadFont(File.ReadAllBytes(IsoVolume + "UI\\Fonts\\zap-ext-light16.psf"));
            CustomConsole.WriteLineOK("zap-ext-light16.psf font loaded.");
        }

        public static void LoadImages()
        {
            // Icons
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\up.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\close.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\minimize.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\explorer.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\network-idle.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\network-offline.bmp", "16");
            //LoadImage(isoVol + "UI\\Images\\Icons\\16\\network-transmit.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\program.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\terminal.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\drive.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\drive-readonly.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\16\\settings.bmp", "16");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\24\\program.bmp", "24");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\24\\reboot.bmp", "24");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\24\\logout.bmp", "24");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\24\\settings.bmp", "24");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\24\\shutdown.bmp", "24");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\24\\terminal.bmp", "24");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\24\\explorer.bmp", "24");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\32\\file.bmp", "32");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\32\\folder.bmp", "32");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\32\\dialog-information.bmp", "32");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\32\\dialog-error.bmp", "32");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\cursor.bmp", "00");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\grab.bmp", "00");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\resize-horizontal.bmp", "00");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\resize-vertical.bmp", "00");
            LoadImage(IsoVolume + "UI\\Images\\Icons\\start.bmp", "00");
        }

        public static void LoadImage(string path, string type)
        {
            string fileName = Path.GetFileName(path);
            Bitmap bitmap = new(File.ReadAllBytes(path));
            Kernel.ResourceManager.AddIcon(type + "-" + fileName, bitmap);
            CustomConsole.WriteLineOK(fileName + " icon loaded.");
        }
    }
}
