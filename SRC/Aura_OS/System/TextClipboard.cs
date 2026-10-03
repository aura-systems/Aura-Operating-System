/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Text clipboard
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;

namespace Aura_OS.System
{
    /// <summary>
    /// The text Ctrl+C copies (in a text box or a terminal) and Ctrl+V pastes, and the ones copied
    /// before it, for the taskbar's clipboard history. The desktop's copied file is Kernel.Clipboard.
    /// </summary>
    public static class TextClipboard
    {
        /// <summary>
        /// Texts kept; past it the oldest one goes.
        /// </summary>
        public const int MaxHistory = 10;

        private static readonly List<string> _history = new List<string>();

        /// <summary>
        /// The text Ctrl+V pastes, null when none was copied.
        /// </summary>
        public static string Text => _history.Count > 0 ? _history[0] : null;

        /// <summary>
        /// The texts copied, newest first: Text, then the ones before it.
        /// </summary>
        public static IReadOnlyList<string> History => _history;

        /// <summary>
        /// Changes with the history, for what shows it.
        /// </summary>
        public static int Version { get; private set; }

        /// <summary>
        /// Puts a text on the clipboard. One copied before goes back to the top rather than being
        /// kept twice; an empty text changes nothing.
        /// </summary>
        public static void Copy(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            for (int i = 0; i < _history.Count; i++)
            {
                if (_history[i] == text)
                {
                    _history.RemoveAt(i);
                    break;
                }
            }

            _history.Insert(0, text);

            if (_history.Count > MaxHistory)
            {
                _history.RemoveAt(_history.Count - 1);
            }

            Version++;
        }

        /// <summary>
        /// Empties the clipboard and its history.
        /// </summary>
        public static void Clear()
        {
            _history.Clear();
            Version++;
        }
    }
}
