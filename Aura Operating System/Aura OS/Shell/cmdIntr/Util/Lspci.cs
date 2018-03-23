/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List PCI Devices
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.Util
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
                Console.WriteLine(device.bus + ":" + device.slot + ":" + device.function + " " + device.VendorID + " " + device.DeviceID);
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
