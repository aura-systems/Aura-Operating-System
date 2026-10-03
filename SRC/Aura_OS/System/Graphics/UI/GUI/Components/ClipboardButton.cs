/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Clipboard history button (taskbar)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// The clipboard history: both mouse buttons open a menu of the texts copied last
    /// (TextClipboard), newest first, each on one line. Picking one copies it again, for Ctrl+V; the
    /// last entry empties the history.
    /// </summary>
    public class ClipboardButton : Button
    {
        private const int MenuWidth = 300;

        // The TextClipboard.Version the menu shows, -1 before it is built.
        private int _menuVersion = -1;

        public ClipboardButton(int x, int y, int width, int height) : base(Kernel.ResourceManager.GetIcon("16-paste.bmp"), x, y, width, height)
        {
            NoBackground = true;

            Click = new Action(() =>
            {
                HandleRightClick();
            });
        }

        public override void HandleRightClick()
        {
            if (_menuVersion != TextClipboard.Version)
            {
                BuildMenu();
            }

            base.HandleRightClick();
        }

        private void BuildMenu()
        {
            _menuVersion = TextClipboard.Version;

            if (RightClick != null)
            {
                if (Explorer.WindowManager.ContextMenu == RightClick)
                {
                    RightClick.Opened = false;
                    Explorer.WindowManager.ContextMenu = null;
                }

                RightClick.Dispose();
            }

            IReadOnlyList<string> history = TextClipboard.History;

            // Position is set by HandleRightClick when the menu opens.
            RightClick = new RightClick(0, 0, MenuWidth, Math.Max(1, history.Count + 1) * RightClickEntry.ConstHeight);

            if (history.Count == 0)
            {
                RightClick.AddEntry(new RightClickEntry("Nothing copied yet", MenuWidth, RightClick));
                return;
            }

            for (int i = 0; i < history.Count; i++)
            {
                string text = history[i];

                RightClickEntry entry = new(Summary(text), MenuWidth, RightClick);
                entry.Click = new Action(() =>
                {
                    TextClipboard.Copy(text);
                });
                RightClick.AddEntry(entry);
            }

            RightClickEntry clear = new("Clear history", MenuWidth, RightClick);
            clear.Click = new Action(() =>
            {
                TextClipboard.Clear();
            });
            RightClick.AddEntry(clear);
        }

        /// <summary>
        /// A text on one line as wide as the menu: its line breaks and tabs as spaces, cut with "...".
        /// </summary>
        private static string Summary(string text)
        {
            // The entries' text starts 4 pixels in.
            int columns = (MenuWidth - 8) / Kernel.font.Width;
            string trimmed = text.Trim();
            StringBuilder summary = new StringBuilder(columns + 1);

            for (int i = 0; i < trimmed.Length && summary.Length <= columns; i++)
            {
                char c = trimmed[i];
                summary.Append(c == '\r' || c == '\n' || c == '\t' ? ' ' : c);
            }

            if (summary.Length == 0)
            {
                return "(spaces)";
            }

            string line = summary.ToString();
            return line.Length > columns ? line.Substring(0, columns - 3) + "..." : line;
        }
    }
}
