/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Manages mouse interactions, including left and right clicks, double clicks, and scroll wheel actions.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Cosmos.System;
using System;
using System.Drawing;

namespace Aura_OS.System.Input
{
    public class MouseManager
    {
        /// <summary>
        /// The maximum time interval in milliseconds to detect a double click.
        /// </summary>
        private const int doubleClickTime = 500;

        /// <summary>
        /// Timestamp of the last left mouse button click. Used for double click detection.
        /// </summary>
        private DateTime _lastLeftClickTime;

        /// <summary>
        /// Timestamp of the last right mouse button click. Used for double click detection.
        /// </summary>
        private DateTime _lastRightClickTime;

        /// <summary>
        /// Flag to indicate whether the left mouse button is currently pressed.
        /// </summary>
        private bool _leftButtonPressed;

        /// <summary>
        /// Flag to indicate whether the right mouse button is currently pressed.
        /// </summary>
        private bool _rightButtonPressed;

        /// <summary>
        /// Represents the top component currently under the mouse cursor.
        /// </summary>
        public Component TopComponent;

        /// <summary>
        /// Indicates whether the left mouse button is currently being held down.
        /// </summary>
        public bool IsLeftButtonDown;

        /// <summary>
        /// Indicates whether the right mouse button is currently being held down.
        /// </summary>
        public bool IsRightButtonDown;

        /// <summary>
        /// Initializes a new instance of the MouseManager class.
        /// </summary>
        public MouseManager()
        {
            _lastLeftClickTime = DateTime.MinValue;
            _lastRightClickTime = DateTime.MinValue;
            _leftButtonPressed = false;
            _rightButtonPressed = false;
            IsLeftButtonDown = false;
            IsRightButtonDown = false;
        }

        /// <summary>
        /// Updates the state of the mouse, processing clicks, double clicks, and scrolling.
        /// </summary>
        public void Update()
        {
            if (Cosmos.System.MouseManager.MouseState == MouseState.Left)
            {
                if (!_leftButtonPressed)
                {
                    ProcessLeftClick();
                    _leftButtonPressed = true;
                }
                IsLeftButtonDown = true;
            }
            else
            {
                if (_leftButtonPressed)
                {
                    _leftButtonPressed = false;
                }
                IsLeftButtonDown = false;
            }

            if (Cosmos.System.MouseManager.MouseState == MouseState.Right)
            {
                if (!_rightButtonPressed)
                {
                    ProcessRightClick();
                    _rightButtonPressed = true;
                }
                IsRightButtonDown = true;
            }
            else
            {
                if (_rightButtonPressed)
                {
                    _rightButtonPressed = false;
                }
                IsRightButtonDown = false;
            }

            HandleScroll();
        }

        /// <summary>
        /// Processes a left click action. Determines if the click is a single or double click based on the time elapsed since the last click.
        /// </summary>
        private void ProcessLeftClick()
        {
            if ((DateTime.Now - _lastLeftClickTime).TotalMilliseconds < doubleClickTime)
            {
                HandleLeftDoubleClick();
            }
            else
            {
                HandleLeftSingleClick();
            }

            _lastLeftClickTime = DateTime.Now;
        }

        /// <summary>
        /// Processes a right click action. Determines if the click is a single or double click based on the time elapsed since the last click.
        /// </summary>
        private void ProcessRightClick()
        {
            if ((DateTime.Now - _lastRightClickTime).TotalMilliseconds < doubleClickTime)
            {
                HandleRightDoubleClick();
            }
            else
            {
                HandleRightSingleClick();
            }

            _lastRightClickTime = DateTime.Now;
        }

        /// <summary>
        /// Handles the action for a single left click. Determines the top component for the click and manages the context menu's state.
        /// </summary>
        private void HandleLeftSingleClick()
        {
            if (TopComponent != null)
            {
                if (TopComponent.RightClick != null && TopComponent.RightClick.Opened)
                {
                    DetermineTopComponentForLeftClick();
                    TopComponent.RightClick.Opened = false;
                }
                TopComponent = null;
            }
            else
            {
                DetermineTopComponentForLeftClick();
            }
        }

        /// <summary>
        /// Handles the action for a double left click. Implement the desired double click behavior in this method.
        /// </summary>
        private void HandleLeftDoubleClick()
        {

        }

        /// <summary>
        /// Handles the action for a single right click. Determines the top component for the click and manages the context menu's state.
        /// </summary>
        private void HandleRightSingleClick()
        {
            if (TopComponent != null)
            {
                if (TopComponent.RightClick != null && TopComponent.RightClick.Opened)
                {
                    TopComponent.RightClick.Opened = false;
                }
                TopComponent = null;
            }
            else
            {
                DetermineTopComponentForRightClick();
            }
        }

        /// <summary>
        /// Handles the action for a double right click. Implement the desired double click behavior in this method.
        /// </summary>
        private void HandleRightDoubleClick()
        {

        }

        /// <summary>
        /// Handles the mouse scroll action. Resets the scroll delta after processing.
        /// </summary>
        private void HandleScroll()
        {
            if (Cosmos.System.MouseManager.ScrollDelta != 0)
            {
                Cosmos.System.MouseManager.ResetScrollDelta();
            }
        }

        /// <summary>
        /// Determines the top-level component for a left click. This method checks all applications and finds the one with the highest Z index under the mouse cursor.
        /// </summary>
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

        /// <summary>
        /// Determines the top-level component for a right click. This method checks all applications and finds the one with the highest Z index under the mouse cursor.
        /// </summary>
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

        /// <summary>
        /// Draws the context menu based on the position and state of the mouse.
        /// </summary>
        public void DrawRightClick()
        {
            if (TopComponent != null)
            {
                if (TopComponent.RightClick != null && TopComponent.RightClick.Opened)
                {
                    foreach (var entry in TopComponent.RightClick.Entries)
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
                    TopComponent.RightClick.Update();
                }
            }
        }
    }
}
