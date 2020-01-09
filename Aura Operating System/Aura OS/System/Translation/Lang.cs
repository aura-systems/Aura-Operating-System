using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Aura_OS.System.Translation
{
    public class Lang
    {
        public Dictionary<string, string> lines = new Dictionary<string, string>();

        private string name;
        private string path;

        public Lang(string name)
        {
            this.name = name;
            path = @"0:\System\langs\" + name + ".conf";

            if (Kernel.SystemExists)
            {
                if (File.Exists(path))
                {
                    Load();
                }
            }
        }

        public void Load()
        {
            lines = File.ReadLines(this.path).Where(IsConfigurationLine).Select(line => line.Split('=')).ToDictionary(line => line[0], line => line[1]);
        }

        private static bool IsConfigurationLine(string line)
        {
            return !line.StartsWith("#") && line.Contains("=");
        }

        public string Get(string line)
        {
            string value;
            if (lines.TryGetValue(line, out value)) {
                return value;
            } else {
                return null;
            }
        }
    }
