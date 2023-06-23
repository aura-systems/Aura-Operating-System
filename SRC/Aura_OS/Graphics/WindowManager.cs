using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.Graphics
{
    public class WindowManager
    {
        public List<App> apps;
        public App Focused;

        public void Initialize()
        {
            apps = new List<App>();
            apps.Add(Kernel.console);
            apps.Add(Kernel.memoryInfo);
            apps.Add(Kernel.systemInfo);
            apps.Add(Kernel.gameBoyEmu);
            apps.Add(Kernel.cube);
        }

        public void DrawWindows()
        {
            foreach (App app in apps)
            {
                if (app.Running)
                {
                    app.Update();
                }
            }  
        }
    }
}
