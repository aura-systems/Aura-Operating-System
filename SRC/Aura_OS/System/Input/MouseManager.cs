/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Mouse manager. Handle clicks
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aura_OS.System.Input
{
    public class MouseManager
    {
        public Component component;

        public bool LeftClicked = false;
        public bool RightClicked = false;

        public MouseManager() { }

        public void Update()
        {
            switch (Cosmos.System.MouseManager.MouseState)
            {
                case MouseState.Left:
                    if (!LeftClicked)
                    {
                        if (component != null)
                        {
                            if (component.RightClick != null && component.RightClick.Opened)
                            {
                                DetermineTopComponentForLeftClick();
                                component.RightClick.Opened = false;
                            }
                            component = null;
                        }
                        else
                        {
                            DetermineTopComponentForLeftClick();
                            LeftClicked = true;
                        }
                    }
                    RightClicked = false;
                    Kernel.Pressed = true;
                    break;
                case MouseState.Right:
                    if (!RightClicked)
                    {
                        if (component != null)
                        {
                            if (component.RightClick != null && component.RightClick.Opened)
                            {
                                component.RightClick.Opened = false;
                            }
                            component = null;
                        }
                        else
                        {
                            DetermineTopComponentForRightClick();
                            RightClicked = true;
                        }               
                    }
                    LeftClicked = false;
                    Kernel.Pressed = true;
                    break;
                case MouseState.None:
                    RightClicked = false;
                    LeftClicked = false;
                    Kernel.Pressed = false;
                    break;
            }
        }

        private void DetermineTopComponentForLeftClick()
        {
            Application topApplication = null;
            int topZIndex = -1;

            foreach (var app in Kernel.WindowManager.apps)
            {
                if (app.Visible && app.Window.IsInside((int)Cosmos.System.MouseManager.X, (int)Cosmos.System.MouseManager.Y) && app.zIndex > topZIndex)
                {
                    topApplication = app;
                    topZIndex = app.zIndex;
                }
            }

            if (topApplication != null)
            {
                topApplication.HandleLeftClick();
            }
            else
            {
                Kernel.Desktop.HandleLeftClick();
            }
        }

        private void DetermineTopComponentForRightClick()
        {
            Application topApplication = null;
            int topZIndex = -1;

            foreach (var app in Kernel.WindowManager.apps)
            {
                if (app.Visible && app.Window.IsInside((int)Cosmos.System.MouseManager.X, (int)Cosmos.System.MouseManager.Y) && app.zIndex > topZIndex)
                {
                    topApplication = app;
                    topZIndex = app.zIndex;
                }
            }

            if (topApplication != null)
            {
                topApplication.HandleRightClick();
            }
            else
            {
                Kernel.Desktop.HandleRightClick();
            }
        }

        public void DrawRightClick()
        {
            if (component != null)
            {
                if (component.RightClick != null && component.RightClick.Opened)
                {
                    foreach (var entry in component.RightClick.Entries)
                    {
                        if (entry.IsInside((int)Cosmos.System.MouseManager.X, (int)Cosmos.System.MouseManager.Y))
                        {
                            entry.BackColor = Kernel.DarkBlue;
                            entry.TextColor = Kernel.WhiteColor;
                        }
                        else
                        {
                            entry.BackColor = Color.LightGray;
                            entry.TextColor = Kernel.BlackColor;
                        }
                    }
                    component.RightClick.Update();
                }
            }
        }
    }
}
