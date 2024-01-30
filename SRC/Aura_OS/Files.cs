using Aura_OS.System.Graphics;
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

        //1920x1024
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.wallpaper1920.bmp")]
        public static byte[] Wallpaper;

        private static string isoVol;

        public static void LoadFiles()
        {
            CustomConsole.WriteLineInfo("Checking for ISO9660 volume...");

            var vols = Kernel.VirtualFileSystem.GetVolumes();
            isoVol = Kernel.CurrentVolume;

            foreach (var vol in vols)
            {
                if (Kernel.VirtualFileSystem.GetFileSystemType(vol.mName).Equals("ISO9660"))
                {
                    isoVol = vol.mName;

                    CustomConsole.WriteLineOK("Volume is " + isoVol);
                }
            }

            // Files
            Kernel.errorLogo = new Bitmap(Files.ErrorImage);
            CustomConsole.WriteLineOK("error.bmp image loaded.");

            // Wallpapers
            Kernel.wallpaper = new Bitmap(Files.Wallpaper);
            CustomConsole.WriteLineOK("wallpaper-1.bmp wallpaper loaded.");

            // Images
            Kernel.AuraLogo = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\AuraLogo.bmp"));
            CustomConsole.WriteLineOK("AuraLogo.bmp image loaded.");

            Kernel.AuraLogoWhite = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\AuraLogoWhite.bmp"));
            CustomConsole.WriteLineOK("AuraLogoWhite.bmp image loaded.");

            Kernel.AuraLogo2 = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\aura.bmp"));
            CustomConsole.WriteLineOK("aura.bmp image loaded.");

            Kernel.CosmosLogo = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\CosmosLogo.bmp"));
            CustomConsole.WriteLineOK("CosmosLogo.bmp image loaded.");

            // Fonts
            Kernel.font = PCScreenFont.LoadFont(File.ReadAllBytes(isoVol + "UI\\Fonts\\zap-ext-light16.psf"));
            CustomConsole.WriteLineOK("zap-ext-light16.psf font loaded.");


        }

        public static void LoadImages()
        {
            // Icons
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\up.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\close.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\minimize.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\explorer.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\network-idle.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\network-offline.bmp", "16");
            //LoadImage(isoVol + "UI\\Images\\Icons\\16\\network-transmit.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\program.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\terminal.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\drive.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\drive-readonly.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\program.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\reboot.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\shutdown.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\terminal.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\explorer.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\32\\file.bmp", "32");
            LoadImage(isoVol + "UI\\Images\\Icons\\32\\folder.bmp", "32");
            LoadImage(isoVol + "UI\\Images\\Icons\\32\\dialog-information.bmp", "32");
            LoadImage(isoVol + "UI\\Images\\Icons\\cursor.bmp", "00");
            LoadImage(isoVol + "UI\\Images\\Icons\\start.bmp", "00");
        }

        public static void LoadImage(string path, string type)
        {
            string fileName = Path.GetFileName(path);
            Bitmap bitmap = new(File.ReadAllBytes(path));
            ResourceManager.Icons.Add(type + "-" + fileName, bitmap);
            CustomConsole.WriteLineOK(fileName + " icon loaded.");
        }
    }
}
