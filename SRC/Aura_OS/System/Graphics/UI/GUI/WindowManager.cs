/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS
{
    public class WindowManager
    {
        public List<Application> Applications;
        public List<Rect> ClipRects;

        private bool _isDirty = false;

        public WindowManager()
        {
            Applications = new List<Application>();
            ClipRects = new List<Rect>();
        }

        public void MarkStackDirty()
        {
            _isDirty = true;
        }

        public int GetTopZIndex()
        {
            int topZIndex = 0;
            for (int i = 0; i < Applications.Count; i++)
            {
                if (Applications[i].zIndex > topZIndex)
                {
                    topZIndex = Applications[i].zIndex;
                }
            }
            return topZIndex;
        }

        public void UpdateWindowStack()
        {
            if (_isDirty)
            {
                InsertionSort(Applications);
                _isDirty = false;
            }
        }

        private void InsertionSort(List<Application> apps)
        {
            for (int i = 1; i < apps.Count; i++)
            {
                Application currentApp = apps[i];
                int j = i - 1;

                while (j >= 0 && apps[j].zIndex > currentApp.zIndex)
                {
                    apps[j + 1] = apps[j];
                    j = j - 1;
                }
                apps[j + 1] = currentApp;
            }

            UpdateFocusStatus();
        }

        public void UpdateFocusStatus()
        {
            for (int i = 0; i < Applications.Count; i++)
            {
                Applications[i].Focused = (i == Applications.Count - 1);
            }
        }

        public void DrawWindows()
        {
            /*
            ClipRects.Clear();

            foreach (var currentWindow in Applications)
            {
                Rect tempRect = new Rect(currentWindow.Y, currentWindow.X,
                                         currentWindow.Y + currentWindow.Height - 1,
                                         currentWindow.X + currentWindow.Width - 1);
                Rect.AddClipRect(tempRect);
            }
            */

            foreach (Application app in Applications)
            {
                if (app.Running && app.Visible)
                {
                    app.Draw();
                    app.Display();
                }
            }

            /*
            foreach (var tempRect in Explorer.WindowManager.ClipRects)
            {
                DrawRect(tempRect.Left, tempRect.Top,
                                       tempRect.Right - tempRect.Left + 1,
                                       tempRect.Bottom - tempRect.Top + 1);
            }
            */
        }

        public void DrawRect(int x, int y, int width, int height)
        {
            Kernel.canvas.DrawRectangle(Color.Green, x, y, width, height);
        }
    }
}
