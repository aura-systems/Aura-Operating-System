using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace Aura_OS.System.Utils
{
    class XmlSettings : ISettings
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
                    XDocument doc = XDocument.Load(path);
                   
                    foreach (XElement element in doc.Descendants().Where(p => p.HasElements == false))
                    {
                        int keyInt = 0;
                        string keyName = element.Name.LocalName;

                        while (config.ContainsKey(keyName))
                        {
                            keyName = element.Name.LocalName + "_" + keyInt++;
                        }

                        config[keyName] = element.Value;
                    }
                }
            }
        }

        public void Save()
        {
            if (Kernel.SystemExists)
            {
                XElement settings = new XElement("settings", config.Select(setting => new XElement(setting.Key, setting.Value)));
                settings.Save(path);
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
