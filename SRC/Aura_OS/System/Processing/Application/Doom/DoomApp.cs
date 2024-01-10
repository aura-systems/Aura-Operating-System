/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Doom
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System.Graphics;
using DoomSharp.Core.Abstractions;
using DoomSharp.Core.Data;
using DoomSharp.Core;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using DoomSharp.Windows.Data;
using DoomSharp.Windows.ViewModels;

namespace Aura_OS.System.Processing.Application.Doom
{
    public class DoomApp : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Doom";


        public DoomApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            DoomGame.SetConsole(ConsoleViewModel.Instance);
            DoomGame.SetOutputRenderer(MainViewModel.Instance);
            WadFileCollection.Init(new WadStreamProvider());
        }

        public override void UpdateApp()
        {
            DoomGame.Instance.DoomLoop();
        }
    }
}
