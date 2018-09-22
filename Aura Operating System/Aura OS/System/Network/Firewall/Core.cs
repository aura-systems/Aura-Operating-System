using Aura_OS.System.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.Firewall
{
    class Core
    {
        public static void Enable()
        {
            Settings.LoadValues();
            Settings.EditValue("firewall-status", "true");
            Settings.PushValues();
            System.Translation.Text.Display("fw_enabled");
        }

        public static void Disable()
        {
            Settings.LoadValues();
            Settings.EditValue("firewall-status", "false");
            Settings.PushValues();
            System.Translation.Text.Display("fw_disabled");
        }

        public static bool Status()
        {
            Settings.LoadValues();
            return bool.Parse(Settings.GetValue("firewall-status"));            
        }
    }
}
