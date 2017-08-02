/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Login Class
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

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

            usr.Login();
        }

    }
}
