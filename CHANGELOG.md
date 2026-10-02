# Changelog

<!-- CI publishes the release "Aura <version>" when the AuraVersion in SRC/Aura_OS/Aura_OS.csproj has no
     release yet. The release text is that version's section below, up to the next "## " heading. -->

## 0.8.0

* ⬆️ Port Aura to Cosmos gen3 (NativeAOT, .NET 10, Limine bootloader)
* ✨ Implement screen resolution change in settings app
* ✨ Add change wallpaper option in settings app
* ✨ Implement password textbox
* ✨ Add tab + enter on login screen
* ✨ Add textbox cursor change (left / right arrow)
* ✨ Implement logs command
* ✨ Scan boot.bat on all devices
* ✨ Add a keyboard layout switcher to the taskbar and login screen
* ✨ Add GB, DE, ES, TR and Dvorak keyboard layouts
* ✨ Remember the keyboard layout across reboots
* ✨ Support NVMe and USB disks, with USB hot-plug
* ✨ Improve Lua: plain scripts, `arg`, `os.execute`, `os.getenv`, error tracebacks
* ✨ Show more system details in `lsprocess`, `lspci`, `lsres` and Memory Info
* 💥 Switch to Unix-style paths (`/0/Users/...`), `/` lists the volumes
* ⚡️ Run the FTP server in the background (`ftp /stop`, custom port and PASV IP)
* ⚡️ Improve `wget`: binary-safe, keeps the file name, optional destination, follows redirects
* ⚡️ Improve network commands (`ipconfig /set` with CIDR, `udp` timeout, `ping` in ms)
* ⚡️ Improve the `vol` command (labels, partitions, MBR creation)
* ⚡️ Allow dots and nested paths in `mkdir`, copy into a folder with `cp`
* ⚡️ Flush disks before shutdown and reboot
* 💄 Update window icons
* 💄 Scale wallpapers to the screen
* ⏪️ Change changeres to lsres
* 🔒️ Fix the Windows key opening the start menu on the login screen
* 🔒️ Stop zip extraction from writing outside the target folder
* 🐛 Fix textbox multiline
* 🐛 Fix settings textboxes
* 🐛 Fix AltGr characters on French AZERTY
* 🐛 Fix GameBoy keys never being released
* 🐛 Fix several crashes (login Tab, empty terminal history, new Editor document, bad images, bad ROMs, bad resolution input)
* 🔥 Remove the `beep` command and the files shipped on the ISO (`HelloWorld.gb`)

Known issues:
* No HTTPS yet: the update checker and the package manager can't reach their servers
* A CPU fault freezes the system without Aura's crash screen (details on the serial port)
* IDE/PATA disks aren't supported, use SATA, NVMe or USB
* Only e1000e and virtio-net network cards work
* The resolution is fixed at boot, except on VMware SVGA II

For devs:
* ⬆️ Build with Cosmos.Sdk 3.0.89 and .NET 10, pinned in `global.json`
* 🔥 Merge Aura_Boot into Aura_OS and remove Aura_Plugs
* ➖ Replace the bundled UniLua with the Cosmos.Executable.Lua package
* ✨ Embed all assets as resources (`Files.Get` / `TryGet`)
* ⚡️ Rewrite DirectBitmap to draw straight into the screen buffer (one present per frame)
* 👷 Build the ISO on every commit to main and release it when the version is bumped
