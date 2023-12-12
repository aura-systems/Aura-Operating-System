/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List PCI Devices
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Interpreter;
using Aura_OS.System.Utils;
using Aura_OS.Utils;
using System;
using static Cosmos.HAL.PCIDevice;

namespace Aura_OS.System.Shell.cmdIntr.Util
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
            int count = 0;

            foreach (Cosmos.HAL.PCIDevice device in Cosmos.HAL.PCI.Devices)
            {
                string line = Conversion.D2(device.bus) + ":" + Conversion.D2(device.slot) + ":" + Conversion.D2(device.function) + " - " + "0x" + Conversion.D4(Conversion.DecToHex(device.VendorID)) + ":0x" + Conversion.D4(Conversion.DecToHex(device.DeviceID)) + " : " + DeviceClass.GetTypeString(device) + ": " + DeviceClass.GetDeviceString(device);
                
                Console.WriteLine(line);
                count++;

                if (count == Kernel.console.Rows - 4)
                {
                    //Console.ReadKey();
                    count = 0;
                }
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}