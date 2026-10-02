/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Display bitmap
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Kernel.System.Graphics;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandPicture : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandPicture(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to display a bitmap in a new window";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            try
            {
                string path = AuraPath.Resolve(arguments[0]);
                string name = Path.GetFileName(path);

                if (!File.Exists(path))
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "This file does not exist.");
                }

                byte[] bytes = File.ReadAllBytes(path);

                // GEN3-GAP(bmp): the gen3 BMP loader rejects top-down, bitfield and < 24 bpp images
                // with an exception, reported by the catch below.
                Bitmap bitmap = new Bitmap(bytes);

                if (bitmap.Width <= 0 || bitmap.Height <= 0)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Invalid bitmap.");
                }

                int width = name.Length * 8 + 50;

                if (width < bitmap.Width)
                {
                    width = (int)bitmap.Width + 6;
                }

                var app = new PictureApp(name, bitmap, width, (int)bitmap.Height + 26, 40, 40);
                app.MarkFocused();
                app.Initialize();
                app.Visible = true;

                Explorer.WindowManager.Applications.Add(app);
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
            Console.WriteLine(" - pic {source_file}");
        }
    }
}
