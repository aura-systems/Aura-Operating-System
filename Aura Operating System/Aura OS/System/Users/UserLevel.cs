/*
* PROJECT:          Aura Operating System Development
* CONTENT:          UserLevel System
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Users
{
    class UserLevel
    {

        /// <summary>
        /// Administrator
        /// </summary>
        /// <returns></returns>
        public static string Administrator()
        {
            return "admin";
        }

        /// <summary>
        /// Standard
        /// </summary>
        /// <returns>standard</returns>
        public static string StandardUser()
        {
            return "standard";
        }

        /// <summary>
        /// User type
        /// </summary>
        /// <returns>User type char</returns>
        public static string TypeUser()
        {
            if(Kernel.userLevelLogged == Administrator())
            {
                return "#";
            }
            else
            {
                return "$";
            }
        }


        /// <summary>
        /// Method to apply type of user to actual session
        /// </summary>
        /// <param name="content">
        /// Line of user type in user file (.USR)
        /// </param>
        public static void LevelReader(string content)
        {
            if(content == Administrator())
            {
                Kernel.userLevelLogged = Administrator();
            } else
            {
                Kernel.userLevelLogged = StandardUser();
            }
        }

    }
}
