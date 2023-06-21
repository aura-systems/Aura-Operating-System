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
        public Bitmap Icon;
        public string Name;
        public Button Close;
        public Panel TopBar;

        public Window(string name, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Icon = Kernel.programIco;
            Name = name;
            Close = new Button(Kernel.CloseNormal, X + Width - 20, Y + 5);
            TopBar = new Panel(Kernel.DarkBlue, X + 3, Y + 3, Width - 6, 18);
        }

        public override void Update()
        {
            Kernel.canvas.DrawFilledRectangle(Kernel.DarkGrayLight, X + 2, Y + 2, Width - 3, Height - 3);

            Kernel.canvas.DrawLine(Kernel.Gray, X, Y, X + Width, Y);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + 1, X + Width, Y + 1);
            Kernel.canvas.DrawLine(Kernel.Gray, X, Y, X, Y + Height);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X + 1, Y + 1, X + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y + Height, X + Width + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X + Width, Y, X + Width, Y + Height);

            TopBar.X = X + 3;
            TopBar.Y = Y + 3;
            TopBar.Update();
            Kernel.canvas.DrawString(Name, PCScreenFont.Default, Kernel.WhiteColor, X + 5, Y + 4);
            Close.X = X + Width - 20;
            Close.Y = Y + 5;
            Close.Update();
        }
    }
}