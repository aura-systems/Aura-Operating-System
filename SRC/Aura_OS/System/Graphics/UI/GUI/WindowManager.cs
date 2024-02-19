/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS
{
    public class WindowManager
    {
        public List<Application> Applications;
        public List<System.Graphics.UI.GUI.Rectangle> ClipRects;

        private bool _isDirty = false;

        public WindowManager()
        {
            Applications = new List<Application>();
            ClipRects = new List<System.Graphics.UI.GUI.Rectangle>();
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

        public void AddComponent(System.Graphics.UI.GUI.Components.Component component)
        {
            component.zIndex = ++highestZIndex;
            System.Graphics.UI.GUI.Components.Component.Components.Add(component);
            InsertionSortByZIndex(System.Graphics.UI.GUI.Components.Component.Components);
        }

        public void BringToFront(System.Graphics.UI.GUI.Components.Component component)
        {
            if (component.zIndex < highestZIndex)
            {
                component.zIndex = ++highestZIndex;
                InsertionSortByZIndex(System.Graphics.UI.GUI.Components.Component.Components);
            }
        }

        private int highestZIndex = 0;

        private void InsertionSortByZIndex(List<System.Graphics.UI.GUI.Components.Component> components)
        {
            for (int i = 1; i < components.Count; i++)
            {
                System.Graphics.UI.GUI.Components.Component key = components[i];
                int j = i - 1;

                while (j >= 0 && components[j].zIndex > key.zIndex)
                {
                    components[j + 1] = components[j];
                    j = j - 1;
                }
                components[j + 1] = key;
            }
        }

        public void DrawWindows()
        {
            //ClipRects.Clear();

            InsertionSortByZIndex(System.Graphics.UI.GUI.Components.Component.Components);

            for (int i = 0; i < System.Graphics.UI.GUI.Components.Component.Components.Count; i++)
            {
                var component = System.Graphics.UI.GUI.Components.Component.Components[i];

                if (component.Visible && (component.IsDirty() || component.ForceDirty))
                {
                    component.Draw();
                    component.MarkCleaned();
                    //System.Graphics.UI.GUI.Rectangle.AddClipRect(component.GetRectangle());
                }
            }

            foreach (Application app in Applications)
            {
                if ((app.Running && app.Visible) && (app.IsDirty() || app.ForceDirty))
                {
                    app.Draw();
                    app.MarkCleaned();
                }
            }

            for (int i = 0; i < System.Graphics.UI.GUI.Components.Component.Components.Count; i++)
            {
                var component = System.Graphics.UI.GUI.Components.Component.Components[i];

                if (component.IsRoot)
                {
                    DrawComponentAndChildren(component);
                }
            }

            /*
            for (int i = 0; i < Explorer.WindowManager.ClipRects.Count; i++)
            {
                var tempRect = Explorer.WindowManager.ClipRects[i];
                DrawRect(tempRect.Left, tempRect.Top,
                         tempRect.Right - tempRect.Left + 1,
                         tempRect.Bottom - tempRect.Top + 1);
            }
            */
        }

        public void DrawComponentAndChildren(System.Graphics.UI.GUI.Components.Component component)
        {
            if (!component.Visible) return;

            // Dessiner le composant actuel
            if (component.HasTransparency)
            {
                Kernel.Canvas.DrawImageAlpha(component.GetBuffer(), component.X, component.Y);
            }
            else
            {
                Kernel.Canvas.DrawImage(component.GetBuffer(), component.X, component.Y);
            }

            // Dessiner récursivement tous les enfants
            foreach (var child in component.Children)
            {
                DrawComponentAndChildren(child);
            }
        }

        public void DrawRect(int x, int y, int width, int height)
        {
            Kernel.Canvas.DrawRectangle(Color.Green, x, y, width, height);
        }
    }
}
