using Aura_OS;
using Aura_OS.System;
using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_Plugs
{
    [Plug(Target = typeof(Debugger))]
    public class DebuggerImpl
    {
        public static void DoSend(string aText)
        {
            Logs.DoKernelLog(aText);
        }
    }
}
