/*
* PROJECT:          Alve Operating System Development
* CONTENT:          UserLevel System
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;


namespace Alve_OS.System.Users
{
    class UserLevel
    {
        /// <summary>
        /// Administrateur
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
        /// Type d'utilisateur
        /// </summary>
        /// <returns>Le caractère correspondant au type d'utilisateur</returns>
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
        /// Méthode pour appliquer le type d'utilisateur à la session.
        /// </summary>
        /// <param name="content">
        /// Doit être la ligne avec le type d'utilisateur dans son fichier .USR
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
