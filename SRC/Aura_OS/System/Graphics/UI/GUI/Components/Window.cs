/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Window : Component
    {
        public Bitmap Icon;
        public string Name;
        public Button Close;
        public Button Minimize;
        public Panel TopBar;

        public bool Borders;

        public Window(int x, int y, int width, int height) : base(x, y, width, height)
        {
            Borders = false;
        }

        public Window(string name, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Icon = ResourceManager.GetImage("16-program.bmp");
            Name = name;
            Close = new Button(ResourceManager.GetImage("16-close.bmp"), X + Width - 20, Y + 5);
            Close.NoBackground = true;
            Close.NoBorder = true;
            Minimize = new Button(ResourceManager.GetImage("16-minimize.bmp"), X + Width - 40, Y + 5);
            Minimize.NoBackground = true;
            Minimize.NoBorder = true;
            TopBar = new Panel(Kernel.DarkBlue, Kernel.Pink, X + 3, Y + 3, Width - 5, 18);
            Borders = true;
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

            if (Borders)
            {
                TopBar.X = X + 3;
                TopBar.Y = Y + 3;
                TopBar.Update();
                Kernel.canvas.DrawString(Name, PCScreenFont.Default, Kernel.WhiteColor, X + 5, Y + 4);
                Close.X = X + Width - 20;
                Close.Y = Y + 5;
                Close.Update();
                Minimize.X = X + Width - 40;
                Minimize.Y = Y + 5;
                Minimize.Update();
            }
        }
    }
}