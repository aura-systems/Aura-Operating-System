/*******************************************************
 * Copyright (C) 2012-2013 GruntXProductions <sloan@gruntxproductions.net>
 * 
 * This file is part of Grunty OS Infinity
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential.
 *******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura_OS.HAL
{
    public abstract class Driver
    {
        public string Name;

        public void irq_wait(ushort IRQ)
        {
            Cosmos.Core.Global.CPU.Halt();
        }

        public void setIntHandler(byte Int, Cosmos.Core.INTs.InterruptDelegate id)
        {
            Kernel.SetInterruptGate(Int, id);
        }

        public Driver()
        {
            Kernel.Drivers.Add(this);
        }
        public abstract bool Init();
    }
}
