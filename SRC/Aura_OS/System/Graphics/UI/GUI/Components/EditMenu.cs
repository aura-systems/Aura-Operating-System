/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Right click menu of text (Cut, Copy, Paste...)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// The right click menu of a text box or a console: Cut, Copy, Paste... An entry that cannot be
    /// used when the menu opens (Copy with nothing selected) is gray and does nothing. The control
    /// sets it as its RightClick, calls Refresh before opening it, and CloseMenu then Dispose with itself.
    /// </summary>
    public class EditMenu : RightClick
    {
        private const int MenuWidth = 140;

        private readonly Func<bool>[] _enabled;

        /// <param name="texts">The entries' texts, top to bottom.</param>
        /// <param name="actions">What each entry does.</param>
        /// <param name="enabled">Whether each entry can be used now.</param>
        public EditMenu(string[] texts, Action[] actions, Func<bool>[] enabled) : base(0, 0, MenuWidth, texts.Length * RightClickEntry.ConstHeight)
        {
            _enabled = enabled;

            for (int i = 0; i < texts.Length; i++)
            {
                int index = i;

                RightClickEntry entry = new(texts[i], MenuWidth, this);
                entry.Click = new Action(() =>
                {
                    if (_enabled[index]())
                    {
                        actions[index]();
                    }
                });
                AddEntry(entry);
            }
        }

        /// <summary>
        /// Grays the entries that cannot be used now.
        /// </summary>
        public void Refresh()
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                Entries[i].TextColor = _enabled[i]() ? Kernel.BlackColor : Kernel.DarkGray;
                Entries[i].MarkDirty();
            }
        }

        /// <summary>
        /// Closes the menu if it is open.
        /// </summary>
        public void CloseMenu()
        {
            if (!Opened)
            {
                return;
            }

            Opened = false;

            if (Explorer.WindowManager.ContextMenu == this)
            {
                Explorer.WindowManager.ContextMenu = null;
            }
        }
    }
}
