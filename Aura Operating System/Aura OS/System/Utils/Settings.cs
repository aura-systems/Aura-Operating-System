/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Settings class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Utils
{
    public static class Settings
    {

        static List<string> configurationfile = new List<string>();
        static List<string> usersfile = new List<string>();
        static string[] file;
        static string[] reset;
        static string[] users;

        public static void LoadValues()
        {
            //reset of config in memory if there is "something"
            file = reset;
            //load
            file = File.ReadAllLines(@"0:\System\settings.conf");
        }

        public static void LoadUsers()
        {
            //reset of users string array in memory if there is "something"
            users = reset;
            //load
            users = File.ReadAllLines(@"0:\System\passwd");
        }

        public static void PushValues()
        {
            File.WriteAllLines(@"0:\System\settings.conf", file);
        }

        public static void PushUsers()
        {
            File.WriteAllLines(@"0:\System\passwd", users);
        }

        public static void PutValue(string parameter, string value)
        {
            bool contains = false;

            foreach (string line in file)
            {
                configurationfile.Add(line);
                if (line.StartsWith(parameter))
                {
                    contains = true;
                }
            }

            if (!contains)
            {
                configurationfile.Add(parameter + "=" + value);
            }

            file = configurationfile.ToArray();

            configurationfile.Clear();
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

        public static string GetValue(string parameter)
        {
            string value = "null";

            foreach (string line in file)
            {
                configurationfile.Add(line);
            }

            foreach (string element in configurationfile)
            {
                if (element.StartsWith(parameter))
                {
                    value = element.Remove(0, parameter.Length + 1 );
                }
            }

            configurationfile.Clear();

            return value;
        }

        public static void EditValue(string parameter, string value)
        {
            foreach (string line in file)
            {
                configurationfile.Add(line);
            }

            int counter = -1;
            int index = 0;

            bool exists = false;

            foreach (string element in configurationfile)
            {
                counter = counter + 1;
                if (element.Contains(parameter))
                {
                    index = counter;
                    exists = true;
                }
            }
            if (exists)
            {
                configurationfile[index] = parameter + "=" + value;

                file = configurationfile.ToArray();

                configurationfile.Clear();
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
                password = Security.MD5.hash(password);

                usersfile[index] = "user:" + username + ":" + password + "|" + Kernel.userLevelLogged;

                users = usersfile.ToArray();

                usersfile.Clear();
            }
        }

        public static void DisableParameter(string parameter)
        {
            foreach (string line in file)
            {
                configurationfile.Add(line);
            }

            int counter = -1;
            int index = 0;

            bool exists = false;

            foreach (string element in configurationfile)
            {
                counter = counter + 1;
                if (element.Contains(parameter))
                {
                    index = counter;
                    exists = true;
                }
            }
            if (exists)
            {
                configurationfile[index] = "#" + parameter + "=" + GetValue(parameter);

                file = configurationfile.ToArray();

                configurationfile.Clear();
            }
        }

        public static void EnableParameter(string parameter)
        {
            foreach (string line in file)
            {
                configurationfile.Add(line);
            }

            int counter = -1;
            int index = 0;

            bool exists = false;

            foreach (string element in configurationfile)
            {
                counter = counter + 1;
                if (element.Contains(parameter))
                {
                    index = counter;
                    exists = true;
                }
            }
            if (exists)
            {
                configurationfile[index] = parameter + "=" + GetValue(parameter);

                file = configurationfile.ToArray();

                configurationfile.Clear();
            }
        }

        public static void DeleteUser(string user)
        {
            List<string> NewUsersFile = new List<string>();

            foreach (string line in users)
            {
                if (!line.StartsWith("user:" + user + ":"))
                {
                    NewUsersFile.Add(line);
                    Console.WriteLine("[FALSE] OK");
                }
                else
                {
                    Console.WriteLine("[TRUE] " + line);
                }
            }
            File.Delete(@"0:\System\passwd");
            File.WriteAllLines(@"0:\System\passwd", NewUsersFile.ToArray());

            LoadUsers();

            NewUsersFile.Clear();            
        }

    }
}
