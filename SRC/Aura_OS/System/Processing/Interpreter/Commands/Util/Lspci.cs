/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List PCI Devices
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Globalization;
using System.Text;
using Aura_OS.System.Utils;
using Cosmos.Kernel.System.Diagnostics;

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
            int count = 0;
            StringBuilder sb = new();

            // GEN3-GAP(pci): no typed PCI enumeration; each PCI function is a driver-kit node
            // whose Path ("pci:ssss:bb:dd.f") and Description ("vvvv:dddd class cc.ss.pp") are parsed.
            int nodeCount = DriverDiagnostics.NodeCount;
            for (int i = 0; i < nodeCount; i++)
            {
                if (!DriverDiagnostics.TryGetNode(i, out DeviceNodeInfo node) || node.BusName != "pci")
                {
                    continue;
                }

                string line;
                if (TryParseDescription(node.Description, out ushort vendor, out ushort device, out byte cls, out byte sub, out byte progIf))
                {
                    line = GetAddress(node.Path) + " - " + "0x" + vendor.ToString("X4") + ":0x" + device.ToString("X4") + " : " + PciNames.GetTypeString(cls, sub, progIf) + ": " + PciNames.GetDeviceString(vendor, device);
                }
                else
                {
                    line = GetAddress(node.Path) + " - " + node.Description;
                }

                line += " [" + (node.DriverName ?? "no driver") + "]";

                sb.AppendLine(line);
                count++;

                /*
                if (count == Kernel.console.Rows - 4)
                {
                    // key wait stays disabled: from the GUI Terminal it would block on, and drain, tty1
                    count = 0;
                }
                */
            }

            Console.Write(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// "pci:0000:00:03.0" -> "00:03.0" (bus:device.function, hex).
        /// </summary>
        private static string GetAddress(string path)
        {
            if (path == null || path.Length <= 4)
            {
                return path ?? "";
            }

            int separator = path.IndexOf(':', 4);
            return separator < 0 ? path : path.Substring(separator + 1);
        }

        /// <summary>
        /// Parses "vvvv:dddd class cc.ss.pp" (PciIdentity.Describe()).
        /// </summary>
        private static bool TryParseDescription(string desc, out ushort vendor, out ushort device, out byte cls, out byte sub, out byte progIf)
        {
            vendor = 0;
            device = 0;
            cls = 0;
            sub = 0;
            progIf = 0;

            if (desc == null || desc.Length < 9 || desc[4] != ':')
            {
                return false;
            }

            int c = desc.IndexOf("class ", StringComparison.Ordinal);
            if (c < 0 || desc.Length < c + 14)
            {
                return false;
            }
            c += 6;

            return ushort.TryParse(desc.AsSpan(0, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out vendor)
                && ushort.TryParse(desc.AsSpan(5, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out device)
                && byte.TryParse(desc.AsSpan(c, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out cls)
                && byte.TryParse(desc.AsSpan(c + 3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out sub)
                && byte.TryParse(desc.AsSpan(c + 6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out progIf);
        }
    }
}