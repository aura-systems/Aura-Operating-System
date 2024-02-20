/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Display bitmap
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Processes;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandPicture : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandPicture(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to display a bitmap in a new window.";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            try
            {
                string path = arguments[0];
                string name = Path.GetFileName(path);
                byte[] bytes = File.ReadAllBytes(Kernel.CurrentDirectory + path);
                Bitmap bitmap = new Bitmap(bytes);
                int width = name.Length * 8 + 50;

                if (width < bitmap.Width)
                {
                    width = (int)bitmap.Width + 1;
                }

                var app = new PictureApp(name, bitmap, width, (int)bitmap.Height + 20);
                app.MarkFocused();
                app.Initialize();

                Explorer.WindowManager.Applications.Add(app);
                app.zIndex = Explorer.WindowManager.GetTopZIndex() + 1;

                app.Visible = true;

                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();

                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return new ReturnInfo(this, ReturnCode.ERROR);
            }
        }


        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - cp {source_file/directory} {destination_file/directory}");
        }
    }
}
