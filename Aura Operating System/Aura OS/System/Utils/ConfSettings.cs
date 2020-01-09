using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Aura_OS.System.Utils
{
    public class ConfSettings : ISettings
    {

        private Dictionary<string, string> config = new Dictionary<string, string>();
        private string path;

        public void Load(string path)
        {
            this.path = path;

            if (Kernel.SystemExists)
            {
                if (File.Exists(path))
                {
                    config = File.ReadLines(path).Where(IsConfigurationLine).Select(line => line.Split('=')).ToDictionary(line => line[0], line => line[1]);
                }
            }
        }

        private static bool IsConfigurationLine(string line)
        {
            return !line.StartsWith("#") && line.Contains("=");
        }

        public void Save()
        {
            if (Kernel.SystemExists)
            {
                string[] fileContent = File.ReadAllLines(path);
                List<string> tempConfig = new List<string>();

                int counter = -1;
                int index = 0;
                bool exists = false;

                foreach (string line in fileContent)
                {
                    tempConfig.Add(line);
                }

                // loop throuh dictonary 
                foreach (var setting in config)
                {
                    // see if the current setting exists in the config file
                    foreach(string element in tempConfig)
                    {
                        counter = counter + 1;
                        if (element.Contains(setting.Key))
                        {
                            index = counter;
                            exists = true;
                        }
                    }

                    if (exists)
                    {
                        // update the value
                        tempConfig[index] = setting.Key + "=" + setting.Value;
                    }
                    else
                    {
                        // add to temp config
                        tempConfig.Add(setting.Key + "=" + setting.Value);
                    }
                }

                // now update file content and save
                fileContent = tempConfig.ToArray();
                File.WriteAllLines(path, fileContent);
            }
        }

        public string GetString(string key)
        {
            string value;
            if (config.TryGetValue(key, out value))
            {
                return value;
            }

            return null;
        }

        public void SetString(string key, string value)
        {
            config[key] = value;
        }

        public bool? GetBool(string key)
        {
            return GetString(key) != null ? bool.Parse(GetString(key)) : new bool?();
        }

        public void SetBool(string key, bool value)
        {
            config[key] = value.ToString();
        }

        public int? GetInt(string key)
        {
            return GetString(key) != null ? int.Parse(GetString(key)) : new int?();
        }

        public void SetInt(string key, int value)
        {
            config[key] = value.ToString();
        }

        public long? GetLong(string key)
        {
            return GetString(key) != null ? long.Parse(GetString(key)) : new long?();
        }

        public void SetLong(string key, long value)
        {
            config[key] = value.ToString();
        }

        public IDictionary<string, string> getConfig()
        {
            return config;
        }

    }
}
