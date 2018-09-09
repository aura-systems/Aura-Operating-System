using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework.COSMOS;
namespace Aura_OS.Shell.CommandInterperter.Util
{
	public class Lspci : Command
	{
		public override string[] Aliases()
		{
			return new string[0];
		}

		public override string Description()
		{
			return "Display's PCIDevices";
		}

		public override void Invoke(CommandInvoker invoker, CommandArgs args)
		{
			int count = 0;
			foreach (Cosmos.HAL.PCIDevice device in Cosmos.HAL.PCI.Devices)
			{
				Console.WriteLine(device.bus + ":" + device.slot + ":" + device.function + " " + Cosmos.HAL.PCIDevice.DeviceClass.GetTypeString(device) + ": " + Cosmos.HAL.PCIDevice.DeviceClass.GetDeviceString(device) + " (0x" + System.Utils.Conversion.DecToHex(device.VendorID) + ":0x" + System.Utils.Conversion.DecToHex(device.DeviceID) + ")");
				count++;
				if (count == 19)
				{
					Console.ReadKey();
					count = 0;
				}
			}
		}

		public override string Name()
		{
			return "lspci";
		}

		public override string Syntax()
		{
			return "";
		}

		public override CommandVersion Version()
		{
			return new CommandVersion(1, 0, 0, 0, "a");
		}
	}
}
