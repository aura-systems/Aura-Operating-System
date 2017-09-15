/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Login Class
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using usr = Aura_OS.System.Users;

namespace Aura_OS.System
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
