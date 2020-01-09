using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Aura_OS.System.Utils
{
    public class JsonSettings : ISettings
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
                    var fileContents = File.ReadAllText(path);
                    config = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
                }
            }
        }

        public void Save()
        {
            if (Kernel.SystemExists)
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(path, json);
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
