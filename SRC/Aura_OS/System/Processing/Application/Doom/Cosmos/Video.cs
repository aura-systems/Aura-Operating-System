using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.ExceptionServices;
using ManagedDoom.Video;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.Utils;
using Aura_OS;
using Cosmos.System.Graphics;

namespace ManagedDoom.Cosmos
{
    public sealed class CosmosVideo : IVideo, IDisposable
    {
        private Renderer renderer;

        private int textureWidth;
        private int textureHeight;

        private byte[] textureData;

        private DirectBitmap bitmap;

        public CosmosVideo(Config config, GameContent content)
        {
            try
            {
                Console.Write("Initialize video: ");

                renderer = new Renderer(config, content);
                bitmap = new DirectBitmap(640, 400);

                if (config.video_highresolution)
                {
                    textureWidth = 512;
                    textureHeight = 1024;
                }
                else
                {
                    textureWidth = 256;
                    textureHeight = 512;
                }

                textureData = new byte[4 * renderer.Width * renderer.Height];
               

                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed");
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        public void Render(Doom doom, Fixed frameFrac)
        {
            renderer.Render(doom, textureData, frameFrac);
            //Kernel.canvas.DrawImage(bitmap.Bitmap, 0, 0);
        }

        public void InitializeWipe()
        {
            renderer.InitializeWipe();
        }

        public bool HasFocus()
        {
            return true;
        }

        public void Dispose()
        {
            Console.WriteLine("Shutdown video.");
        }

        public int WipeBandCount => renderer.WipeBandCount;
        public int WipeHeight => renderer.WipeHeight;

        public int MaxWindowSize => renderer.MaxWindowSize;

        public int WindowSize
        {
            get => renderer.WindowSize;
            set => renderer.WindowSize = value;
        }

        public bool DisplayMessage
        {
            get => renderer.DisplayMessage;
            set => renderer.DisplayMessage = value;
        }

        public int MaxGammaCorrectionLevel => renderer.MaxGammaCorrectionLevel;

        public int GammaCorrectionLevel
        {
            get => renderer.GammaCorrectionLevel;
            set => renderer.GammaCorrectionLevel = value;
        }
    }
}
