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
        static string[] file;
        static string[] reset;

        public static void LoadValues()
        {
            //reset of config in memory if there is "something"
            file = reset;
            //load
            file = File.ReadAllLines(@"0:\System\settings.conf");
        }

        public static void PushValues()
        {
            File.WriteAllLines(@"0:\System\settings.conf", file);
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

            foreach (string element in configurationfile)
            {
                counter = counter + 1;

                if (element.StartsWith(parameter))
                {
                    configurationfile[counter] = parameter + "=" + value;
                }
            }

            file = configurationfile.ToArray();

            configurationfile.Clear();
        }

        public static void DeleteParameter(string parameter)
        {
            foreach (string line in file)
            {
                configurationfile.Add(line);
            }

            int counter = -1;

            foreach (string element in configurationfile)
            {
                counter = counter + 1;

                if (element.StartsWith(parameter))
                {
                    configurationfile.RemoveAt(counter);
                }
            }

            file = configurationfile.ToArray();

            configurationfile.Clear();
        }

    }
}
