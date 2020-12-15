/*
* PROJECT:          Aura Operating System Development
* CONTENT:          https://github.com/Haydend/ConsoleDraw
* PROGRAMMERS:      Haydend <haydendunnicliffe@gmail.com>
*/

using System;
using ConsoleDraw.Inputs.Base;
using ConsoleDraw.Windows.Base;

namespace ConsoleDraw.Inputs
{
    public class ProgressBar : Input
    {
        public ConsoleColor BackgroundColour = ConsoleColor.Gray;
        public ConsoleColor BarColour = ConsoleColor.Black;

        private int percentageComplete;
        public int PercentageComplete {
            get { return this.percentageComplete; }
            set {
                if (value < 0 || value > 100) {
                    throw new ArgumentOutOfRangeException(String.Format("Percentage must be between 0 & 100, actual:{0}", value));     
                }
                this.percentageComplete = value;
                Draw();
            }
        }

        public ProgressBar(int percentageComplete, int x, int y, int height, int width, String iD, Window parentWindow) : base(x, y, height, width, parentWindow, iD)
        {
            Selectable = false;
            PercentageComplete = percentageComplete;
    }

        public override void Draw()
        {
            int widthCompleted = (int)Math.Round(Width * ((double)PercentageComplete / 100));
            int widthUncompleted = Width - widthCompleted;

            //WindowManager.DrawColourBlock(BackgroundColour, Xpostion, Ypostion, Xpostion + Height, Ypostion + Width);


            WindowManager.WirteText("".PadRight(widthCompleted, '█'), Xpostion, Ypostion, BarColour, BackgroundColour);
            WindowManager.WirteText("".PadRight(widthUncompleted, '▒'), Xpostion, Ypostion + widthCompleted, BarColour, BackgroundColour);
        }

    }
}
