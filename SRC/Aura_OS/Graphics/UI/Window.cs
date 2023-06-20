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

namespace Aura_OS.System.Graphics.UI
{
    public class Window : Element
    {
        public string Name;

        public Window(string name, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Name = name;
        }

        public override void Update()
        {
            base.Update();

            Kernel.canvas.DrawFilledRectangle(Kernel.DarkBlue, X + 3, Y + 3, Width - 5, 18);
            Kernel.canvas.DrawString(Name, Kernel.font, Kernel.WhiteColor, X + 2, Y + 2);
            Kernel.canvas.DrawImage(Kernel.CloseNormal, X + Width - 20, Y + 5);
        }
    }
}