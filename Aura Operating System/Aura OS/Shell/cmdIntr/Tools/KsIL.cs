using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.Shell.cmdIntr.Tools
{
    public class KsIL_command
    {

        public static void c_KsIL(string cmd)
        {

            KsIL.KsILVM KsILVM = new KsIL.KsILVM(1024 * 1024);

            KsILVM.LoadFile(cmd.Remove(0, 4));

            KsILVM.AutoTick();  

        }

    }
}
