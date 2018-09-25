using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Executables.Drivers
{
	public abstract class Driver
	{
		public string Name;

		public Driver()
		{
			Kernel.Drivers.Add(this);
		}

		public abstract bool Init();
	}
}
