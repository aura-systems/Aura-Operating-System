/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Manages keyboard interactions
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;
using Cosmos.System;
using Cosmos.System.Graphics;
using System;

namespace Aura_OS.System.Input
{
    /// <summary>
    /// Manages keyboard for AuraOS. 
    /// </summary>
    public class KeyboardManager : IManager
    {
        /// <summary>
        /// Initializes the keyboard manager.
        /// </summary>
        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting keyboard manager...");

            CustomConsole.WriteLineInfo("Starting keyboard...");
            Cosmos.System.KeyboardManager.SetKeyLayout(new Cosmos.System.ScanMaps.USStandardLayout());
        }

        /// <summary>
        /// Updates the state of the keyboard, processing keys.
        /// </summary>
        public void Update()
        {
            KeyEvent k;
            bool IsKeyPressed = Cosmos.System.KeyboardManager.TryReadKey(out k);

            if (!IsKeyPressed) //if the user did not press a key return 
                return;

            if (Cosmos.System.KeyboardManager.ControlPressed && Cosmos.System.KeyboardManager.AltPressed && k.Key == ConsoleKeyEx.Delete)
            {
                Power.Reboot();
            }
            if (Cosmos.System.KeyboardManager.AltPressed && k.Key == ConsoleKeyEx.F4)
            {
                if (Explorer.WindowManager.FocusedApp != null)
                {
                    Explorer.WindowManager.FocusedApp.Window.Close.Click();
                }
            }
            else if (k.Key == ConsoleKeyEx.LWin)
            {
                Explorer.ShowStartMenu = !Explorer.ShowStartMenu;
            }
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return "Keyboard Manager";
        }
    }
}
