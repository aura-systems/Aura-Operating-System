/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Manages keyboard interactions
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Input;
using Aura_OS.System.Processing.Processes;
using Aura_OS.Processing;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Utils;
using CosmosKeyboard = Cosmos.Kernel.System.Input.KeyboardManager;

namespace Aura_OS.System.Input
{
    /// <summary>
    /// Manages keyboard for AuraOS. 
    /// </summary>
    /// <remarks>
    /// This class is the ONLY caller of CosmosKeyboard.TryReadKey (convention C11). It drains the
    /// kernel key queue once per frame on the UI thread, handles the global hotkeys and queues every
    /// other key for TryGetKey. Everything else in Aura reads keys through TryGetKey.
    /// Never use the Console input APIs (ReadKey, KeyAvailable, ReadLine, In) while the GUI runs: they move the
    /// whole kernel queue into tty1's private buffer, where TryReadKey no longer sees it.
    /// GEN3-GAP(console-input): Console input APIs drain KeyboardManager's queue into tty1.
    /// GEN3-GAP(sessions): never open virtual consoles/sessions, they permanently swallow Alt+F1..F12 (incl. Alt+F4).
    /// </remarks>
    public class KeyboardManager : Process, IManager
    {
        private static Queue<KeyEvent> keyEvents = new Queue<KeyEvent>();

        public KeyboardManager() : base(nameof(KeyboardManager), ProcessType.KernelComponent)
        {
        }

        /// <summary>
        /// Initializes the keyboard manager.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            CustomConsole.WriteLineInfo("Starting keyboard manager...");

            CustomConsole.WriteLineInfo("Starting keyboard...");
            if (KernelFeatures.Keyboard)
            {
                string code = GetSavedLayoutCode();

                if (!KeyboardLayouts.Set(code))
                {
                    CustomConsole.WriteLineWarning("Unknown keyboard layout '" + code + "', using US.");
                    KeyboardLayouts.Set("US");
                }

                CustomConsole.WriteLineInfo("Keyboard layout: " + KeyboardLayouts.CurrentCode);
            }
            else
            {
                CustomConsole.WriteLineWarning("Keyboard support is disabled in this kernel.");
            }

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        /// <summary>
        /// Returns the 'keyboardLayout' value of settings.ini when Aura is installed, else "US".
        /// </summary>
        private static string GetSavedLayoutCode()
        {
            string code = "US";

            if (Kernel.Installed)
            {
                try
                {
                    Settings config = new Settings(AuraPaths.SettingsIni);
                    string value = config.GetValue("keyboardLayout");

                    if (value != null && value != "null" && value.Trim().Length > 0)
                    {
                        code = value.Trim();
                    }
                }
                catch (Exception)
                {
                    code = "US";
                }
            }

            return code;
        }

        /// <summary>
        /// Updates the state of the keyboard, processing keys.
        /// </summary>
        public override void Update()
        {
            // C11: the ONLY call site of CosmosKeyboard.TryReadKey in Aura. Do not add another one.
            while (CosmosKeyboard.TryReadKey(out KeyEvent keyEvent))
            {
                // Physical Ctrl+Alt: AltGr also reports Control|Alt in Modifiers, the globals do not.
                if (CosmosKeyboard.ControlPressed && CosmosKeyboard.AltPressed && keyEvent.Key == Key.Delete)
                {
                    AuraPower.Reboot();
                    continue;
                }
                // The event snapshot is more accurate than AltPressed, since keys are drained once per frame.
                if ((keyEvent.Modifiers & ConsoleModifiers.Alt) != 0 && keyEvent.Key == Key.F4)
                {
                    var focusedApp = Explorer.WindowManager.FocusedApp;

                    if (focusedApp != null && focusedApp.Window != null && focusedApp.Window.Close != null && focusedApp.Window.Close.Click != null)
                    {
                        focusedApp.Window.Close.Click();
                    }
                    continue;
                }
                else if (keyEvent.Key == Key.LWin)
                {
                    // Only once logged in: DrawWindows/DetermineTopComponent also draw and click a visible
                    // start menu over the login screen, which would bypass the login.
                    if (Kernel.LoggedIn)
                    {
                        Explorer.ShowStartMenu = !Explorer.ShowStartMenu;
                    }
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
        /// Whether the key is that letter with Ctrl held (Ctrl+C), not a character AltGr types, which
        /// also reports Ctrl.
        /// </summary>
        public static bool IsShortcut(KeyEvent keyEvent, Key letter)
        {
            char c = keyEvent.KeyChar;
            bool typesCharacter = char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' ';

            return keyEvent.Key == letter && !typesCharacter
                && (CosmosKeyboard.ControlPressed || (keyEvent.Modifiers & ConsoleModifiers.Control) != 0);
        }

        /// <summary>
        /// Whether Shift was held with the key.
        /// </summary>
        public static bool IsShiftHeld(KeyEvent keyEvent)
        {
            return CosmosKeyboard.ShiftPressed || (keyEvent.Modifiers & ConsoleModifiers.Shift) != 0;
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return nameof(KeyboardManager);
        }
    }
}
