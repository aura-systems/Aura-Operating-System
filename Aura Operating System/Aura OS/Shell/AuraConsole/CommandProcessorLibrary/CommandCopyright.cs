using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework
{
    public class CommandCopyright
    {
        private string author = "";
        private string company = "";
        private int baseYear;

        public CommandCopyright(string author, int baseYear = 0)
        {
            Author = author;
            if (baseYear < 1978)
                BaseYear = DateTime.Today.Year;
            else BaseYear = baseYear;
        }

        public string Author
        {
            get => author;
            set => author = value;
        }

        public string Company
        {
            get => company;
            set => company = value;
        }

        public int BaseYear
        {
            get => baseYear;
            set => baseYear = value;
        }

        public override string ToString()
        {
            if (CommandUtils.DebugMode)
                Console.WriteLine("[CommandInvoker/CommandCopyright]: Set CommandArgs to empty.");
            if ((Author != null || Author != "") && (Company != null || Company != ""))
            {
                if (BaseYear == DateTime.Today.Year)
                    return $"Copyright © {BaseYear} {Company}, All Rights Reserved!\nDeveloped By: {Author}!";
                else if ((BaseYear < 1978) || (BaseYear > DateTime.Today.Year))
                    return $"Copyright © {DateTime.Today.Year} {Company}, All Rights Reserved!\nDeveloped By: {Author}!";
                else if (!(BaseYear < 1978) && (BaseYear < DateTime.Today.Year))
                    return $"Copyright © {BaseYear}-{DateTime.Today.Year} {Company}, All Rights Reserved!\nDeveloped By: {Author}!";
            }
            else if ((Author == null || Author == "") && (Company != null || Company != ""))
            {
                if (BaseYear == DateTime.Today.Year)
                    return $"Copyright © {BaseYear} {Company}, All Rights Reserved!";
                else if ((BaseYear < 1978) || (BaseYear > DateTime.Today.Year))
                    return $"Copyright © {DateTime.Today.Year} {Company}, All Rights Reserved!";
                else if (!(BaseYear < 1978) && (BaseYear < DateTime.Today.Year))
                    return $"Copyright © {BaseYear}-{DateTime.Today.Year} {Company}, All Rights Reserved!";
            }
            else if ((Author != null || Author != "") && (Company == null || Company == ""))
            {
                if (BaseYear == DateTime.Today.Year)
                    return $"Copyright © {BaseYear} {Author}, All Rights Reserved!";
                else if ((BaseYear < 1978) || (BaseYear > DateTime.Today.Year))
                    return $"Copyright © {DateTime.Today.Year} {Author}, All Rights Reserved!";
                else if (!(BaseYear < 1978) && (BaseYear < DateTime.Today.Year))
                    return $"Copyright © {BaseYear}-{DateTime.Today.Year} {Author}, All Rights Reserved!";
            }
            else return "";
            return "";
        }
    }
}
