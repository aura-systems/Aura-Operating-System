/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Login Class
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using usr = Alve_OS.System.Users;

namespace Alve_OS.System
{
    class Login
    {
        /// <summary>
        /// Init user system
        /// </summary>
        public static void Init()
        {
            usr.Users usr = new usr.Users();
            usr.Login();
        }
    }
}
