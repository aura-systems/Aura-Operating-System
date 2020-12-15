/*
* PROJECT:          Aura Operating System Development
* CONTENT:          https://github.com/Haydend/ConsoleDraw
* PROGRAMMERS:      Haydend <haydendunnicliffe@gmail.com>
*/

using ConsoleDraw.Windows.Base;
using System;

namespace ConsoleDraw.Inputs.Base
{
    public abstract class IInput
    {
        abstract public void Draw();

        public abstract void Select();
        public abstract void Unselect();

        public abstract void AddLetter(Char letter);
        public abstract void BackSpace();
        public abstract void CursorMoveLeft();
        public abstract void CursorMoveUp();
        public abstract void CursorMoveDown();
        public abstract void CursorMoveRight();
        public abstract void CursorToStart();
        public abstract void CursorToEnd();
        public abstract void Enter();
        public abstract void Tab();

        public int Xpostion;
        public int Ypostion;
        public int Width;
        public int Height;

        public bool Selectable { get; set; }

        public String ID;
        public Window ParentWindow;
    }
}
