/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Kernel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

#region using;

using System;
using Cosmos.System.FileSystem;
using Sys = Cosmos.System;
using Aura_OS.System;
using System.Collections.Generic;
using System.Text;
using Cosmos.System.ExtendedASCII;
using Aura_OS.System.Shell.cmdIntr;
using Aura_OS.System.Users;

#endregion

namespace Aura_OS
{
    public class Kernel : Sys.Kernel
    {
        #region Before Run

        protected override void BeforeRun()
        {
            try
            {

                #region Console Encoding Provider

                Encoding.RegisterProvider(CosmosEncodingProvider.Instance);

                if (Global.AConsole.Type == System.AConsole.ConsoleType.Graphical)
                {
                    Console.InputEncoding = Encoding.Unicode;
                    Console.OutputEncoding = Encoding.Unicode;
                }
                else
                {

                    Console.InputEncoding = Encoding.GetEncoding(437);
                    Console.OutputEncoding = Encoding.GetEncoding(437);
                }

                #endregion

                #region Commmand Manager Init

                CommandManager.RegisterAllCommands();

                #endregion

                System.CustomConsole.WriteLineInfo("Booting Aura Operating System...");

                #region Filesystem Init

                Sys.FileSystem.VFS.VFSManager.RegisterVFS(Global.vFS);
                if (Global.ContainsVolumes())
                {
                    System.CustomConsole.WriteLineOK("FileSystem Registration");
                }
                else
                {
                    System.CustomConsole.WriteLineError("FileSystem Registration");
                }

                #endregion

                System.CustomConsole.WriteLineOK("Aura successfully started!");

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                #region Installation Init

                Global.setup.InitSetup();

                /*if (SystemExists)
                {
                    if (!JustInstalled)
                    {

                        Settings config = new Settings(@"0:\System\settings.conf");
                        langSelected = config.GetValue("language");

                        #region Language

                        Lang.Keyboard.Init();

                        #endregion

                        Info.getComputerName();

                        System.Network.NetworkInterfaces.Init();

                        running = true;

                    }
                }
                else
                {
                    running = true;
                }*/

                Global.running = true;

                #endregion

                Global.boottime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            }
            catch (Exception ex)
            {
                Global.running = false;
                Crash.StopKernel(ex);
            }
        }

        #endregion

        #region Run

        protected override void Run()
        {
            try
            {

                while (Global.running)
                {
                    if (Global.Logged) //If logged
                    {
                        Global.BeforeCommand();

                        Global.AConsole.writecommand = true;

                        var cmd = Console.ReadLine();

                        CommandManager._CommandManger(cmd);
                    }
                    else
                    {
                        Login.LoginForm();
                    }
                }
            }
            catch (Exception ex)
            {
                Global.running = false;
                Crash.StopKernel(ex);
            }
        }

        #endregion
    }
}
