/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Graphical terminal application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Users;
using Cosmos.System;
using System;
using System.Drawing;
using System.Collections.Generic;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics;

namespace Aura_OS.System.Processing.Application
{
    public struct Cell
    {
        public char Char;
        public Color ForegroundColor;
        public Color BackgroundColor;
    }

    public class Terminal : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Terminal";

        public System.Graphics.UI.GUI.Components.Console Console;

        List<string> Commands = new List<string>();
        private int CommandIndex = 0;
        public string Command = string.Empty;

        public Terminal(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            Window.Icon = ResourceManager.GetImage("16-terminal.bmp");

            CustomConsole.BootConsole = new(0, 0, width, height);

            BeforeCommand();
        }

        public override void UpdateApp()
        {
            if (Focused)
            {
                KeyEvent keyEvent = null;

                if (KeyboardManager.TryReadKey(out keyEvent))
                {
                    switch (keyEvent.Key)
                    {
                        case ConsoleKeyEx.Enter:
                            if (Console.ScrollMode)
                            {
                                break;
                            }
                            if (Command.Length > 0)
                            {
                                Console.mX -= Command.Length;

                                Console.ScrollMode = true;

                                Console.WriteLine(Command);

                                Kernel.CommandManager.Execute(Command);

                                Console.ScrollMode = false;

                                Commands.Add(Command);
                                CommandIndex = Commands.Count - 1;

                                Command = string.Empty;
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine();
                            }

                            BeforeCommand();
                            break;
                        case ConsoleKeyEx.Backspace:
                            if (Console.ScrollMode)
                            {
                                break;
                            }
                            if (Command.Length > 0)
                            {
                                Command = Command.Remove(Command.Length - 1);
                                Console.mX--;
                            }
                            break;
                        case ConsoleKeyEx.UpArrow:
                            if (KeyboardManager.ControlPressed)
                            {
                                Console.ScrollUp();
                            }
                            else
                            {
                                if (CommandIndex >= 0)
                                {
                                    Console.mX -= Command.Length;
                                    Command = Commands[CommandIndex];
                                    CommandIndex--;
                                    Console.mX += Command.Length;
                                }
                            }
                            break;
                        case ConsoleKeyEx.DownArrow:
                            if (KeyboardManager.ControlPressed)
                            {
                                Console.ScrollDown();
                            }
                            else
                            {
                                if (CommandIndex < Commands.Count - 1)
                                {
                                    Console.mX -= Command.Length;
                                    CommandIndex++;
                                    Command = Commands[CommandIndex];
                                    Console.mX += Command.Length;
                                }
                            }
                            break;
                        default:
                            if (Console.ScrollMode)
                            {
                                break;
                            }
                            if (char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || keyEvent.KeyChar == ' ')
                            {
                                Command += keyEvent.KeyChar;
                                Console.mX++;
                            }
                            break;
                    }
                }
            }

            DrawTerminal();
        }

        public void DrawTerminal()
        {
            Console.Draw();

            if (!Console.ScrollMode)
            {
                if (Command.Length > 0)
                {
                    int baseX = Console.mX - Command.Length;

                    for (int i = 0; i < Command.Length; i++)
                    {
                        Console.WriteByte(Command[i], Kernel.console.x + (baseX + i) * Kernel.font.Width, Kernel.console.y + Console.mY * Kernel.font.Height, Console.ForegroundColor);
                    }
                }

                Console.DrawCursor();
            }
        }

        #region BeforeCommand

        /// <summary>
        /// Display the line before the user input and set the console color.
        /// </summary>
        public void BeforeCommand()
        {
            Console.Foreground = ConsoleColor.Blue;
            Console.Write(UserLevel.TypeUser);

            Console.Foreground = ConsoleColor.Yellow;
            Console.Write(Kernel.userLogged);

            Console.Foreground = ConsoleColor.DarkGray;
            Console.Write("@");

            Console.Foreground = ConsoleColor.Blue;
            Console.Write(Kernel.ComputerName);

            Console.Foreground = ConsoleColor.Gray;
            Console.Write("> ");

            Console.Foreground = ConsoleColor.DarkGray;
            Console.Write(Kernel.CurrentDirectory + "~ ");

            Console.Foreground = ConsoleColor.White;
        }

        public string GetConsoleInfo()
        {
            return Kernel.canvas.Name() + " (" + Console.mCols + "x" + Console.mRows + " - " + global::System.Console.OutputEncoding.BodyName + ")";
        }

        #endregion
    }
}
