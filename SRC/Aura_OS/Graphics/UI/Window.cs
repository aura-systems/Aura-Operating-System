/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Aura_OS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.System;

namespace Aura_OS.System.Graphics.UI
{
    public class Window : Element
    {
        public string Name;
        public Button Close;
        public Panel TopBar;

        public Window(string name, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Name = name;
            Close = new Button(Kernel.CloseNormal, X + Width - 20, Y + 5, 18, 18);
            TopBar = new Panel(Kernel.DarkBlue, X + 3, Y + 3, Width - 6, 18);
        }

        public override void Update()
        {
            base.Update();

            TopBar.X = X + 3;
            TopBar.Y = Y + 3;
            TopBar.Update();
            Kernel.canvas.DrawString(Name, Kernel.font, Kernel.WhiteColor, X + 2, Y + 2);
            Close.X = X + Width - 20;
            Close.Y = Y + 5;
            Close.Update();
        }
    }
}