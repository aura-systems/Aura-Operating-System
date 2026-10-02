/*
* PROJECT:          Aura Operating System Development
* CONTENT:          User class
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.IO;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Security;
using System.Collections.Generic;
using System;

namespace Aura_OS.System.Users
{
    class Users
    {

        #region UserDirs
        public static void InitUserDirs(string user)
        {
            if (user == "root")
            {
                return;
            }
            else
            {
                string[] DefaultDirectories =
                {
                    AuraPaths.UsersDir + user + "/Desktop",
                    AuraPaths.UsersDir + user + "/Documents",
                    AuraPaths.UsersDir + user + "/Downloads",
                    AuraPaths.UsersDir + user + "/Music",
                };
                foreach (string dirs in DefaultDirectories)
                    if (!Directory.Exists(dirs))
                        Directory.CreateDirectory(dirs);
            }
        }
        #endregion UserDirs

        public static string[] users = Array.Empty<string>();
        static string[] reset = Array.Empty<string>();
        static List<string> usersfile = new List<string>();

        /// <summary>
        /// Method to create an user.
        /// </summary>
        public void Create(string username, string password, string type = "standard")
        {
            try
            {
                password = Sha256.hash(password);
                LoadUsers();
                if (GetUser("user").StartsWith(username))
                {
                    Console.WriteLine(username + " already exists.");
                    return;
                }
                PutUser("user:" + username, password + "|" + type);
                PushUsers();
                Console.WriteLine(username + " has been created!");

                InitUserDirs(username);
                Console.WriteLine("Personal directories has been created!");
            }
            catch
            {
                Console.WriteLine("Error while creating user.");
            }
        }

        /// <summary>
        /// Method to remove an user.
        /// </summary>
        public void Remove(string username)
        {
            if (GetUser("user").StartsWith(username))
            {
                LoadUsers();
                DeleteUser(username);
                //Directory.Delete(AuraPaths.UsersDir + username, true);
                Console.WriteLine("User has been remnoved.");
            }
            else
            {
                Console.WriteLine("User does not exist.");
            }
        }

        /// <summary>
        /// Method to change the password of an user.
        /// </summary>
        public void ChangePassword(string username, string password)
        {

            LoadUsers();
            EditUser(username, password);
            PushUsers(); // WriteAllLines truncates passwd (gen2 Delete + undisposed Create leaked an fd)
            //Directory.Delete(AuraPaths.UsersDir + username, true);
            Console.WriteLine("Password has been changed.");

        }

        public static void DeleteUser(string user)
        {

            foreach (string line in users)
            {
                usersfile.Add(line);
            }

            int counter = -1;
            int index = 0;

            bool exists = false;

            foreach (string element in usersfile)
            {
                counter = counter + 1;
                if (element.Contains(user))
                {
                    index = counter;
                    exists = true;
                }
            }
            if (exists)
            {
                usersfile.RemoveAt(index);

                users = usersfile.ToArray();

                usersfile.Clear();

                PushUsers();
            }
        }

        public static void EditUser(string username, string password)
        {
            foreach (string line in users)
            {
                usersfile.Add(line);
            }

            int counter = -1;
            int index = 0;

            bool exists = false;

            foreach (string element in usersfile)
            {
                counter = counter + 1;
                if (element.Contains(username))
                {
                    index = counter;
                    exists = true;
                }
            }
            if (exists)
            {
                password = Sha256.hash(password);

                usersfile[index] = "user:" + username + ":" + password + "|" + Kernel.userLevelLogged;

                users = usersfile.ToArray();

                usersfile.Clear();
            }
        }

        public static string GetUser(string parameter)
        {
            string value = "null";

            foreach (string line in users)
            {
                usersfile.Add(line);
            }

            foreach (string element in usersfile)
            {
                if (element.StartsWith(parameter))
                {
                    value = element.Remove(0, parameter.Length + 1);
                }
            }

            usersfile.Clear();

            return value;
        }

        public static void PutUser(string parameter, string value)
        {
            bool contains = false;

            foreach (string line in users)
            {
                usersfile.Add(line);
                if (line.StartsWith(parameter))
                {
                    contains = true;
                }
            }

            if (!contains)
            {
                usersfile.Add(parameter + ":" + value);
            }

            users = usersfile.ToArray();

            usersfile.Clear();
        }

        public static void PushUsers()
        {
            File.WriteAllLines(AuraPaths.Passwd, users);
        }

        public static void LoadUsers()
        {
            //reset of users string array in memory if there is "something"
            users = reset;
            //load (a missing passwd behaves as an empty one: users never becomes null)
            if (File.Exists(AuraPaths.Passwd))
            {
                users = File.ReadAllLines(AuraPaths.Passwd);
            }
        }

    }
}