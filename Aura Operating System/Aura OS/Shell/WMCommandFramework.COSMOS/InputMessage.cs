using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework.COSMOS
{
    public class InputMessage
    {
        private ConsoleColor _color = Console.ForegroundColor;
        private string _message = "";

        /// <summary>
        /// Creates a standerd InputMessage string.
        /// </summary>
        public InputMessage()
        {
            _message = "CommandFramework";
        }

        /// <summary>
        /// Creates an InputMessage with the specified message.
        /// </summary>
        /// <param name="message">Text to display.</param>
        public InputMessage(string message)
        {
            _message = message;
        }

        /// <summary>
        /// Creates an InputMessage with the specified message.
        /// </summary>
        /// <param name="color">The color the message will display in.</param>
        /// <param name="message">The message to display.</param>
        public InputMessage(ConsoleColor color, string message)
        {
            _message = message;
            _color = color;
        }

        /// <summary>
        /// Gets the currently active color.
        /// </summary>
        /// <returns>The color this message uses.</returns>
        public ConsoleColor GetColor()
        {
            return _color;
        }

        /// <summary>
        /// Gets the current message.
        /// </summary>
        /// <returns>The text this message uses.</returns>
        public string GetMessage()
        {
            return _message;
        }

        /// <summary>
        /// Creates a new line.
        /// </summary>
        public static InputMessage NewLine
        {
            get => new InputMessage("\n");
        }

        /// <summary>
        /// Resets any color after the point where this class is used to whatever the console is currently using.
        /// </summary>
        public static InputMessage ResetColor
        {
            get => new InputMessage("");
        }

        /// <summary>
        /// Appends an InputMessage to the current InputMessage array.
        /// </summary>
        /// <param name="array">The array to append to.</param>
        /// <param name="value">The value to append.</param>
        /// <returns>The new InputMessage array with the appended value.</returns>
        public static InputMessage[] AppendMessage(InputMessage[] array, InputMessage value)
        {
            List<InputMessage> input = new List<InputMessage>(array.Length + 1);
            foreach (InputMessage message in array)
            {
                input.Add(message);
            }
            input.Add(value);
            return input.ToArray();
        }
    }
}
