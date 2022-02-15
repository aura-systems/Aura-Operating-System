/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Change Vol
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Interpreter;
using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;

namespace Aura_OS.Interpreter.Commands.Filesystem
{
    class CommandVol : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandVol(string[] commandvalues) : base(commandvalues)
        {
            Description = "to change, list and edit volumes";
        }

        public override ReturnInfo Execute()
        {
            return ListVolumes();
        }

        /// <summary>
        /// CommandChangeVol
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments[0].Equals("/l") || arguments[0].Equals("/list"))
            {
                return ListVolumes();
            }
            else if (arguments[0].Equals("/cv") || arguments[0].Equals("/changevol"))
            {
                return ChangeVolume(arguments[1]);
            }
            else if (arguments[0].Equals("/fp") || arguments[0].Equals("/formatpartiton"))
            {
                return FormatVolume(int.Parse(arguments[1]), int.Parse(arguments[2]));
            }
            else if (arguments[0].Equals("/lp") || arguments[0].Equals("/listpartitions"))
            {
                return ListDisk();
            }
            else if (arguments[0].Equals("/mp") || arguments[0].Equals("/makepartition"))
            {
                return MakeDisk(int.Parse(arguments[1]), int.Parse(arguments[2]));
            }
            else if (arguments[0].Equals("/dp") || arguments[0].Equals("/deletepartition"))
            {
                return DeleteDisk(int.Parse(arguments[1]), int.Parse(arguments[2]));
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            } 
        }

        public ReturnInfo DeleteDisk(int disknumber, int index)
        {
            Disk ourDisk = null;
            int counter = 0;

            index--;

            foreach (var disk in Kernel.VirtualFileSystem.GetDisks())
            {
                if (counter == disknumber)
                {
                    ourDisk = disk;

                    ourDisk.DeletePartition(index);

                    Kernel.console.WriteLine("Partition #" + (index + 1) + " deleted on disk #" + disknumber + "!");

                    Kernel.VirtualFileSystem.Disks.Clear();
                    Kernel.VirtualFileSystem.Initialize(false);

                    break;
                }

                counter++;
            }
            if (ourDisk == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo MakeDisk(int disknumber, int size)
        {
            Disk ourDisk = null;
            int counter = 0;

            foreach (var disk in Kernel.VirtualFileSystem.GetDisks())
            {
                if (counter == disknumber)
                {
                    ourDisk = disk;

                    ourDisk.CreatePartition(size);

                    Kernel.console.WriteLine("Partition created on disk #" + disknumber + "!");

                    Kernel.VirtualFileSystem.Disks.Clear();
                    Kernel.VirtualFileSystem.Initialize(false);

                    break;
                }

                counter++;
            }
            if (ourDisk == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo DiskInfo(int disknumber)
        {
            Disk ourDisk = null;
            int counter = 0;

            foreach (var disk in Kernel.VirtualFileSystem.GetDisks())
            {
                if (counter == disknumber)
                {
                    ourDisk = disk;

                    ourDisk.DisplayInformation();

                    break;
                }

                counter++;
            }
            if (ourDisk == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ListDisk()
        {
            int counter = 0;

            foreach (var disk in Kernel.VirtualFileSystem.GetDisks())
            {
                string type = disk.IsMBR ? "MBR" : "GPT";

                Kernel.console.WriteLine();
                Kernel.console.WriteLine("Disk #: " + counter + " (" + type + ")");

                disk.DisplayInformation();

                counter++;
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo FormatVolume(int driveName, int partition)
        {
            Disk ourDisk = null;
            int counter = 0;

            partition--;

            foreach (var disk in Kernel.VirtualFileSystem.GetDisks())
            {
                if (counter == driveName)
                {
                    ourDisk = disk;

                    disk.FormatPartition(partition, "FAT32", true);

                    Kernel.console.WriteLine("Partition #" + (partition + 1) + " formatted to FAT32 on disk #" + driveName + "!");

                    Kernel.VirtualFileSystem.Disks.Clear();
                    Kernel.VirtualFileSystem.Initialize(false);

                    break;
                }

                counter++;
            }
            if (ourDisk == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ChangeVolume(string volume)
        {
            try
            {
                bool exist = false;

                foreach (var vol in Kernel.VirtualFileSystem.GetVolumes())
                {
                    if (vol.mName == volume + ":\\")
                    {
                        exist = true;
                        Kernel.CurrentVolume = vol.mName;
                        Kernel.CurrentDirectory = Kernel.CurrentVolume;
                    }
                }
                if (!exist)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "The specified drive is not found.");
                }
            }
            catch
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "The specified drive is not found.");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ListVolumes()
        {
            var vols = Kernel.VirtualFileSystem.GetVolumes();

            Kernel.console.WriteLine();
            Kernel.console.WriteLine("  Volume ###\tFormat\tSize");
            Kernel.console.WriteLine("  ----------\t------\t--------");

            foreach (var vol in vols)
            {
                if (vol.mName == Kernel.CurrentVolume && vols.Count > 1)
                {
                    Kernel.console.WriteLine(" >" + vol.mName + "\t   \t" + Kernel.VirtualFileSystem.GetFileSystemType(vol.mName) + " \t" + vol.mSize + " MB\t" + vol.mParent);
                }
                else
                {
                    Kernel.console.WriteLine("  " + vol.mName + "\t   \t" + Kernel.VirtualFileSystem.GetFileSystemType(vol.mName) + " \t" + vol.mSize + " MB\t" + vol.mParent);
                }
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Available commands:");
            Kernel.console.WriteLine("- vol /l                                  List volumes");
            Kernel.console.WriteLine("- vol /cv {volume}                        Change current volume");
            Kernel.console.WriteLine("- vol /lp                                 List partitions");
            Kernel.console.WriteLine("- vol /fp {disknumber} {partitionnumber}  Format partition to FAT32");
            Kernel.console.WriteLine("- vol /mp {disknumber} {size}             Make MBR partition");
            Kernel.console.WriteLine("- vol /dp {disknumber} {partitionnumber}  Delete partition");
        }
    }
}