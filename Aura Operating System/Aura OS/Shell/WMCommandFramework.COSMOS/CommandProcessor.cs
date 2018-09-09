using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework.COSMOS
{
    public class CommandProcessor
    {
        /// <summary>
        /// Whether debug information should be printed to the current terminal.
        /// </summary>
        public bool Debug
        {
            get => CommandUtils.DebugMode;
            set => CommandUtils.DebugMode = value;
        }
        
        /// <summary>
        /// The message to display in every command input prompt.
        /// </summary>
        public InputMessage[] Message
        {
            get => CommandUtils.InputMessage;
            set => CommandUtils.InputMessage = value;
        }

        /// <summary>
        /// The version of the current application.
        /// </summary>
        public ApplicationVersion Version
        {
            get => CommandUtils.Version;
            set => CommandUtils.Version = value;
        }

        private CommandInvoker invoker = null;

        /// <summary>
        /// Creates a new instance of the command processor class to allow terminal input.
        /// </summary>
        public CommandProcessor()
        {
            invoker = new CommandInvoker();
        }

        /// <summary>
        /// Gets the default internal invoker.
        /// </summary>
        /// <returns>Gets the internal invoker.</returns>
        public CommandInvoker GetInvoker()
        {
            return invoker;
        }

        /// <summary>
        /// Processes commands.
        /// </summary>
        public void Process()
        {
            for (int i = 0; i == Message.Length; i++)
            {
                int index = i--;
                var dat = Message[index];
                Console.ForegroundColor = dat.GetColor();
                Console.Write($"{dat.GetMessage()} ");
            }
            Console.Write(">");
            var input = Console.ReadLine();
            if (input != null && input != "")
                invoker.InvokeCommand(input);
        }

        /// <summary>
        /// Processes input and returns true if the input met the specified input result.
        /// </summary>
        /// <param name="text">The text to display next to the input.</param>
        /// <param name="result">The result the input will be compared with.</param>
        /// <param name="hideInput">If true the input will be hidden from the console.</param>
        /// <returns>If the input met the result.</returns>
        public bool ProcessInput(string text, string result, bool hideInput = false)
        {
            if (hideInput)
            {
                if (text.EndsWith(" "))
                    Console.Write(text);
                else
                    Console.Write($"{text} ");
                var input = new KeyInput();
                input.ProcessInput();
                Console.WriteLine();
                if (input.ToString() == result)
                    return true;
            }
            else
            {
                if (text.EndsWith(" "))
                    Console.Write(text);
                else
                    Console.Write(text + " ");
                var input = Console.ReadLine();
                if (input == result) return true;
            }
            return false;
        }

        /// <summary>
        /// Promps for a username and password then compares it with the values specified in the constructor.
        /// </summary>
        /// <param name="usernameResult">The correct value the username wants.</param>
        /// <param name="passwordResult">The correct value the password wants.</param>
        /// <returns>If the username and password where correct.</returns>
        public bool LoginPrompt(string usernameResult, string passwordResult)
        {
            var username = ProcessInput("Username:", usernameResult);
            var password = ProcessInput("Password:", passwordResult);
            if (username == true && password == true) return true;
            else if (username == true && password == false) return false;
            else if (username == false && password == true) return false;
            else return false;
        }
    }
    internal class KeyInput
    {
        private string x = "";

        public void ProcessInput()
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (x.Length == 1)
                    {
                        x = "";
                    }
                    else if (x.Length > 1)
                    {
                        x = x.Remove(x.Length - 1);
                    }
                }
                if (!(key.KeyChar.ToString() == "" || key.KeyChar.ToString() == null))
                    if (ValidChar(key.KeyChar))
                        x += key.KeyChar;
            }
        }

        public override string ToString()
        {
            if (x == null || x == "")
            {
                return "";
            }
            else
            {
                return x;
            }
        }

        private bool ValidChar(char c)
        {
            if ((!(CharTypes.IsWhitespace(c))) || CharTypes.IsLetter(c) || CharTypes.IsNumeral(c) || CharTypes.IsPunctuation(c) || CharTypes.IsSymbol(c))
                return true;
            return false;
        }

        private class CharTypes
        {
            public static bool IsLetter(char c)
            {
                if (c == 'A' || c == 'a')
                    return true;
                else if (c == 'B' || c == 'b')
                    return true;
                else if (c == 'C' || c == 'c')
                    return true;
                else if (c == 'D' || c == 'd')
                    return true;
                else if (c == 'E' || c == 'e')
                    return true;
                else if (c == 'F' || c == 'F')
                    return true;
                else if (c == 'G' || c == 'g')
                    return true;
                else if (c == 'H' || c == 'h')
                    return true;
                else if (c == 'I' || c == 'i')
                    return true;
                else if (c == 'J' || c == 'j')
                    return true;
                else if (c == 'K' || c == 'k')
                    return true;
                else if (c == 'L' || c == 'l')
                    return true;
                else if (c == 'M' || c == 'm')
                    return true;
                else if (c == 'N' || c == 'n')
                    return true;
                else if (c == 'O' || c == 'o')
                    return true;
                else if (c == 'P' || c == 'p')
                    return true;
                else if (c == 'Q' || c == 'q')
                    return true;
                else if (c == 'R' || c == 'r')
                    return true;
                else if (c == 'S' || c == 's')
                    return true;
                else if (c == 'T' || c == 't')
                    return true;
                else if (c == 'U' || c == 'u')
                    return true;
                else if (c == 'V' || c == 'v')
                    return true;
                else if (c == 'W' || c == 'w')
                    return true;
                else if (c == 'X' || c == 'x')
                    return true;
                else if (c == 'Y' || c == 'y')
                    return true;
                else if (c == 'Z' || c == 'z')
                    return true;
                else return false;
            }
            public static bool IsPunctuation(char c)
            {
                if (c == '!')
                    return true;
                else if (c == '.')
                    return true;
                else if (c == ',')
                    return true;
                else if (c == '?')
                    return true;
                else if (c == '\"')
                    return true;
                else if (c == '\'')
                    return true;
                else if (c == ':')
                    return true;
                else if (c == ';')
                    return true;
                else return false;
            }
            public static bool IsSymbol(char c)
            {
                if (c == '@')
                    return true;
                else if (c == '#')
                    return true;
                else if (c == '$')
                    return true;
                else if (c == '%')
                    return true;
                else if (c == '^')
                    return true;
                else if (c == '&')
                    return true;
                else if (c == '*')
                    return true;
                else if (c == '(')
                    return true;
                else if (c == ')')
                    return true;
                else if (c == '-')
                    return true;
                else if (c == '_')
                    return true;
                else if (c == '+')
                    return true;
                else if (c == '=')
                    return true;
                else if (c == '[')
                    return true;
                else if (c == ']')
                    return true;
                else if (c == '{')
                    return true;
                else if (c == '}')
                    return true;
                else if (c == '|')
                    return true;
                else if (c == '\\')
                    return true;
                else if (c == '<')
                    return true;
                else if (c == '>')
                    return true;
                else if (c == '?')
                    return true;
                else if (c == '/')
                    return true;
                else return false;
            }
            public static bool IsNumeral(char c)
            {
                if (c == '0')
                    return true;
                else if (c == '1')
                    return true;
                else if (c == '2')
                    return true;
                else if (c == '3')
                    return true;
                else if (c == '4')
                    return true;
                else if (c == '5')
                    return true;
                else if (c == '6')
                    return true;
                else if (c == '7')
                    return true;
                else if (c == '8')
                    return true;
                else if (c == '9')
                    return true;
                else return false;
            }
            public static bool IsWhitespace(char c)
            {
                if (c == ' ')
                    return true;
                return false;
            }
        }
    }
}