using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.Shell.AuraConsole.subsystems.Aura;
using Aura_OS.Shell.AuraConsole.subsystems.Windows;
using WMCommandFramework;
namespace Aura_OS.Shell.AuraConsole.subsystems
{
    public class SubsystemConnector
    {//parser format base | extentions (example aura | linux) 
        public void Connect_Aura()
        {
            Kernel.processor.GetInvoker().AddCommand(new Aura_Reboot());
            Kernel.processor.GetInvoker().AddCommand(new Aura_Shutdown());
            //clear
            //help
            //filestuff
            //vol
            //run
            //logout
            //settings
            //psswd
            //sysinfo
            //ver
            //ipconfig
            //time
            //crash
            //crashcpu
            //snake
            //md5
            //export
            
        }

        public void Connect_Linux()
        {

        }

        public void Connect_Windows()
        {
            Kernel.processor.GetInvoker().AddCommand(new Windows_CLS());
        }
    }
}
