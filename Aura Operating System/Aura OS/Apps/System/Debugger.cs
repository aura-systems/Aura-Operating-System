/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Debugger using TCP!
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Network.IPV4;
using System;
using System.Text;
using Aura_OS.System.Network.IPV4.TCP;

namespace Aura_OS.Apps.System
{
    public class Debugger
    {

        public static Cosmos.Debug.Kernel.Debugger debugger = new Cosmos.Debug.Kernel.Debugger("aura", "debugger");

        TCPClient xClient;

        public bool enabled = false;

        int port;
        public Address ip;

        public Debugger(Address IP, int Port)
        {
            ip = IP;
            port = Port;
        }

        public void Start()
        {
            xClient = new TCPClient(4224);
            xClient.Connect(new Address(192, 168, 1, 12), 4224);
            if (enabled)
            {
                Send("--- Aura Debugger v0.2 ---");
                Send("Connected!");
                debugger.Send("Debugger started!");
            }
        }

        public void Send(string message)
        {
            debugger.Send(message);
            if (enabled)
            {
                xClient.Send(Encoding.ASCII.GetBytes("[" + Aura_OS.System.Time.TimeString(true, true, true) + "] - " + message));
            }
        }

        internal void Stop()
        {
            if (enabled)
            {
                xClient.Send(Encoding.ASCII.GetBytes("[" + Aura_OS.System.Time.TimeString(true, true, true) + "] - Properly disconnected by the operating system!"));
                xClient.Close();
                Kernel.debugger.enabled = false;
            }
            else
            {
                Console.WriteLine("Debugger already disabled!");
            }
        }
    }

    public class DebuggerSettings
    {

        /// <summary>
        /// Settings of the debugger
        /// </summary>
        public static void RegisterSetting()
        {

            string result;

            if (Kernel.debugger.enabled)
            {
                result = DispSettingsDialog(true);
            }
            else
            {
                result = DispSettingsDialog(false);
            }

            Console.Clear();

            //HAL.SaveScreen.PushLastScreen();

            if (result.Equals("on"))
            {
                Console.WriteLine("Starting debugger at: " + Kernel.debugger.ip.ToString() + ":4224");
                Kernel.debugger.enabled = true;
                Kernel.debugger.Start();
                Console.WriteLine("Debugger started!");
            }
            else if (result.Equals("off"))
            {
                if (!Kernel.debugger.enabled)
                {
                    Console.WriteLine("Debugger already disabled!");
                }
                else
                {
                    Kernel.debugger.Stop();
                    Console.WriteLine("Debugger disabled!");
                }
            }
            else if (result.Equals("changeip"))
            {

                string ip = Aura_OS.System.Drawable.Menu.DispDialogOneArg("Change IP address (currently " + Kernel.debugger.ip.ToString() +")", "IP Address: ");

                if (Aura_OS.System.Utils.Misc.IsIpv4Address(ip))
                {
                    Kernel.debugger.ip = Address.Parse(ip);
                }
                else
                {
                    Aura_OS.System.Drawable.Menu.DispErrorDialog("It is not an IP address!");
                    RegisterSetting();
                }
            }
        }

        public static int x_;
        public static int y_;

        /// <summary>
        /// Display settings dialog
        /// </summary>
        public static string DispSettingsDialog(bool enabled)
        {
            int x = (Kernel.AConsole.Width / 2) - (64 / 2);
            int y = (Kernel.AConsole.Height / 2) - (10 / 2);
            x_ = x;
            y_ = y;
            SettingMenu(x, y, enabled);
            string[] item = { "Enable", "Disable", "Change IP address" };
            int settings = Aura_OS.System.Drawable.Menu.GenericMenu(item, Settings, x, y);
            if (settings == 0)
            {
                return "on";
            }
            else if (settings == 1)
            {
                return "off";
            }
            else if (settings == 2)
            {
                return "changeip";
            }
            else
            {
                return "off";
            }
        }

        static int x_lang = Console.CursorLeft;
        static int y_lang = Console.CursorTop;

        static void SettingMenu(int x, int y, bool enabled)
        {
            Console.Clear();

            Console.BackgroundColor = ConsoleColor.DarkBlue;

            Console.SetCursorPosition(x, y);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x_lang, y_lang);
            Console.SetCursorPosition(x, y + 1);
            if (enabled)
            {
                Console.WriteLine("║ Enable or disable TCP debugger: (currently enabled)          ║");
            }
            else
            {
                Console.WriteLine("║ Enable or disable TCP debugger: (currently disabled)         ║");
            }
            Console.SetCursorPosition(x_lang, y_lang);
            Console.SetCursorPosition(x, y + 2);
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");

            Console.SetCursorPosition(x, y + 3);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y + 4);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y + 5);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y + 6);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y + 7);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y + 8);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y + 9);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.BackgroundColor = ConsoleColor.Black;
        }

        static void Settings()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            Console.SetCursorPosition(x_ + 2, y_ + 3);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x_ + 2, y_ + 4);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x_ + 2, y_ + 5);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x_ + 2, y_ + 6);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x_ + 2, y_ + 7);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);
        }
    }

    public class DebugConsole
    {
        public static void WriteLine(string text)
        {
            Console.WriteLine(text);
            Kernel.debugger.Send(text);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }
    } 
}
