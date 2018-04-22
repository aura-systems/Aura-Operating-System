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

        /// <summary>
        /// Load values from settings.conf.
        /// </summary>
        public static void LoadValues()
        {
            //reset of config in memory if there is "something"
            file = reset;
            //load
            file = File.ReadAllLines(@"0:\System\settings.conf");
        }

        /// <summary>
        /// Push values to settings.conf.
        /// </summary>
        public static void PushValues()
        {
            if (Kernel.SystemExists)
            {
                File.WriteAllLines(@"0:\System\settings.conf", file);
            }
        }

        /// <summary>
        /// Put a value in settings.
        /// </summary>
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

        /// <summary>
        /// Get a value from settings.
        /// </summary>
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

        /// <summary>
        /// Edit a value in settings.
        /// </summary>
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

        /** public static void DisableParameter(string parameter)
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
    **/

    }
}
