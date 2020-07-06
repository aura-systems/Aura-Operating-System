<p align="center"><img width=60% src="https://raw.githubusercontent.com/aura-systems/Aura-Operating-System/master/ARTWORK/auralogo.png"></p>

![Build Status](https://github.com/aura-systems/Aura-Operating-System/workflows/.NET%20Core/badge.svg)
[![Issues](https://img.shields.io/github/issues/aura-systems/Aura-Operating-System.svg)](https://github.com/aura-systems/Aura-Operating-System/issues)
[![Pull requests](https://img.shields.io/github/issues-pr/aura-systems/Aura-Operating-System.svg)](https://github.com/aura-systems/Aura-Operating-System/pulls)
[![Discord](https://img.shields.io/badge/join%20us%20on-discord-blue.svg)](https://discord.gg/DFbAtVA)
[![Website](https://img.shields.io/badge/our%20website-aura--team.com-blue.svg)](http://aura-team.com)

A Cosmos based Operating System developped in C# made by Alexy DA CRUZ (GeomTech) and Valentin Charbonnier (valentinbreiz).

## Current features
Please read the [Aura Progression](https://github.com/aura-systems/Aura-Operating-System/projects/4) or our [Roadmap](https://github.com/aura-systems/Aura-Operating-System/projects/3) to know what will be added soon.

* ATA IDE / AHCI Driver.
* FAT32/16/12 + Virtual FileSystem.
* PCI Device Scan.
* PS2 Keyboard.
* Restart + ACPI Shutdown.
* Multi languages/Multi users support.
* Basic command interpreter.
* Exception Handler.
* Console in VGA Textmode (80x25) / SVGAII Graphics mode / VBE Graphics mode + Extended ASCII support.
* Networking (PCNETII / RTL8168 Driver, ARP, IPV4, ICMP, TCP, UDP, DNS, DHCP Client)

Aura's kernel: https://github.com/aura-systems/Cosmos.

Work in progress:

* GUI (VBE) - [Pull request](https://github.com/aura-systems/Aura-Operating-System/pull/55).
* Multitask - [Pull request](https://github.com/aura-systems/Cosmos/pull/40).

## Screenshots

<p align="center"><img width=60% src="https://raw.githubusercontent.com/aura-systems/Aura-Operating-System/master/ARTWORK/aura2.png"></p>

<p align="center"><img width=60% src="https://raw.githubusercontent.com/aura-systems/Aura-Operating-System/master/ARTWORK/aura3.png"></p>

<p align="center"><img width=60% src="https://raw.githubusercontent.com/aura-systems/Aura-Operating-System/master/ARTWORK/aura1.png"></p>

## Contribute
You want to add awesome features to Aura? Here's how:

- Fork Aura-Operating-System repo
- Commit & push a new feature to the forked repository
- Open a pull request from your fork to Aura OS repository
- We will review and merge it :)

## Try Aura
Download VMWare [at this address](https://my.vmware.com/en/web/vmware/free#desktop_end_user_computing/vmware_workstation_player/12_0). Install and run it.

Now click on "Create a new virtual machine", select the iso file downloaded on [this page](https://github.com/aura-systems/Aura-Operating-System/releases) and click the "Next" button.

Now click on "Other" for "Guest operating system" and "Other" for version, click "Next" again, select "Store virtual disk as a single file" and select "Finish". 

The Virtual File System won't work so go to "C:\Users\username\Documents\Virtual Machines\Other" and replace the "Other.vmdk" by [this file](https://github.com/CosmosOS/Cosmos/raw/master/Build/VMWare/Workstation/Filesystem.vmdk).

Now you can select Aura (Other) and click on "Play Virtual Machine".
