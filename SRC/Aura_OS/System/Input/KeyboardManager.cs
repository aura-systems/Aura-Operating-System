/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Manages keyboard interactions
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using Cosmos.System;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Input
{
    /// <summary>
    /// Manages keyboard for AuraOS. 
    /// </summary>
    public class KeyboardManager : IManager
    {
        private static Queue<KeyEvent> keyEvents = new Queue<KeyEvent>();

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
            KeyEvent keyEvent;
            while (Cosmos.System.KeyboardManager.TryReadKey(out keyEvent))
            {
                if (Cosmos.System.KeyboardManager.ControlPressed && Cosmos.System.KeyboardManager.AltPressed && keyEvent.Key == ConsoleKeyEx.Delete)
                {
                    Power.Reboot();
                    continue;
                }
                if (Cosmos.System.KeyboardManager.AltPressed && keyEvent.Key == ConsoleKeyEx.F4)
                {
                    if (Explorer.WindowManager.FocusedApp != null)
                    {
                        Explorer.WindowManager.FocusedApp.Window.Close.Click();
                    }
                    continue;
                }
                else if (keyEvent.Key == ConsoleKeyEx.LWin)
                {
                    Explorer.ShowStartMenu = !Explorer.ShowStartMenu;
                    continue;
                }

                keyEvents.Enqueue(keyEvent);
            }
        }

        public static bool TryGetKey(out KeyEvent keyEvent)
        {
            if (keyEvents.Count > 0)
            {
                keyEvent = keyEvents.Dequeue();
                return true;
            }

            keyEvent = default;
            return false;
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
