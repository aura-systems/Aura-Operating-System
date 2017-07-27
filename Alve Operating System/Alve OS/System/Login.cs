using System;
using System.Collections.Generic;
using System.Text;
using usr = Alve_OS.System.Users;

namespace Alve_OS.System
{
    class Login
    {

        public static void Init()
        {
            usr.Users usr = new usr.Users();

            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.Write("Utilisateur > ");
                    var user = Console.ReadLine();
                    Console.WriteLine();
                    Console.Write("Mot de passe > ");
                    var psw = Console.ReadLine();
                    Console.WriteLine();

                    usr.Login(user, psw);

                    break;

                case "en_US":
                    Console.Write("Login > ");
                    var usera = Console.ReadLine();
                    Console.WriteLine();
                    Console.Write("Password > ");
                    var pswa = Console.ReadLine();
                    Console.WriteLine();

                    usr.Login(usera, pswa);

                    break;


            }
        }

    }
}
