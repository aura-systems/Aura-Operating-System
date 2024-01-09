/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Doom
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Cosmos.Core;
using Cosmos.Core.Memory;
using static ManagedDoom.CommandLineArgs;
using System;
using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.DMG;
using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.Utils;
using Aura_OS.System.Utils;
using ManagedDoom.UserInput;
using System.Reflection.Metadata;
using ManagedDoom;
using static Cosmos.HAL.PCIDevice;
using Aura_OS.System.Processing.Interpreter;
using ManagedDoom.Video;
using System.Drawing;
using System.Numerics;
using Cosmos.HAL.BlockDevice;
using ManagedDoom.Cosmos;

namespace Aura_OS.System.Processing.Application
{
    public class DoomApp : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Doom";

        private int fpsScale;
        private int frameCount;

        private Exception exception;

        private Doom doom;

        private CommandLineArgs args;
        private ManagedDoom.Config config;
        private GameContent content;

        private IVideo video;
        private IUserInput userInput;

        public DoomApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            video = new CosmosVideo(config, content);

            doom = new Doom(args, config, content, video, null);

            fpsScale = 1;
            frameCount = -1;
        }

        public override void UpdateApp()
        {
            try
            {
                frameCount++;

                if (frameCount % fpsScale == 0)
                {
                    if (doom.Update() == UpdateResult.Completed)
                    {
                        Stop();
                    }
                }

                DrawApp();
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (exception != null)
            {
                Stop();
            }
        }

        

        public void DrawApp()
        {
            var frameFrac = Fixed.FromInt(frameCount % fpsScale + 1) / fpsScale;
            
        }
    }
}
