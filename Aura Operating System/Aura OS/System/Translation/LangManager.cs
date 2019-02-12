using System.Collections.Generic;

namespace Aura_OS.System.Translation
{
    public class LangManager
    {
        private Dictionary<string, Lang> Languages = new Dictionary<string, Lang>();

        private string DefaultLang = "en_US";

        public void addLang(string name, Lang lang)
        {
            if (!Languages.ContainsKey(name))
            {
                Languages.Add(name, lang);
            }
        }

        public Lang GetLang(string name)
        {
            Lang value;
            if (Languages.TryGetValue(name, out value))
            {
                return value;
            }
            else
            {
                return Languages[DefaultLang];
            }
        }

        public Dictionary<string, Lang> getLangs()
        {
            return Languages;
        }
    }
}
