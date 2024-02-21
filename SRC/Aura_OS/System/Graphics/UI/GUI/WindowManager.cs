/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using System.Drawing;
using Aura_OS.System;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Processes;
using Rectangle = Aura_OS.System.Graphics.UI.GUI.Rectangle;
using Component = Aura_OS.System.Graphics.UI.GUI.Components.Component;
using Aura_OS.System.Graphics.UI.GUI.Components;
using System.ComponentModel;

namespace Aura_OS
{
    public class WindowManager : IManager
    {
        public static bool WindowMoving = false;

        public Application FocusedApp { get; set; }
        public RightClick ContextMenu { get; set; }

        public List<Application> Applications;
        public List<Rectangle> ClipRects;

        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting window manager...");

            Applications = new List<Application>();
            ClipRects = new List<Rectangle>();
        }

        public void AddComponent(Component component)
        {
            component.zIndex = ++highestZIndex;
            Component.Components.Add(component);
            InsertionSortByZIndex(Component.Components);
        }

        public void BringToFront(Component component)
        {
            if (component.zIndex < highestZIndex)
            {
                component.zIndex = ++highestZIndex;
                InsertionSortByZIndex(Component.Components);
            }

            foreach (Component child in component.Children)
            {
                BringToFront(child);
            }
        }

        private int highestZIndex = 0;

        private void InsertionSortByZIndex(List<Component> components)
        {
            for (int i = 1; i < components.Count; i++)
            {
                Component key = components[i];
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
            ClipRects.Clear();

            // Sort x index
            InsertionSortByZIndex(Component.Components);

            // Draw components
            for (int i = 0; i < Component.Components.Count; i++)
            {
                var component = Component.Components[i];

                if (component.IsRoot && component.Visible && (component.IsDirty() || component.ForceDirty))
                {
                    component.Draw();
                    component.MarkCleaned();
                    Rectangle.AddClipRect(component.GetRectangle());
                }

                foreach (var child in component.Children)
                {
                    if (child.Visible && (child.IsDirty() || child.ForceDirty))
                    {
                        child.Draw(child.Parent);
                        child.MarkCleaned();

                        var childRect = child.GetRectangle();
                        var parentRect = child.Parent.GetRectangle();
                        var top = parentRect.Top + childRect.Top;
                        var left = parentRect.Left + childRect.Left;
                        var realRect = new Rectangle(top, left, childRect.Height + top, childRect.Width + left);
                        Rectangle.AddClipRect(realRect);
                    }
                }
            }

            // Draw apps
            foreach (Application app in Applications)
            {
                if ((app.Running && app.Visible) && (app.IsDirty() || app.ForceDirty))
                {
                    app.Draw();
                    app.MarkCleaned();
                    Rectangle.AddClipRect(app.Window.GetRectangle());
                }
            }

            // Display on screen
            for (int i = 0; i < Component.Components.Count; i++)
            {
                var component = Component.Components[i];

                if (component.IsRoot)
                {
                    DrawComponentAndChildren(component);
                }
            }

            // Draw context menu
            RightClick contextMenu = Explorer.WindowManager.ContextMenu;
            if (contextMenu != null && contextMenu.Opened)
            {
                Explorer.WindowManager.ContextMenu.Draw();
                Kernel.Canvas.DrawImage(contextMenu.GetBuffer(), contextMenu.X, contextMenu.Y);
            }

            // Draw clip rects
            for (int i = 0; i < Explorer.WindowManager.ClipRects.Count; i++)
            {
                var tempRect = Explorer.WindowManager.ClipRects[i];
                DrawRect(tempRect.Left, tempRect.Top,
                         tempRect.Right - tempRect.Left + 1,
                         tempRect.Bottom - tempRect.Top + 1);
            }
        }

        public void DrawComponentAndChildren(Component component)
        {
            if (!component.Visible) return;

            if (component.HasTransparency)
            {
                Kernel.Canvas.DrawImageAlpha(component.GetBuffer(), component.X, component.Y);
            }
            else
            {
                Kernel.Canvas.DrawImage(component.GetBuffer(), component.X, component.Y);
            }
        }

        public void DrawRect(int x, int y, int width, int height)
        {
            Kernel.Canvas.DrawRectangle(Color.Green, x, y, width, height);
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return "Window Manager";
        }
    }
}
