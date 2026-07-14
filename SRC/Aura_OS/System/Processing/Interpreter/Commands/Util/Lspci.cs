/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List PCI Devices
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Utils;
using Cosmos.Kernel.HAL.Pci;
using System;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.Util
{
    class CommandLspci : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandLspci(string[] commandvalues) : base(commandvalues)
        {
            Description = "list pci devices";
        }

        /// <summary>
        /// CommandLspci
        /// </summary>
        public override ReturnInfo Execute()
        {
            if (PciManager.Devices == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "PCI bus has not been scanned.");
            }

            StringBuilder sb = new();

            // Devices is a fixed-size array; only the first Count slots are
            // populated. Iterating the whole array would dereference the null
            // trailing slots (page fault). Walk only the populated entries.
            for (int i = 0; i < PciManager.Count; i++)
            {
                PciDevice device = PciManager.Devices[i];

                string line = Conversion.D2(device.Bus) + ":" + Conversion.D2(device.Slot) + ":" + Conversion.D2(device.Function) + " - " + "0x" + Conversion.D4(Conversion.DecToHex(device.VendorId)) + ":0x" + Conversion.D4(Conversion.DecToHex(device.DeviceId)) + " : " + device.GetTypeString() + ": " + device.GetDeviceString();

                sb.AppendLine(line);
            }

            Console.Write(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
