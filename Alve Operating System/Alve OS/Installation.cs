using System;
using System.IO;

namespace Alve_OS
{
    class Installation
    {
        public static void Setup()
        {

                Console.WriteLine("Do you want to install Alve Operating System? (y or n)");
                ConsoleKeyInfo keypressed = Console.ReadKey();
                if (keypressed.Key == ConsoleKey.Y)
                {
                    string name;
                    string pass;
                    string language;

                    goto Setup;
                    Setup:
                    {
                        Console.Clear();
                        Console.Write("Name: ");
                        name = Console.ReadLine();
                        Console.Write("Pass: ");
                        pass = Console.ReadLine();
                        Console.WriteLine("Available language: en_US, fr_FR");
                        Console.Write("Language: ");
                        language = Console.ReadLine();
                        goto Install;
                    }
                    Install:
                    {
                        Console.WriteLine("Do you want to install Alve with '" + name + "' and '" + pass + "' ? (y or n)");
                        ConsoleKeyInfo keypressed1 = Console.ReadKey();
                        if (keypressed1.Key == ConsoleKey.Y)
                        {
                            Kernel.running = false;
                            Console.Clear();
                            Console.WriteLine("Installing ...");

                            int xx = Console.CursorLeft;
                            int yy = Console.CursorTop;

                            Kernel.FS.CreateDirectory("0:\\System");

                            Console.SetCursorPosition(0, 13);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("#####");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(xx, yy);

                            Kernel.FS.CreateDirectory("0:\\Users");

                            Console.SetCursorPosition(0, 13);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("###########");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(xx, yy);

                            Kernel.FS.CreateDirectory("0:\\Users\\" + name);
                            Kernel.FS.CreateDirectory("0:\\Users\\" + name + "\\Documents");
                            Kernel.FS.CreateDirectory("0:\\Users\\" + name + "\\Downloads");
                            Kernel.FS.CreateDirectory("0:\\Users\\" + name + "\\Images");
                            Console.SetCursorPosition(0, 13);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("################");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(xx, yy);

                            File.Create("0:\\System\\user");

                            Console.SetCursorPosition(0, 13);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("########################################");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(xx, yy);

                            File.Create("0:\\System\\pass");
                            File.Create("0:\\System\\language");

                            Console.SetCursorPosition(0, 13);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("######################################################");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(xx, yy);

                            File.WriteAllText("0:\\System\\user", name);
                            File.WriteAllText("0:\\System\\pass", pass);
                            File.WriteAllText("0:\\System\\language", language);
                            Console.SetCursorPosition(0, 13);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("################################################################################");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(xx, yy);

                            Console.SetCursorPosition(0, 11);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("                         The installation succeeded !                           ");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(xx, yy);

                            Console.SetCursorPosition(0, 12);
                            Console.WriteLine("                         Press any key to access Alve                           ");
                            Console.SetCursorPosition(xx, yy);

                            Console.ReadKey();
                            Console.Clear();
                            Kernel.running = true;
                        }
                        else
                        {
                            goto Setup;
                        }
                    }
                }
                else
                {
                    Console.Clear();
                }
        }
    }
}
