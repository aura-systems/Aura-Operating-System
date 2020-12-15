/*
* PROJECT:          Aura Operating System Development
* CONTENT:          https://github.com/Haydend/ConsoleDraw
* PROGRAMMERS:      Haydend <haydendunnicliffe@gmail.com>
*/

namespace ConsoleDraw.Windows.Base
{
    public abstract class IWindow
    {
        abstract public void ReDraw();

        public Window ParentWindow;
    }
}
