/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Manages mouse interactions, including left and right clicks, double clicks, and scroll wheel actions.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Graphics;
using System;
using CosmosMouse = Cosmos.Kernel.System.Mouse.MouseManager;

namespace Aura_OS.System.Input
{
    public enum CursorState
    {
        Normal,
        ResizeHorizontal,
        ResizeVertical,
        Grab
    }

    /// <summary>
    /// Manages mouse and cursor position for AuraOS. 
    /// </summary>
    public class MouseManager : Process, IManager
    {
        /// <summary>
        /// Represents the top component currently under the mouse cursor.
        /// </summary>
        public Component TopComponent;

        /// <summary>
        /// Indicates whether the left mouse button is currently being held down.
        /// </summary>
        public bool IsLeftButtonDown;

        /// <summary>
        /// True from the update the left button went down until the next one only: what a press starts
        /// (moving or resizing a window, taking a slider) checks it, so the button held from elsewhere
        /// crossing a window border or a control starts nothing.
        /// </summary>
        public bool IsLeftButtonPressed;

        /// <summary>
        /// The root component (window, taskbar, start menu, menu...) drawn on top under the mouse
        /// when the left button went down, null over none.
        /// </summary>
        public Component LeftPressRoot;

        /// <summary>
        /// Indicates whether the right mouse button is currently being held down.
        /// </summary>
        public bool IsRightButtonDown;

        /// <summary>
        /// Focused component (set by click)
        /// </summary>
        public Component FocusedComponent;

        public static CursorState CursorState = CursorState.Normal;

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

        private Bitmap _cursorNormal;
        private Bitmap _cursorResizeHorizontal;
        private Bitmap _cursorResizeVertical;
        private Bitmap _cursorGrap;

        public MouseManager() : base(nameof(MouseManager), ProcessType.KernelComponent)
        {
        }

        /// <summary>
        /// Initializes the mouse manager and prepares buttons states.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            CustomConsole.WriteLineInfo("Starting mouse manager...");

            _lastLeftClickTime = DateTime.MinValue;
            _lastRightClickTime = DateTime.MinValue;
            _leftButtonPressed = false;
            _rightButtonPressed = false;
            IsLeftButtonDown = false;
            IsRightButtonDown = false;

            CustomConsole.WriteLineInfo("Starting mouse...");
            if (KernelFeatures.Mouse)
            {
                // Kernel.ScreenWidth/Height hold the UI size (the display divided by the scale), so the
                // pointer reports UI coordinates; the kernel default clamp is 1024x768.
                int width = (int)Kernel.ScreenWidth;
                int height = (int)Kernel.ScreenHeight;

                CosmosMouse.SetScreenSize(width, height);
                CosmosMouse.SetPosition(width / 2, height / 2);
            }
            else
            {
                CustomConsole.WriteLineWarning("Mouse support is disabled in this kernel.");
            }

            CursorState = CursorState.Normal;

            _cursorNormal = Kernel.ResourceManager.GetIcon("00-cursor.bmp");
            _cursorResizeHorizontal = Kernel.ResourceManager.GetIcon("00-resize-horizontal.bmp");
            _cursorResizeVertical = Kernel.ResourceManager.GetIcon("00-resize-vertical.bmp");
            _cursorGrap = Kernel.ResourceManager.GetIcon("00-grab.bmp");

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        /// <summary>
        /// Updates the state of the mouse, processing clicks, double clicks, and scrolling.
        /// </summary>
        public override void Update()
        {
            // GEN3-GAP(mouse): buttons are level-only (no press/release events), so sample them once
            // per frame and keep Aura's own edge detection below.
            bool leftButton = CosmosMouse.LeftButton;
            bool rightButton = CosmosMouse.RightButton;

            IsLeftButtonPressed = false;

            if (leftButton)
            {
                if (!_leftButtonPressed)
                {
                    // Before the click, which may close the menu that was pressed.
                    LeftPressRoot = DetermineTopRoot();
                    IsLeftButtonPressed = true;

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

            if (rightButton)
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

            DrawCursor(CosmosMouse.X, CosmosMouse.Y);
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
            Component topComponent = DetermineTopComponent();

            if (topComponent != null)
            {
                topComponent.HandleLeftClick();
            }
        }

        /// <summary>
        /// Handles the action for a double left click.
        /// </summary>
        private void HandleLeftDoubleClick()
        {
            Component topComponent = DetermineTopComponent();

            if (topComponent != null)
            {
                topComponent.HandleLeftDoubleClick();
            }
        }

        /// <summary>
        /// Handles the action for a single right click. Determines the top component for the click and manages the context menu's state.
        /// </summary>
        private void HandleRightSingleClick()
        {
            Component topComponent = DetermineTopComponent();

            if (topComponent != null)
            {
                topComponent.HandleRightClick();
            }
        }

        /// <summary>
        /// Handles the action for a double right click.
        /// </summary>
        private void HandleRightDoubleClick()
        {

        }

        /// <summary>
        /// Handles the mouse scroll action. Resets the scroll delta after processing.
        /// </summary>
        private void HandleScroll()
        {
            // The delta accumulates until reset (the driver never clears it).
            int d = CosmosMouse.ScrollDelta;
            if (d != 0)
            {
                CosmosMouse.ResetScrollDelta();
            }
        }

        /// <summary>
        /// Determines the top-level component. This method checks all applications and finds the one with the highest Z index under the mouse cursor.
        /// </summary>
        private Component DetermineTopComponent()
        {
            Component topComponent = null;
            int topZIndex = -1;

            void CheckComponent(Component component)
            {
                if (component.Visible && component.IsInside(CosmosMouse.X, CosmosMouse.Y))
                {
                    if (component.zIndex > topZIndex)
                    {
                        topComponent = component;
                        topZIndex = component.zIndex;
                    }
                }

                for (int i = 0; i < component.Children.Count; i++)
                {
                    Component child = component.Children[i];
                    CheckComponent(child);
                }
            }

            for (int i = 0; i < Component.Components.Count; i++)
            {
                Component component = Component.Components[i];
                CheckComponent(component);
            }

            return topComponent;
        }

        /// <summary>
        /// The visible root component under the mouse with the highest zIndex: the one drawn on top.
        /// </summary>
        private Component DetermineTopRoot()
        {
            Component top = null;

            for (int i = 0; i < Component.Components.Count; i++)
            {
                Component component = Component.Components[i];

                if (component.IsRoot && component.Visible && component.IsInside(CosmosMouse.X, CosmosMouse.Y)
                    && (top == null || component.zIndex > top.zIndex))
                {
                    top = component;
                }
            }

            return top;
        }

        /// <summary>
        /// Whether the left button went down in this update over that component, in the window (or
        /// other root component) drawn on top there.
        /// </summary>
        public bool IsLeftPressOn(Component component)
        {
            if (!IsLeftButtonPressed || !component.IsInside(CosmosMouse.X, CosmosMouse.Y))
            {
                return false;
            }

            Component root = component;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            return root == LeftPressRoot;
        }

        /// <summary>
        /// Whether that component's window (or other root component) is the one drawn on top under the
        /// mouse.
        /// </summary>
        public bool IsOnTop(Component component)
        {
            Component root = component;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            return root == DetermineTopRoot();
        }

        /// <summary>
        /// Clamps the cursor to the screen after a resolution change.
        /// </summary>
        public void ResizeToScreen()
        {
            if (KernelFeatures.Mouse)
            {
                int width = (int)Kernel.ScreenWidth;
                int height = (int)Kernel.ScreenHeight;

                CosmosMouse.SetScreenSize(width, height);
                CosmosMouse.SetPosition(Math.Min(CosmosMouse.X, width - 1), Math.Min(CosmosMouse.Y, height - 1));
            }
        }

        /// <summary>
        /// Draws the software cursor onto Explorer.Screen (Canvas.DrawImage blends it by alpha and clips at the screen edges).
        /// GEN3-GAP(hw-cursor): the IHardwareCursor facet is experimental and VMware-only; software cursor in phase 1.
        /// </summary>
        public void DrawCursor(int x, int y)
        {
            // A null dereference is a fatal #PF on gen3 (C6): skip the frame if the screen or an icon is missing.
            if (Explorer.Screen == null)
            {
                return;
            }

            if (CursorState == CursorState.Normal)
            {
                if (_cursorNormal != null)
                {
                    Explorer.Screen.DrawImage(_cursorNormal, x, y);
                }
            }
            else if (CursorState == CursorState.ResizeHorizontal)
            {
                if (_cursorResizeHorizontal != null)
                {
                    Explorer.Screen.DrawImage(_cursorResizeHorizontal, x - 23 / 2, y);
                }
            }
            else if (CursorState == CursorState.ResizeVertical)
            {
                if (_cursorResizeVertical != null)
                {
                    Explorer.Screen.DrawImage(_cursorResizeVertical, x, y - 23 / 2);
                }
            }
            else if (CursorState == CursorState.Grab)
            {
                if (_cursorGrap != null)
                {
                    Explorer.Screen.DrawImage(_cursorGrap, x, y);
                }
            }
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return nameof(MouseManager);
        }
    }
}
