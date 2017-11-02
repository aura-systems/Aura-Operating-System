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
        static List<string> resetconfigurationfile = new List<string>();
        static string[] file;
        static string[] reset;

        private static void Clear()
        {
            configurationfile = resetconfigurationfile;
        }

        public static void LoadValues()
        {
            //reset of config in memory if there is "something"
            if(file.Length >= 1)
            {
                file = reset;
            }
            //load
            file = File.ReadAllLines(@"0:\System\settings.conf");
        }

        public static void PushValues()
        {
            File.WriteAllLines(@"0:\System\settings.conf", file);
        }

        public static void PutValue(string parameter, string value)
        {
            foreach (string line in file)
            {
                configurationfile.Add(line);
            }

            if (!configurationfile.Contains(parameter))
            {
                configurationfile.Add(parameter + "=" + value);
            }

            file = configurationfile.ToArray();

            Clear();
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
                if (element.Contains(parameter))
                {
                    value = element.Remove(0, parameter.Length + 1 );
                }
            }

            Clear();

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

                if (element.Contains(parameter))
                {
                    configurationfile[counter] = parameter + "=" + value;
                }
            }

            file = configurationfile.ToArray();

            Clear();
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

                if (element.Contains(parameter))
                {
                    configurationfile.RemoveAt(counter);
                }
            }

            file = configurationfile.ToArray();

            Clear();
        }

    }
}
