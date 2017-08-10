# Alve Operating System
A Cosmos based Operating System developped in C# made by Alexy DA CRUZ (GeomTech) and Valentin Charbonnier (valentinbreiz).

## Current features
Please read the [TODO](https://github.com/Alve-OS/Alve-Operating-System/blob/master/TODO.md) file to know what will be added soon.

* Restart.
* Shutdown.
* Basic command interpreter.
* Virtual FileSystem.
* Multilanguage support.
* Exception with screen of death.
* Extended ASCII support.
* Multi users.
* Secured Users With MD5 Encryption.
* Text Editor (Liquid Editor)

## Screenshots

Login:

![Alve Operating System](https://image.noelshack.com/fichiers/2017/32/4/1502379821-alve4.png)

Shell:

![Alve Operating System](https://image.noelshack.com/fichiers/2017/32/4/1502379822-alve5.png)

![Alve Operating System](https://image.noelshack.com/fichiers/2017/32/4/1502320901-alve2.png)

![Alve Operating System](https://image.noelshack.com/fichiers/2017/31/4/1501777813-alve5.png)

## How to compile Alve ?
First, clone [our modified version of Cosmos](https://github.com/Alve-OS/Cosmos), run the "install-VS2017.bat" file and wait until the installation is done. 

Now clone [this repository](https://github.com/Alve-OS/Alve-Operating-System) then inside the folder Alve OS, run Alve OS.sln and select "build" once Visual Studio 2017 has loaded.

## Commands

Shutdown (to do an ACPI Shutdown) :
```
shutdown
```

Reboot (to do a CPU Reboot) :
```
reboot
```

Clear (to clear the console)
```
clear
```

Echo (to echo some text)
```
echo text
```

Help (to show availables commands)
```
help
```

Cd .. (to navigate to the parent folder)
```
cd ..
```

Cd (to navigate to a folder)
```
cd directory
```

Dir (to list directories and files)
```
dir
```

Mkdir (to create a directory)
```
mkdir directory
```

Rmdir (to remove a directory)
```
rmdir directory
```

Mkfil (to create a file and edit it in Liquid Editor)
```
mkfil file.txt
```

Prfil (to edit a file in Liquid Editor)
```
prfil file.txt
```

Rmfil (to remove a file)
```
rmfil file.txt
```

Vol (to list volumes)
```
vol
```

Systeminfo (to display system informations)
```
systeminfo
```

Ver (to display system version and revision)
```
ver
```

TextColor (to change console foreground color)
```
textcolor 1 (choose an ID)
```

BackgroundColor (to change console background color)
```
backgroundcolor 1 (choose an ID)
```

Logout (to disconnect and change of user)
```
logout
```

Settings (to access to the settings of Alve)
```
settings {args}
```


