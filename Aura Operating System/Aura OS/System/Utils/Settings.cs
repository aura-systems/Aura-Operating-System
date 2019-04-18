/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Settings Class v2
* PROGRAMMERS:      <dacruzalexy@gmail.com> Alexy DA CRUZ
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Utils
{
    public class Settings
    {
        static List<string> config = new List<string>();
        string[] fileContent;
        string path;

        public Settings(string path)
        {
            this.path = path;

            fileContent = null;
            if (Kernel.SystemExists)
            {
                if (File.Exists(path))
                {
                    fileContent = File.ReadAllLines(path);
                }
            }
        }

        public void Push()
        {
            if (Kernel.SystemExists)
            {
                File.WriteAllLines(path, fileContent);
            }
        }

        public void PushValues()
        {
            Push();
        }

        public void Add(string parameter, string value)
        {
            bool contains = false;

            foreach (string line in fileContent)
            {
                config.Add(line);
                if (line.StartsWith(parameter))
                {
                    contains = true;
                }
            }

            if (!contains)
            {
                config.Add(parameter + "=" + value);
            }

            fileContent = config.ToArray();

            config.Clear();
        }

        public void PutValue(string parameter, string value)
        {
            Add(parameter, value);
        }

        public string Get(string parameter)
        {
            if (fileContent == null)
            {
                return "null";
            }

            string value = "null";

            foreach (string line in fileContent)
            {
                config.Add(line);
            }

            foreach (string element in config)
            {
                if (element.StartsWith(parameter))
                {
                    value = element.Remove(0, parameter.Length + 1);
                }
            }

            config.Clear();

            return value;
        }

        public string GetValue(string parameter)
        {
            return Get(parameter);
        }

        public void Edit(string parameter, string value)
        {
            foreach (string line in fileContent)
            {
                config.Add(line);
            }

            int counter = -1;
            int index = 0;

            bool exists = false;

            foreach (string element in config)
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
                config[index] = parameter + "=" + value;

                fileContent = config.ToArray();

                config.Clear();
            }
            else
            {
                Add(parameter, value);
            }
        }

        public void EditValue(string parameter, string value)
        {
            Edit(parameter, value);
        }

    }
}