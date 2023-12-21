/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Aura_OS
{
    public class WindowManager
    {
        public List<App> apps;
        private bool isDirty = false;

        public WindowManager()
        {
            apps = new List<App>();
        }

        public void MarkStackDirty()
        {
            isDirty = true;
        }

        public int GetTopZIndex()
        {
            int topZIndex = 0;
            for (int i = 0; i < apps.Count; i++)
            {
                if (apps[i].zIndex > topZIndex)
                {
                    topZIndex = apps[i].zIndex;
                }
            }
            return topZIndex;
        }

        public void UpdateWindowStack()
        {
            if (isDirty)
            {
                InsertionSort(apps);
                isDirty = false;
            }
        }

        private void InsertionSort(List<App> apps)
        {
            for (int i = 1; i < apps.Count; i++)
            {
                App currentApp = apps[i];
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
            for (int i = 0; i < apps.Count; i++)
            {
                apps[i].Focused = (i == apps.Count - 1);
            }
        }

        public void DrawWindows()
        {
            foreach (App app in apps)
            {
                if (app.Running)
                {
                    app.Update();
                }
            }
        }
    }
}
