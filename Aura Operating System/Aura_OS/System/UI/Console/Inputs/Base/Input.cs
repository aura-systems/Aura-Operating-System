/*
* PROJECT:          Aura Operating System Development
* CONTENT:          https://github.com/Haydend/ConsoleDraw
* PROGRAMMERS:      Haydend <haydendunnicliffe@gmail.com>
*/

using ConsoleDraw.Windows.Base;
using System;

namespace ConsoleDraw.Inputs.Base
{
    public class Input : IInput
    {
        public Input(int xPostion, int yPostion, int height, int width, Window parentWindow, String iD)
        {
            ParentWindow = parentWindow;
            ID = iD;

            Xpostion = xPostion;
            Ypostion = yPostion;

            Height = height;
            Width = width;
        }

        public override void AddLetter(Char letter) { }
        public override void BackSpace() { }
        public override void CursorMoveLeft() { }
        public override void CursorMoveRight() { }
        public override void CursorMoveUp() { }
        public override void CursorMoveDown() { }
        public override void CursorToStart() { }
        public override void CursorToEnd() { }
        public override void Enter() { }
        public override void Tab() {
            ParentWindow.MoveToNextItem();
        }
        
        public override void Unselect() { }
        public override void Select() { }
        public override void Draw() { }
    }
}
