/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Keyboard layout switcher button (taskbar + login screen)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Input;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// Keyboard layout switcher: the button face is the active layout code
    /// ("US", "FR", ...) and both mouse buttons open a menu listing every layout
    /// of KeyboardLayouts. Picking one activates it and saves it in settings.ini.
    /// Shared by the taskbar and the login screen; the host adds it to its
    /// children and calls Update/Draw like any Button.
    /// </summary>
    public class KeyboardLayoutButton : Button
    {
        public KeyboardLayoutButton(int x, int y, int width, int height) : base(KeyboardLayouts.CurrentCode, x, y, width, height)
        {
            // Position is set by HandleRightClick when the menu opens.
            RightClick = new RightClick(0, 0, 240, KeyboardLayouts.Codes.Length * RightClickEntry.ConstHeight);

            foreach (string code in KeyboardLayouts.Codes)
            {
                string layoutCode = code;

                RightClickEntry entry = new(layoutCode + " - " + KeyboardLayouts.GetDisplayName(layoutCode), RightClick.Width, RightClick);
                entry.Click = new Action(() =>
                {
                    if (KeyboardLayouts.Set(layoutCode))
                    {
                        KeyboardLayouts.Persist();
                        SyncText();
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

        /// <summary>
        /// Refreshes the face when the layout was changed elsewhere (setkeyboardmap).
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (Text != KeyboardLayouts.CurrentCode)
            {
                SyncText();
            }
        }

        public override void Draw()
        {
            // Stays in sync when the layout is changed elsewhere (setkeyboardmap).
            Text = KeyboardLayouts.CurrentCode;

            base.Draw();
        }

        private void SyncText()
        {
            Text = KeyboardLayouts.CurrentCode;
            MarkDirty();

            if (Parent != null)
            {
                Parent.MarkDirty();
            }
        }
    }
}
