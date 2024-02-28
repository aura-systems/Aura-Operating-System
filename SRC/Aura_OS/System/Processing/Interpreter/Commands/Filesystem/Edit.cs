/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Edit
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandEdit: ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandEdit(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to open a text file";
        }

        /// <summary>
        /// CommandEdit
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                string file = arguments[0];
                string path = Kernel.CurrentDirectory + file;

                if (File.Exists(path))
                {
                    var app = new EditorApp(path, 701, 600, 40, 40);
                    app.Initialize();
                    app.MarkFocused();
                    app.Visible = true;

                    Explorer.WindowManager.Applications.Add(app);
                    Kernel.ProcessManager.Start(app);

                    Explorer.Taskbar.UpdateApplicationButtons();
                }
                else
                {
                    Console.WriteLine("This file does not exist.");
                }
                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - edit {file}");
        }
    }
}