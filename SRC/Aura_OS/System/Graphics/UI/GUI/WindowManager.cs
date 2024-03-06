/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using System.Drawing;
using Aura_OS.System;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Rectangle = Aura_OS.System.Graphics.UI.GUI.Rectangle;
using Component = Aura_OS.System.Graphics.UI.GUI.Components.Component;
using Aura_OS.System.Utils;

namespace Aura_OS
{
    public class WindowManager : IManager
    {
        public static bool WindowMoving = false;

        public Application FocusedApp { get; set; }
        public RightClick ContextMenu { get; set; }
        public byte WindowsTransparency { get; set; }
        public byte TaskbarTransparency { get; set; }

        public List<Application> Applications;
        public List<Rectangle> ClipRects;

        private int _highestZIndex = -1;
        private DirectBitmap _screen;

        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting window manager...");

            Applications = new List<Application>();
            ClipRects = new List<Rectangle>();

            if (Kernel.Installed)
            {
                Settings config = new Settings(@"0:\System\settings.ini");
                byte windowsTransparency = byte.Parse(config.GetValue("windowsTransparency"));
                byte taskbarTransparency = byte.Parse(config.GetValue("taskbarTransparency"));
                WindowsTransparency = windowsTransparency;
                TaskbarTransparency = taskbarTransparency;
            }
            else
            {
                WindowsTransparency = 0xFF;
                TaskbarTransparency = 0xFF;
            }
        }

        public void AddComponent(Component component)
        {
            component.zIndex = ++_highestZIndex;
            Component.Components.Add(component);
            InsertionSortByZIndex(Component.Components);
        }

        public void BringToFront(Component component)
        {
            if (component.zIndex < _highestZIndex)
            {
                component.zIndex = ++_highestZIndex;
                InsertionSortByZIndex(Component.Components);
            }

            foreach (Component child in component.Children)
            {
                BringToFront(child);
            }
        }

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
            if (Kernel.GuiDebug)
            {
                ClipRects.Clear();
            }

            // Sort x index
            InsertionSortByZIndex(Component.Components);

            // Draw components
            for (int i = 0; i < Component.Components.Count; i++)
            {
                Component component = Component.Components[i];

                if (component.IsRoot && component.Visible)
                {
                    if (component.IsDirty() || component.ForceDirty)
                    {
                        component.Draw();
                        component.MarkCleaned();

                        if (Kernel.GuiDebug)
                        {
                            Rectangle.AddClipRect(component.GetRectangle());
                        }
                    }
                }

                for (int j = 0; j < component.Children.Count; j++)
                {
                    Component child = component.Children[j];

                    if (child.Visible && (child.IsDirty() || child.ForceDirty))
                    {
                        child.Draw(child.Parent);
                        child.MarkCleaned();

                        if (Kernel.GuiDebug)
                        {
                            Rectangle childRect = child.GetRectangle();
                            Rectangle parentRect = child.Parent.GetRectangle();
                            int top = parentRect.Top + childRect.Top;
                            int left = parentRect.Left + childRect.Left;
                            Rectangle realRect = new Rectangle(top, left, childRect.Height + top, childRect.Width + left);
                            Rectangle.AddClipRect(realRect);
                        }
                    }
                }

                if (component.IsRoot && component.Visible)
                {
                    DrawComponentAndChildren(component);
                }
            }

            // Draw apps
            for (int i = 0; i < Applications.Count; i++)
            {
                Application app = Applications[i];

                if ((app.Running && app.Visible) && (app.IsDirty() || app.ForceDirty))
                {
                    app.Draw();
                    app.MarkCleaned();

                    if (Kernel.GuiDebug)
                    {
                        Rectangle.AddClipRect(app.Window.GetRectangle());
                    }
                }
            }

            // Draw context menu
            RightClick contextMenu = Explorer.WindowManager.ContextMenu;
            if (contextMenu != null && contextMenu.Opened)
            {
                Explorer.WindowManager.ContextMenu.Update();
                Explorer.WindowManager.ContextMenu.Draw();
                _screen.DrawImage(contextMenu.GetBuffer(), contextMenu.X, contextMenu.Y);
            }

            if (Kernel.GuiDebug)
            {
                // Draw clip rects
                for (int i = 0; i < Explorer.WindowManager.ClipRects.Count; i++)
                {
                    var tempRect = Explorer.WindowManager.ClipRects[i];
                    DrawRect(tempRect.Left, tempRect.Top,
                             tempRect.Right - tempRect.Left + 1,
                             tempRect.Bottom - tempRect.Top + 1);
                }
            }  
        }

        public void DrawComponentAndChildren(Component component)
        {
            if (component is Window)
            {
                _screen.DrawImageAlpha(component.GetBuffer(), component.X, component.Y, WindowsTransparency);
            }
            else if (component is Taskbar && TaskbarTransparency != 0xFF)
            {
                _screen.DrawImageAlpha(component.GetBuffer(), component.X, component.Y, TaskbarTransparency);
            }
            else
            {
                _screen.DrawImage(component.GetBuffer(), component.X, component.Y);
            }
        }

        private int green = Color.Green.ToArgb();

        public void DrawRect(int x, int y, int width, int height)
        {
            _screen.DrawRectangle(green, x, y, width, height);
        }

        public void SetScreen(DirectBitmap Screen)
        {
            _screen = Screen;
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
