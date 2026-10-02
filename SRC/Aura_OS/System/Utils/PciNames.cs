// This code is licensed under the BSD 3-Clause license (see LICENSES/COSMOS/LICENSE.md for details)
/*
* PROJECT:          Aura Operating System Development
* CONTENT:          PCI class and device names
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*
* Tables copied from Cosmos (https://github.com/CosmosOS/Cosmos), file
* src/Cosmos.Kernel.HAL/Pci/DeviceString.cs (internal in gen3).
* Copyright (c) 2007-2026, CosmosOS, COSMOS Project. BSD 3-Clause.
* Adapted to take the raw ids; the "Aura additions" are QEMU devices.
*/

namespace Aura_OS.System.Utils
{
    /// <summary>
    /// PCI names for lspci.
    /// GEN3-GAP(pci): Cosmos keeps its name tables (DeviceString) internal, so Aura carries this copy.
    /// </summary>
    public static class PciNames
    {
        public static string GetDeviceString(ushort vendor, ushort device)
        {
            switch (vendor)
            {
                case 0x1022: //AMD
                    switch (device)
                    {
                        case 0x2000:
                            return "AMD PCnet LANCE PCI Ethernet Controller";
                        default:
                            return "AMD Unknown device";
                    }
                case 0x104B: //Sony
                    switch (device)
                    {
                        case 0x1040:
                            return "Mylex BT958 SCSI Host Adaptor";
                        default:
                            return "Mylex Unknown device";
                    }
                case 0x1234: //Bochs
                    switch (device)
                    {
                        case 0x1111:
                            return "Bochs BGA";
                        default:
                            return "Bochs Unknown device";
                    }
                case 0x1274: //Ensoniq
                    switch (device)
                    {
                        case 0x1371:
                            return "Ensoniq AudioPCI";
                        default:
                            return "Ensoniq Unknown device";
                    }
                case 0x15AD: //VMware
                    switch (device)
                    {
                        case 0x0405:
                            return "VMware NVIDIA 9500MGS";
                        case 0x0740:
                            return "Vmware Virtual Machine Communication Interface";
                        case 0x0770:
                            return "VMware Standard Enhanced PCI to USB Host Controller";
                        case 0x0790:
                            return "VMware 6.0 Virtual USB 2.0 Host Controller";
                        case 0x07A0:
                            return "VMware PCI Express Root Port";
                        default:
                            return "VMware Unknown device";
                    }
                case 0x8086: //Intel
                    switch (device)
                    {
                        case 0x7190:
                            return "Intel 440BX/ZX AGPset Host Bridge";
                        case 0x7191:
                            return "Intel 440BX/ZX AGPset PCI-to-PCI bridge";
                        case 0x7110:
                            return "Intel PIIX4/4E/4M ISA Bridge";
                        case 0x7111:
                            return "Intel PIIX4/82371AB/EB/MB IDE";
                        case 0x7112:
                            return "Intel PIIX4/4E/4M USB Interface";
                        case 0x7113:
                            return "Intel PIIX4/82371AB/EB/MB ACPI";
                        // Aura additions (QEMU i440FX/Q35 machines)
                        case 0x100E:
                            return "Intel 82540EM Gigabit Ethernet Controller";
                        case 0x10D3:
                            return "Intel 82574L Gigabit Network Connection";
                        case 0x1237:
                            return "Intel 440FX - 82441FX PMC [Natoma]";
                        case 0x2668:
                            return "Intel 82801FB (ICH6) High Definition Audio Controller";
                        case 0x2918:
                            return "Intel 82801IB (ICH9) LPC Interface Controller";
                        case 0x2922:
                            return "Intel 82801IR (ICH9R) 6 port SATA Controller [AHCI mode]";
                        case 0x2930:
                            return "Intel 82801I (ICH9 Family) SMBus Controller";
                        case 0x293E:
                            return "Intel 82801I (ICH9 Family) HD Audio Controller";
                        case 0x29C0:
                            return "Intel 82G33/G31/P35/P31 Express DRAM Controller";
                        case 0x7000:
                            return "Intel 82371SB PIIX3 ISA [Natoma/Triton II]";
                        case 0x7010:
                            return "Intel 82371SB PIIX3 IDE [Natoma/Triton II]";
                        case 0x7020:
                            return "Intel 82371SB PIIX3 USB [Natoma/Triton II]";
                        default:
                            return "Intel Unknown device";
                    }
                // Aura addition
                case 0x1AF4: //Red Hat (virtio)
                    switch (device)
                    {
                        case 0x1000:
                            return "Virtio network device";
                        case 0x1001:
                            return "Virtio block device";
                        case 0x1041:
                            return "Virtio 1.0 network device";
                        case 0x1042:
                            return "Virtio 1.0 block device";
                        case 0x1050:
                            return "Virtio 1.0 GPU";
                        case 0x1052:
                            return "Virtio 1.0 input";
                        default:
                            return "Virtio Unknown device";
                    }
                // Aura addition
                case 0x1B36: //Red Hat (QEMU)
                    switch (device)
                    {
                        case 0x0008:
                            return "QEMU PCIe Host bridge";
                        case 0x000D:
                            return "QEMU XHCI Host Controller";
                        case 0x0010:
                            return "QEMU NVM Express Controller";
                        default:
                            return "QEMU Unknown device";
                    }
                case 0x80EE: //VirtualBox
                    switch (device)
                    {
                        case 0xBEEF:
                            return "VirtualBox Graphics Adapter";
                        case 0xCAFE:
                            return "VirtualBox Guest Service";
                        default:
                            return "VirtualBox Unknown device";
                    }
                default:
                    return "Unknown device";
            }
        }

        public static string GetTypeString(byte cls, byte sub, byte progIf)
        {
            switch (cls)
            {
                case 0x00:
                    switch (sub)
                    {
                        case 0x01:
                            return "VGA-Compatible Device";
                        default:
                            return "0x00 Subclass";
                    }
                case 0x01:
                    switch (sub)
                    {
                        case 0x00:
                            return "SCSI Bus Controller";
                        case 0x01:
                            return "IDE Controller";
                        case 0x02:
                            return "Floppy Disk Controller";
                        case 0x03:
                            return "IPI Bus Controller";
                        case 0x04:
                            return "RAID Controller";
                        case 0x05:
                            return "ATA Controller";
                        case 0x06:
                            return "Serial ATA";
                        case 0x80:
                            return "Unknown Mass Storage Controller";
                        default:
                            return "Mass Storage Controller";
                    }
                case 0x02:
                    switch (sub)
                    {
                        case 0x00:
                            return "Ethernet Controller";
                        case 0x01:
                            return "Token Ring Controller";
                        case 0x02:
                            return "FDDI Controller";
                        case 0x03:
                            return "ATM Controller";
                        case 0x04:
                            return "ISDN Controller";
                        case 0x05:
                            return "WorldFip Controller";
                        case 0x06:
                            return "PICMG 2.14 Multi Computing";
                        case 0x80:
                            return "Unknown Network Controller";
                        default:
                            return "Network Controller";
                    }
                case 0x03:
                    switch (sub)
                    {
                        case 0x00:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "VGA-Compatible Controller";
                                case 0x01:
                                    return "8512-Compatible Controller";
                                default:
                                    return "Unknown Display Controller";
                            }
                        case 0x01:
                            return "XGA Controller";
                        case 0x02:
                            return "3D Controller (Not VGA-Compatible)";
                        case 0x80:
                            return "Unknown Display Controller";
                        default:
                            return "Display Controller";
                    }
                case 0x04:
                    switch (sub)
                    {
                        case 0x00:
                            return "Video Device";
                        case 0x01:
                            return "Audio Device";
                        case 0x02:
                            return "Computer Telephony Device";
                        case 0x80:
                            return "Unknown Multimedia Controller";
                        default:
                            return "Multimedia Controller";
                    }
                case 0x05:
                    switch (sub)
                    {
                        case 0x00:
                            return "RAM Controller";
                        case 0x01:
                            return "Flash Controller";
                        case 0x80:
                            return "Unknown Memory Controller";
                        default:
                            return "Memory Controller";
                    }
                case 0x06:
                    switch (sub)
                    {
                        case 0x00:
                            return "Host Bridge";
                        case 0x01:
                            return "ISA Bridge";
                        case 0x02:
                            return "EISA Bridge";
                        case 0x03:
                            return "MCA Bridge";
                        case 0x04:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "PCI-to-PCI Bridge";
                                case 0x01:
                                    return "PCI-to-PCI Bridge (Subtractive Decode)";
                                default:
                                    return "Unknown PCI-to-PCI Bridge";
                            }
                        case 0x05:
                            return "PCMCIA Bridge";
                        case 0x06:
                            return "NuBus Bridge";
                        case 0x07:
                            return "CardBus Bridge";
                        case 0x08:
                            return "RACEway Bridge";
                        case 0x09:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "PCI-to-PCI Bridge (Semi-Transparent, Primary)";
                                case 0x01:
                                    return "PCI-to-PCI Bridge (Semi-Transparent, Secondary)";
                                default:
                                    return "Unknown PCI-to-PCI Bridge";
                            }
                        case 0x0A:
                            return "InfiniBrand-to-PCI Host Bridge";
                        case 0x80:
                            return "Unknown Bridge Device";
                        default:
                            return "Bridge Device";
                    }
                case 0x07:
                    switch (sub)
                    {
                        case 0x00:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "Generic XT-Compatible Serial Controller";
                                case 0x01:
                                    return "16450-Compatible Serial Controller";
                                case 0x02:
                                    return "16550-Compatible Serial Controller";
                                case 0x03:
                                    return "16650-Compatible Serial Controller";
                                case 0x04:
                                    return "16750-Compatible Serial Controller";
                                case 0x05:
                                    return "16850-Compatible Serial Controller";
                                case 0x06:
                                    return "16950-Compatible Serial Controller";
                                default:
                                    return "Simple Communication Controller";
                            }
                        case 0x01:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "Parallel Port";
                                case 0x01:
                                    return "Bi-Directional Parallel Port";
                                case 0x02:
                                    return "ECP 1.X Compliant Parallel Port";
                                case 0x03:
                                    return "IEEE 1284 Controller";
                                case 0xFE:
                                    return "IEEE 1284 Target Device";
                                default:
                                    return "Parallel Port";
                            }
                        case 0x02:
                            return "Multiport Serial Controller";
                        case 0x03:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "Generic Modem";
                                case 0x01:
                                    return "Hayes Compatible Modem (16450-Compatible Interface)";
                                case 0x02:
                                    return "Hayes Compatible Modem (16550-Compatible Interface)";
                                case 0x03:
                                    return "Hayes Compatible Modem (16650-Compatible Interface)";
                                case 0x04:
                                    return "Hayes Compatible Modem (16750-Compatible Interface)";
                                default:
                                    return "Modem";
                            }
                        case 0x04:
                            return "IEEE 488.1/2 (GPIB) Controller";
                        case 0x05:
                            return "Smart Card";
                        case 0x80:
                            return "Unknown Communications Device";
                        default:
                            return "Simple Communication Controller";
                    }
                case 0x08:
                    switch (sub)
                    {
                        case 0x00:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "Generic 8259 PIC";
                                case 0x01:
                                    return "ISA PIC";
                                case 0x02:
                                    return "EISA PIC";
                                case 0x10:
                                    return "I/O APIC Interrupt Controller";
                                case 0x20:
                                    return "I/O(x) APIC Interrupt Controller";
                                default:
                                    return "PIC";
                            }
                        case 0x01:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "Generic 8237 DMA Controller";
                                case 0x01:
                                    return "ISA DMA Controller";
                                case 0x02:
                                    return "EISA DMA Controller";
                                default:
                                    return "DMA Controller";
                            }
                        case 0x02:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "Generic 8254 System Timer";
                                case 0x01:
                                    return "ISA System Timer";
                                case 0x02:
                                    return "EISA System Timer";
                                default:
                                    return "System Timer";
                            }
                        case 0x03:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "Generic RTC Controller";
                                case 0x01:
                                    return "ISA RTC Controller";
                                default:
                                    return "RTC Controller";
                            }
                        case 0x04:
                            return "Generic PCI Hot-Plug Controller";
                        case 0x80:
                            return "Unknown System Peripheral";
                        default:
                            return "Base System Peripheral";
                    }
                case 0x09:
                    switch (sub)
                    {
                        case 0x00:
                            return "Keyboard Controller";
                        case 0x01:
                            return "Digitizer";
                        case 0x02:
                            return "Mouse Controller";
                        case 0x03:
                            return "Scanner Controller";
                        case 0x04:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "Gameport Controller (Generic)";
                                case 0x10:
                                    return "Gameport Controller (Legacy)";
                                default:
                                    return "Gameport Controller";
                            }
                        case 0x80:
                            return "Unknown Input Controller";
                        default:
                            return "Input Device";
                    }
                case 0x0A:
                    switch (sub)
                    {
                        case 0x00:
                            return "Generic Docking Station";
                        case 0x80:
                            return "Unknown Docking Station";
                        default:
                            return "Docking Station";
                    }
                case 0x0B:
                    switch (sub)
                    {
                        case 0x00:
                            return "386 Processor";
                        case 0x01:
                            return "486 Processor";
                        case 0x02:
                            return "Pentium Processor";
                        case 0x10:
                            return "Alpha Processor";
                        case 0x20:
                            return "PowerPC Processor";
                        case 0x30:
                            return "MIPS Processor";
                        case 0x40:
                            return "Co-Processor";
                        default:
                            return "Processor";
                    }
                case 0x0C:
                    switch (sub)
                    {
                        case 0x00:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "IEEE 1394 Controller (FireWire)";
                                case 0x10:
                                    return "IEEE 1394 Controller (1394 OpenHCI Spec)";
                                default:
                                    return "IEEE Controller";
                            }
                        case 0x01:
                            return "ACCESS.bus";
                        case 0x02:
                            return "SSA";
                        case 0x03:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "USB (Universal Host Controller Spec)";
                                case 0x10:
                                    return "USB (Open Host Controller Spec)";
                                case 0x20:
                                    return "USB2 Host Controller (Intel Enhanced Host Controller Interface)";
                                case 0x30:
                                    return "USB3 XHCI Controller";
                                case 0x80:
                                    return "Unspecified USB Controller";
                                case 0xFE:
                                    return "USB (Not Host Controller)";
                                default:
                                    return "USB";
                            }
                        case 0x04:
                            return "Fibre Channel";
                        case 0x05:
                            return "SMBus";
                        case 0x06:
                            return "InfiniBand";
                        case 0x07:
                            switch (progIf)
                            {
                                case 0x00:
                                    return "IPMI SMIC Interface";
                                case 0x01:
                                    return "IPMI Kybd Controller Style Interface";
                                case 0x02:
                                    return "IPMI Block Transfer Interface";
                                default:
                                    return "IPMI SMIC Interface";
                            }
                        case 0x08:
                            return "SERCOS Interface Standard (IEC 61491)";
                        case 0x09:
                            return "CANbus";
                        default:
                            return "Serial Bus Controller";
                    }
                case 0x0D:
                    switch (sub)
                    {
                        case 0x00:
                            return "iRDA Compatible Controller";
                        case 0x01:
                            return "Consumer IR Controller";
                        case 0x10:
                            return "RF Controller";
                        case 0x11:
                            return "Bluetooth Controller";
                        case 0x12:
                            return "Broadband Controller";
                        case 0x20:
                            return "Ethernet Controller (802.11a)";
                        case 0x21:
                            return "Ethernet Controller (802.11b)";
                        case 0x80:
                            return "Unknown Wireless Controller";
                        default:
                            return "Wireless Controller";
                    }
                case 0x0E:
                    switch (sub)
                    {
                        case 0x00:
                            return "Message FIFO";
                        default:
                            return "Intelligent I/O Controller";
                    }
                case 0x0F:
                    switch (sub)
                    {
                        case 0x01:
                            return "TV Controller";
                        case 0x02:
                            return "Audio Controller";
                        case 0x03:
                            return "Voice Controller";
                        case 0x04:
                            return "Data Controller";
                        default:
                            return "Satellite Communication Controller";
                    }
                case 0x10:
                    switch (sub)
                    {
                        case 0x00:
                            return "Network and Computing Encrpytion/Decryption";
                        case 0x10:
                            return "Entertainment Encryption/Decryption";
                        case 0x80:
                            return "Unknown Encryption/Decryption";
                        default:
                            return "Encryption/Decryption Controller";
                    }
                case 0x11:
                    switch (sub)
                    {
                        case 0x00:
                            return "DPIO Modules";
                        case 0x01:
                            return "Performance Counters";
                        case 0x10:
                            return "Communications Syncrhonization Plus Time and Frequency Test/Measurment";
                        case 0x20:
                            return "Management Card";
                        case 0x80:
                            return "Unknown Data Acquisition/Signal Processing Controller";
                        default:
                            return "Data Acquisition and Signal Processing Controller";
                    }
                default:
                    return "Unknown device type";
            }
        }
    }
}
