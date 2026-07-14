/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Keyboard layout switcher button (taskbar + login screen)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Input;
using MouseManager = Cosmos.Kernel.System.Mouse.MouseManager;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// Keyboard layout switcher: the button face is the active layout code
    /// ("US", "FR", ...) and both mouse buttons open a menu listing every scan
    /// map the gen3 kernel ships. Shared by the taskbar and the login screen.
    /// </summary>
    public class KeyboardLayoutButton : Button
    {
        public KeyboardLayoutButton(int x, int y, int width, int height) : base(KeyboardLayouts.CurrentCode, x, y, width, height)
        {
            RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 240, KeyboardLayouts.Codes.Length * RightClickEntry.ConstHeight);

            foreach (string code in KeyboardLayouts.Codes)
            {
                RightClickEntry entry = new(code + " - " + KeyboardLayouts.GetDisplayName(code), RightClick.Width, RightClick);
                entry.Click = new Action(() =>
                {
                    if (KeyboardLayouts.Set(code))
                    {
                        Text = KeyboardLayouts.CurrentCode;
                        MarkDirty();

                        if (Parent != null)
                        {
                            Parent.MarkDirty();
                        }
                    }
                });
                RightClick.AddEntry(entry);
            }

            // A layout switcher is expected to react to a plain left click too,
            // so both buttons open the same menu.
            Click = new Action(() =>
            {
                HandleRightClick();
            });
        }

        public override void Draw()
        {
            // Stays in sync when the layout is changed elsewhere (setkeyboardmap).
            Text = KeyboardLayouts.CurrentCode;

            base.Draw();
        }
    }
}
