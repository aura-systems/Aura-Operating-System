/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Debugger using UDP!
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Network.IPV4;
using Aura_OS.System.Network.IPV4.UDP;
using System;
using Aura_OS.System;
using System.Text;

namespace Aura_OS.Apps.System
{
    public class Debugger
    {

        public static Cosmos.Debug.Kernel.Debugger debugger = new Cosmos.Debug.Kernel.Debugger("aura", "debugger");

        UdpClient xClient;

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
            xClient = new UdpClient(port);
            xClient.Connect(ip, port);
            if (enabled)
            {
                Send("--- Aura Debugger v0.1 ---");
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
            

            if (result.Equals("on"))
            {
                Console.Clear();
                Console.WriteLine("Starting debugger at: " + Kernel.debugger.ip.ToString() + ":4224");
                Kernel.debugger.enabled = true;
                Kernel.debugger.Start();
                Console.WriteLine("Debugger started!");
            }
            else if (result.Equals("off"))
            {
                Console.Clear();
                Kernel.debugger.enabled = false;
                Console.WriteLine("Debugger disabled!");
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
            string[] item = { "Enable", "Disable" };
            int language = Aura_OS.System.Drawable.Menu.GenericMenu(item, Settings, x, y);
            if (language == 0)
            {
                return "on";
            }
            else if (language == 1)
            {
                return "off";
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
                Console.WriteLine("║ Enable or disable UDP debugger: (currently enabled)          ║");
            }
            else
            {
                Console.WriteLine("║ Enable or disable UDP debugger: (currently disabled)         ║");
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
}
