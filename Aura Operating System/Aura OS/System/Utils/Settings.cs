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

        public static void PutValue(string parameter, string value)
        {
            string[] file = File.ReadAllLines(@"0:\System\settings.conf");
            foreach (string line in file)
            {
                configurationfile.Add(line);
            }

            if (!configurationfile.Contains(parameter))
            {
                configurationfile.Add(parameter + "=" + value);
            }

            file = configurationfile.ToArray();

            configurationfile.Clear();

            File.WriteAllLines(@"0:\System\settings.conf", file);
        }

        public static string GetValue(string parameter)
        {
            string value = "null";

            string[] file = File.ReadAllLines(@"0:\System\settings.conf");
            foreach (string line in file)
            {
                configurationfile.Add(line);
            }

            foreach (string element in configurationfile)
            {
                if (element.Contains(parameter))
                {
                    value = element.Remove(parameter.Length + 1 );
                }
            }
            return value;
        }
        public static void EditValue(string parameter, string value)
        {

        }

        public static void DeleteParameter(string parameter)
        {

        }

    }
}
