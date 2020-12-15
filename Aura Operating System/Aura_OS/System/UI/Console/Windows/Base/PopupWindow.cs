/*
* PROJECT:          Aura Operating System Development
* CONTENT:          https://github.com/Haydend/ConsoleDraw
* PROGRAMMERS:      Haydend <haydendunnicliffe@gmail.com>
*/

using System;

namespace ConsoleDraw.Windows.Base
{
    public class PopupWindow : Window
    {
        protected String Title;

        protected ConsoleColor TitleBarColour = ConsoleColor.DarkGray;
        protected ConsoleColor TitleColour = ConsoleColor.Black;

        public PopupWindow(String title, int postionX, int postionY, int width, int height, Window parentWindow)
            : base(postionX, postionY, width, height, parentWindow)
        {
            Title = title;
        }

        public override void ReDraw()
        {
            WindowManager.DrawColourBlock(TitleBarColour, PostionX, PostionY, PostionX + 1, PostionY + Width); //Title Bar
            WindowManager.WirteText(' ' + Title + ' ', PostionX, PostionY + 2, TitleColour, BackgroundColour);

            WindowManager.DrawColourBlock(BackgroundColour, PostionX + 1, PostionY, PostionX + Height, PostionY + Width); //Main Box
        }

    }
}
