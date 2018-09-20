/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List PCI Devices
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Utils;
using System;
using static Cosmos.HAL.PCIDevice;

namespace Aura_OS.System.Shell.cmdIntr.Util
{
    class Lspci
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public Lspci() { }

        /// <summary>
        /// c = command, c_Export
        /// </summary>
        /// <param name="arg">The command</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Lspci()
        {
            int count = 0;
            foreach (Cosmos.HAL.PCIDevice device in Cosmos.HAL.PCI.Devices)
            {
                Console.WriteLine(Conversion.D2(device.bus) + ":" + Conversion.D2(device.slot) + ":" + Conversion.D2(device.function) + " - " + "0x" + Conversion.D4(Conversion.DecToHex(device.VendorID)) + ":0x" + Conversion.D4(Conversion.DecToHex(device.DeviceID)) + " : " + DeviceClass.GetTypeString(device) + ": " + DeviceClass.GetDeviceString(device));
                count++;
                if (count==19)
                {
                    Console.ReadKey();
                    count = 0;
                }
            }
        }
    }
}
