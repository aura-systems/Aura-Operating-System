/*
* PROJECT:          Aura Operating System Development
* CONTENT:          UserLevel System
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using Aura_OS;

namespace Aura_OS.System.Users
{
    class UserLevel
    {

        /// <summary>
        /// Administrator
        /// </summary>
        /// <returns></returns>
        public static string Administrator
        {
            get
            {
                return "admin";
            }    
        }

        /// <summary>
        /// Standard
        /// </summary>
        /// <returns>standard</returns>
        public static string StandardUser
        {
            get
            {
                return "standard";
            }
        }

        /// <summary>
        /// User type
        /// </summary>
        /// <returns>User type char</returns>
        public static string TypeUser
        {
            get
            {
                if (Kernel.userLevelLogged == Administrator)
                {
                    return "#";
                }
                else
                {
                    return "$";
                }
            } 
        }

    }
}
