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

            int count = 0;
            StringBuilder sb = new();

            foreach (PciDevice device in PciManager.Devices)
            {
                string line = Conversion.D2(device.Bus) + ":" + Conversion.D2(device.Slot) + ":" + Conversion.D2(device.Function) + " - " + "0x" + Conversion.D4(Conversion.DecToHex(device.VendorId)) + ":0x" + Conversion.D4(Conversion.DecToHex(device.DeviceId)) + " : " + device.GetTypeString() + ": " + device.GetDeviceString();

                sb.AppendLine(line);
                count++;

                /*
                if (count == Kernel.console.Rows - 4)
                {
                    //Console.ReadKey();
                    count = 0;
                }
                */
            }

            Console.Write(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
