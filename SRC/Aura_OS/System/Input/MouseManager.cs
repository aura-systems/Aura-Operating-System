/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Manages mouse interactions, including left and right clicks, double clicks, and scroll wheel actions.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;
using Cosmos.Kernel.System.Graphics;
using System;

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

        /// <summary>
        /// Last raw scroll delta sampled from the gen3 mouse manager.
        /// </summary>
        private int _lastRawScrollDelta;

        private Bitmap _cursorNormal;
        private Bitmap _cursorResizeHorizontal;
        private Bitmap _cursorResizeVertical;
        private Bitmap _cursorGrap;

        /// <summary>
        /// SVGA canvas when the device offers a host-composed hardware cursor;
        /// null means the cursor is blitted in software into Explorer.Screen.
        /// </summary>
        private SVGAII3DCanvas _hardwareCursorCanvas;

        /// <summary>
        /// Shape currently loaded in the hardware cursor slot.
        /// </summary>
        private CursorState _hardwareCursorState;

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
            _lastRawScrollDelta = 0;
            IsLeftButtonDown = false;
            IsRightButtonDown = false;

            CustomConsole.WriteLineInfo("Starting mouse...");
            Cosmos.Kernel.System.Mouse.MouseManager.SetScreenSize((int)Kernel.ScreenWidth, (int)Kernel.ScreenHeight);

            CursorState = CursorState.Normal;

            _cursorNormal = Kernel.ResourceManager.GetIcon("00-cursor.bmp");
            _cursorResizeHorizontal = Kernel.ResourceManager.GetIcon("00-resize-horizontal.bmp");
            _cursorResizeVertical = Kernel.ResourceManager.GetIcon("00-resize-vertical.bmp");
            _cursorGrap = Kernel.ResourceManager.GetIcon("00-grab.bmp");

            if (Kernel.Canvas is SVGAII3DCanvas svgaCanvas && svgaCanvas.HasHardwareCursor)
            {
                _hardwareCursorCanvas = svgaCanvas;
                _hardwareCursorState = CursorState.Normal;
                DefineHardwareCursor(CursorState.Normal);
                CustomConsole.WriteLineOK("SVGA hardware cursor enabled.");
            }

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        /// <summary>
        /// Updates the state of the mouse, processing clicks, double clicks, and scrolling.
        /// </summary>
        public override void Update()
        {
            // GEN3-GAP(mouse-state): gen3 removed the MouseState flags enum; only the
            // instantaneous LeftButton/RightButton booleans exist, so sample them once
            // per frame and edge-detect locally.
            bool leftButton = Cosmos.Kernel.System.Mouse.MouseManager.LeftButton;
            bool rightButton = Cosmos.Kernel.System.Mouse.MouseManager.RightButton;

            if (leftButton)
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

            DrawCursor((uint)Cosmos.Kernel.System.Mouse.MouseManager.X, (uint)Cosmos.Kernel.System.Mouse.MouseManager.Y);
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
        /// Handles the mouse scroll action.
        /// </summary>
        private void HandleScroll()
        {
            // GEN3-GAP(mouse-scroll): gen3 MouseManager.ScrollDelta is overwritten on
            // each device event and never cleared (ResetScrollDelta() is gone), so a
            // poller keeps seeing the last notch forever. Latch the raw value per frame
            // and only treat a changed value as new scroll input; identical consecutive
            // notches are lost until upstream adds a consume/reset API.
            int rawScrollDelta = Cosmos.Kernel.System.Mouse.MouseManager.ScrollDelta;

            if (rawScrollDelta != _lastRawScrollDelta)
            {
                _lastRawScrollDelta = rawScrollDelta;

                // No component consumes scroll input yet (parity with gen2, which only
                // reset the delta here).
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
                if (component.Visible && component.IsInside(Cosmos.Kernel.System.Mouse.MouseManager.X, Cosmos.Kernel.System.Mouse.MouseManager.Y))
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

        public void DrawCursor(uint x, uint y)
        {
            if (_hardwareCursorCanvas != null)
            {
                // The host composes the cursor: reload the shape only when it
                // changes, then just update the position registers.
                if (CursorState != _hardwareCursorState)
                {
                    DefineHardwareCursor(CursorState);
                    _hardwareCursorState = CursorState;
                }

                _hardwareCursorCanvas.SetCursor(true, (int)x, (int)y);
                return;
            }

            if (CursorState == CursorState.Normal)
            {
                Explorer.Screen.DrawImageAlpha(_cursorNormal, (int)x, (int)y);
            }
            else if (CursorState == CursorState.ResizeHorizontal)
            {
                Explorer.Screen.DrawImageAlpha(_cursorResizeHorizontal, (int)x - 23 / 2, (int)y);
            }
            else if (CursorState == CursorState.ResizeVertical)
            {
                Explorer.Screen.DrawImageAlpha(_cursorResizeVertical, (int)x, (int)y - 23 / 2);
            }
            else if (CursorState == CursorState.Grab)
            {
                Explorer.Screen.DrawImageAlpha(_cursorGrap, (int)x, (int)y);
            }
        }

        /// <summary>
        /// Loads the bitmap for the given cursor state into the hardware cursor
        /// slot. Hotspots mirror the offsets the software path draws with.
        /// </summary>
        private void DefineHardwareCursor(CursorState state)
        {
            Bitmap bitmap = _cursorNormal;
            int hotspotX = 0;
            int hotspotY = 0;

            if (state == CursorState.ResizeHorizontal)
            {
                bitmap = _cursorResizeHorizontal;
                hotspotX = 23 / 2;
            }
            else if (state == CursorState.ResizeVertical)
            {
                bitmap = _cursorResizeVertical;
                hotspotY = 23 / 2;
            }
            else if (state == CursorState.Grab)
            {
                bitmap = _cursorGrap;
            }

            // SVGA alpha cursors are premultiplied BGRA; the icons carry straight alpha.
            int[] raw = bitmap.RawData;
            int[] premultiplied = new int[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                uint pixel = (uint)raw[i];
                uint a = pixel >> 24;
                uint r = ((pixel >> 16) & 0xFF) * a / 255;
                uint g = ((pixel >> 8) & 0xFF) * a / 255;
                uint b = (pixel & 0xFF) * a / 255;
                premultiplied[i] = (int)((a << 24) | (r << 16) | (g << 8) | b);
            }

            _hardwareCursorCanvas.DefineAlphaCursor(hotspotX, hotspotY, (int)bitmap.Width, (int)bitmap.Height, premultiplied);
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
