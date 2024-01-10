//
// Copyright (C) 1993-1996 Id Software, Inc.
// Copyright (C) 2019-2020 Nobuaki Tanaka
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//



using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Aura_OS;
using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.Utils;
using Cosmos.System.Graphics;
using SFML.Graphics;
using SFML.System;

namespace ManagedDoom.SoftwareRendering
{
    public sealed class SfmlRenderer : IRenderer, IDisposable
    {
        private static double[] gammaCorrectionParameters = new double[]
        {
            1.00,
            0.95,
            0.90,
            0.85,
            0.80,
            0.75,
            0.70,
            0.65,
            0.60,
            0.55,
            0.50
        };

        private Config config;

        private RenderWindow sfmlWindow;
        private Palette palette;

        private int sfmlWindowWidth;
        private int sfmlWindowHeight;

        private DrawScreen screen;

        private int sfmlTextureWidth;
        private int sfmlTextureHeight;

        private byte[] sfmlTextureData;
        private SFML.Graphics.Texture sfmlTexture;
        private SFML.Graphics.Sprite sfmlSprite;
        private SFML.Graphics.RenderStates sfmlStates;

        private MenuRenderer menu;
        private ThreeDRenderer threeD;
        private StatusBarRenderer statusBar;
        private IntermissionRenderer intermission;
        private OpeningSequenceRenderer openingSequence;
        private AutoMapRenderer autoMap;
        private FinaleRenderer finale;

        private Patch pause;

        private int wipeBandWidth;
        private int wipeBandCount;
        private int wipeHeight;
        private byte[] wipeBuffer;

        public int X;
        public int Y;

        public SfmlRenderer(Config config, RenderWindow window, CommonResource resource)
        {
            try
            {
                Aura_OS.System.Processing.Application.DoomApp.debugger.Write("Initialize renderer: ");

                this.config = config;

                config.video_gamescreensize = Math.Clamp(config.video_gamescreensize, 0, MaxWindowSize);
                config.video_gammacorrection = Math.Clamp(config.video_gammacorrection, 0, MaxGammaCorrectionLevel);

                sfmlWindow = window;
                palette = resource.Palette;

                sfmlWindowWidth = (int)window.Size.X;
                sfmlWindowHeight = (int)window.Size.Y;

                if (config.video_highresolution)
                {
                    screen = new DrawScreen(resource.Wad, 640, 400);
                    sfmlTextureWidth = 512;
                    sfmlTextureHeight = 1024;
                }
                else
                {
                    screen = new DrawScreen(resource.Wad, 320, 200);
                    sfmlTextureWidth = 320;
                    sfmlTextureHeight = 200;
                }

                sfmlTextureData = new byte[4 * screen.Width * screen.Height];

                sfmlTexture = new SFML.Graphics.Texture((uint)sfmlTextureWidth, (uint)sfmlTextureHeight);
                sfmlSprite = new SFML.Graphics.Sprite(sfmlTexture);

                sfmlSprite.Position = new Vector2f(0, 0);
                sfmlSprite.Rotation = 90;
                var scaleX = (float)sfmlWindowWidth / screen.Width;
                var scaleY = (float)sfmlWindowHeight / screen.Height;
                sfmlSprite.Scale = new Vector2f(scaleY, -scaleX);

                sfmlStates = new RenderStates(BlendMode.None);

                menu = new MenuRenderer(resource.Wad, screen);
                threeD = new ThreeDRenderer(resource, screen, config.video_gamescreensize);
                statusBar = new StatusBarRenderer(resource.Wad, screen);
                intermission = new IntermissionRenderer(resource.Wad, screen);
                openingSequence = new OpeningSequenceRenderer(resource.Wad, screen, this);
                autoMap = new AutoMapRenderer(resource.Wad, screen);
                finale = new FinaleRenderer(resource, screen);

                pause = Patch.FromWad(resource.Wad, "M_PAUSE");

                var scale = screen.Width / 320;
                wipeBandWidth = 2 * scale;
                wipeBandCount = screen.Width / wipeBandWidth + 1;
                wipeHeight = screen.Height / scale;
                wipeBuffer = new byte[screen.Data.Length];

                palette.ResetColors(gammaCorrectionParameters[config.video_gammacorrection]);

                Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("OK");
            }
            catch (Exception e)
            {
                Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("Failed");
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        public void RenderApplication(DoomApplication app)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            if (app.State == ApplicationState.Opening)
            {
                openingSequence.Render(app.Opening);
            }
            else if (app.State == ApplicationState.DemoPlayback)
            {
                RenderGame(app.DemoPlayback.Game);
            }
            else if (app.State == ApplicationState.Game)
            {
                RenderGame(app.Game);
            }
            //Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("RenderApplication 1st part: {0} s", (float)(watch.ElapsedMilliseconds) / 1000);
            //watch.Restart();

            if (!app.Menu.Active)
            {
                if (app.State == ApplicationState.Game &&
                    app.Game.State == GameState.Level &&
                    app.Game.Paused)
                {
                    var scale = screen.Width / 320;
                    screen.DrawPatch(
                        pause,
                        (screen.Width - scale * pause.Width) / 2,
                        4 * scale,
                        scale);
                }
            }
            //Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("RenderApplication 2nd part: {0} s", (float)(watch.ElapsedMilliseconds) / 1000);
            //watch.Restart();
        }

        public void RenderMenu(DoomApplication app)
        {
            if (app.Menu.Active)
            {
                menu.Render(app.Menu);
            }
        }

        public void RenderGame(DoomGame game)
        {
            // var watch = System.Diagnostics.Stopwatch.StartNew();
            if (game.State == GameState.Level)
            {
                var consolePlayer = game.World.ConsolePlayer;
                var displayPlayer = game.World.DisplayPlayer;

                if (game.World.AutoMap.Visible)
                {
                    autoMap.Render(consolePlayer);
                    statusBar.Render(consolePlayer, true);
                }
                else
                {
                    // var watch2 = System.Diagnostics.Stopwatch.StartNew();
                    threeD.Render(displayPlayer);
                    // Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("threeD.Render(displayPlayer) {0} ms", watch2.ElapsedMilliseconds);
                    // watch2.Restart();
                    if (threeD.WindowSize < 8)
                    {
                        statusBar.Render(consolePlayer, true);
                    }
                    else if (threeD.WindowSize == ThreeDRenderer.MaxScreenSize)
                    {
                        statusBar.Render(consolePlayer, false);
                    }
                    // Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("statusBar.Render {0} ms", watch2.ElapsedMilliseconds);
                    // watch2.Restart();
                }

                if (config.video_displaymessage || ReferenceEquals(consolePlayer.Message, (string)DoomInfo.Strings.MSGOFF))
                {
                    if (consolePlayer.MessageTime > 0)
                    {
                        var scale = screen.Width / 320;
                        screen.DrawText(consolePlayer.Message, 0, 7 * scale, scale);
                    }
                }
            }
            else if (game.State == GameState.Intermission)
            {
                intermission.Render(game.Intermission);
            }
            else if (game.State == GameState.Finale)
            {
                finale.Render(game.Finale);
            }
            //Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("Render state {0}, {1} ms", game.State, watch.ElapsedMilliseconds);
        }

        public void Render(DoomApplication app)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            RenderApplication(app);
            //Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("RenderApplication: {0} s", (float)(watch.ElapsedMilliseconds) / 1000);
            //watch.Restart();
            RenderMenu(app);
            //Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("RenderMenu: {0} s", (float)(watch.ElapsedMilliseconds) / 1000);
            //watch.Restart();

            var colors = palette[0];
            if (app.State == ApplicationState.Game &&
                app.Game.State == GameState.Level)
            {
                colors = palette[GetPaletteNumber(app.Game.World.ConsolePlayer)];
            }
            else if (app.State == ApplicationState.Opening &&
                app.Opening.State == OpeningSequenceState.Demo &&
                app.Opening.DemoGame.State == GameState.Level)
            {
                colors = palette[GetPaletteNumber(app.Opening.DemoGame.World.ConsolePlayer)];
            }
            else if (app.State == ApplicationState.DemoPlayback &&
                app.DemoPlayback.Game.State == GameState.Level)
            {
                colors = palette[GetPaletteNumber(app.DemoPlayback.Game.World.ConsolePlayer)];
            }

            Display(colors);
        }

        public void RenderWipe(DoomApplication app, WipeEffect wipe)
        {
            RenderApplication(app);

            var scale = screen.Width / 320;
            for (var i = 0; i < wipeBandCount - 1; i++)
            {
                var x1 = wipeBandWidth * i;
                var x2 = x1 + wipeBandWidth;
                var y1 = Math.Max(scale * wipe.Y[i], 0);
                var y2 = Math.Max(scale * wipe.Y[i + 1], 0);
                var dy = (float)(y2 - y1) / wipeBandWidth;
                for (var x = x1; x < x2; x++)
                {
                    var y = (int)MathF.Round(y1 + dy * ((x - x1) / 2 * 2));
                    var copyLength = screen.Height - y;
                    if (copyLength > 0)
                    {
                        var srcPos = screen.Height * x;
                        var dstPos = screen.Height * x + y;
                        Array.Copy(wipeBuffer, srcPos, screen.Data, dstPos, copyLength);
                    }
                }
            }

            RenderMenu(app);

            Display(palette[0]);
        }

        public void InitializeWipe()
        {
            Array.Copy(screen.Data, wipeBuffer, screen.Data.Length);
        }

        void UpdteTextureDataWithColors(uint[] colors)
        {
            var screenData = screen.Data;
            var p = MemoryMarshal.Cast<byte, uint>(sfmlTextureData);
            for (var i = 0; i < p.Length; i++)
            {
                p[i] = colors[screenData[i]];
            }
        }

        private void Display(uint[] colors)
        {
            RenderWithColorsAndScreenDataUnmarshalled(screen.Data, colors);

            //Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("JS renderWithColorsAndScreenDataUnmarshalled: {0} s", watch.ElapsedMilliseconds);
            // watch.Restart();

            // DoomApplication.JSInProcessRuntime.InvokeVoid("renderWithColorsAndScreenData",  args);
        }

        void RenderWithColorsAndScreenDataUnmarshalled(byte[] screenData, uint[] colors)
        {
            int width = 320;
            int height = 200;
            // Assuming 'canvas' is an object of a class that provides a similar interface to HTML Canvas.
            var imageData = new byte[width * height * 4]; // Assuming an RGBA format
            int x = 0;
            int y = 0;

            for (var i = 0; i < (width * height) / 4; i += 1)
            {
                uint screenDataItem = screenData[i];
                int dataIndex;

                for (var mask = 0; mask <= 24; mask += 8)
                {
                    dataIndex = y * (width * 4) + x;

                    SetSinglePixel(imageData, dataIndex, colors, (screenDataItem >> mask) & 0xff);
                    if (y >= height - 1)
                    {
                        y = 0;
                        x += 4;
                    }
                    else
                    {
                        y += 1;
                    }
                    dataIndex = y * (width * 4) + x;
                }
            }

            DrawPixels(imageData, width, height, X, Y);
        }

        void SetSinglePixel(byte[] imageData, int dataIndex, uint[] colors, uint colorIndex)
        {
            // Extract the RGBA components from the color
            uint color = colors[colorIndex];

            imageData[dataIndex] = (byte)(color & 0xff);
            imageData[dataIndex + 1] = (byte)((color >> 8) & 0xff);
            imageData[dataIndex + 2] = (byte)((color >> 16) & 0xff);
            imageData[dataIndex + 3] = 255;
        }


        void DrawPixels(byte[] imageData, int width, int height, int dx, int dy, int? dirtyX = null, int? dirtyY = null, int? dirtyWidth = null, int? dirtyHeight = null)
        {
            dirtyX ??= 0;
            dirtyY ??= 0;
            dirtyWidth ??= width;
            dirtyHeight ??= height;

            int limitBottom = dirtyY.Value + dirtyHeight.Value;
            int limitRight = dirtyX.Value + dirtyWidth.Value;

            for (int y = dirtyY.Value; y < limitBottom; y++)
            {
                for (int x = dirtyX.Value; x < limitRight; x++)
                {
                    int pos = y * width + x;
                    byte red = imageData[pos * 4];
                    byte green = imageData[pos * 4 + 1];
                    byte blue = imageData[pos * 4 + 2];
                    byte alpha = imageData[pos * 4 + 3];

                    // Assuming 'MyCanvas' has a method 'SetPixel' to draw individual pixels.
                    // Replace 'System.Drawing.Color.FromArgb' with your color implementation if necessary.
                    Kernel.canvas.DrawPoint(System.Drawing.Color.FromArgb(alpha, red, green, blue), x + dx, y + dy);
                }
            }
        }


        private static int GetPaletteNumber(Player player)
        {
            var count = player.DamageCount;

            if (player.Powers[(int)PowerType.Strength] != 0)
            {
                // Slowly fade the berzerk out.
                var bzc = 12 - (player.Powers[(int)PowerType.Strength] >> 6);
                if (bzc > count)
                {
                    count = bzc;
                }
            }

            int palette;

            if (count != 0)
            {
                palette = (count + 7) >> 3;

                if (palette >= Palette.DamageCount)
                {
                    palette = Palette.DamageCount - 1;
                }

                palette += Palette.DamageStart;
            }
            else if (player.BonusCount != 0)
            {
                palette = (player.BonusCount + 7) >> 3;

                if (palette >= Palette.BonusCount)
                {
                    palette = Palette.BonusCount - 1;
                }

                palette += Palette.BonusStart;
            }
            else if (player.Powers[(int)PowerType.IronFeet] > 4 * 32 ||
                (player.Powers[(int)PowerType.IronFeet] & 8) != 0)
            {
                palette = Palette.IronFeet;
            }
            else
            {
                palette = 0;
            }

            return palette;
        }

        public void Dispose()
        {
            Aura_OS.System.Processing.Application.DoomApp.debugger.WriteLine("Shutdown renderer.");

            if (sfmlSprite != null)
            {
                sfmlSprite.Dispose();
                sfmlSprite = null;
            }

            if (sfmlTexture != null)
            {
                sfmlTexture.Dispose();
                sfmlTexture = null;
            }
        }

        public int WipeBandCount => wipeBandCount;
        public int WipeHeight => wipeHeight;

        public int MaxWindowSize
        {
            get
            {
                return ThreeDRenderer.MaxScreenSize;
            }
        }

        public int WindowSize
        {
            get
            {
                return threeD.WindowSize;
            }

            set
            {
                config.video_gamescreensize = value;
                threeD.WindowSize = value;
            }
        }

        public bool DisplayMessage
        {
            get
            {
                return config.video_displaymessage;
            }

            set
            {
                config.video_displaymessage = value;
            }
        }

        public int MaxGammaCorrectionLevel
        {
            get
            {
                return gammaCorrectionParameters.Length - 1;
            }
        }

        public int GammaCorrectionLevel
        {
            get
            {
                return config.video_gammacorrection;
            }

            set
            {
                config.video_gammacorrection = value;
                palette.ResetColors(gammaCorrectionParameters[config.video_gammacorrection]);
            }
        }
    }
}
