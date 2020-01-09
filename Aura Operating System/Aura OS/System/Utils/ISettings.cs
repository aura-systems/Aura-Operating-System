using System.Collections.Generic;

namespace Aura_OS.System.Utils
{
    public interface ISettings
    {
        void Load(string path);

        void Save();
            
        string GetString(string key);

        void SetString(string key, string value);

        bool? GetBool(string key);

        void SetBool(string key, bool value);

        int? GetInt(string key);

        void SetInt(string key, int value);

        long? GetLong(string key);

        void SetLong(string key, long value);

        IDictionary<string, string> getConfig();
    }
}
