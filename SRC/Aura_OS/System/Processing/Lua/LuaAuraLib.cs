/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lua aura library: what a package's code reaches of the kernel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Cosmos.Executable.Lua;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.System.FileSystem;
using Cosmos.Kernel.System.Graphics;
using Aura_OS.Processing;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Graphics.UI.GUI.Layout;
using Aura_OS.System.Network;
using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Interpreter;
using Aura_OS.System.Processing.Interpreter.Commands.Filesystem;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Users;
using Aura_OS.System.Utils;
using UIConsole = Aura_OS.System.Graphics.UI.GUI.Components.Console;
using AuraVersion = Aura_OS.System.Network.Version;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Aura_OS.System.Processing.Lua
{
    /// <summary>
    /// The aura library (global, and require "aura"): the app's window and controls, and the system's
    /// settings. SRC/Packages/README.md is its reference; keep both in step. Strings cross as text:
    /// LuaObject.CheckText and PushText convert them from and to the Lua strings' UTF-8 bytes.
    /// </summary>
    internal static class LuaAuraLib
    {
        public const string LIB_NAME = "aura";

        private const string AppType = "aura.app";
        private const string ControlType = "aura.control";

        /// <summary>
        /// An element found by id: what a control userdata holds.
        /// </summary>
        private sealed class Control
        {
            public readonly PackageApp App;
            public readonly string Id;

            /// <summary>
            /// Null for a container (Stack, Panel, Grid, Row), which has only id and visible.
            /// </summary>
            public readonly Component Component;

            public Control(PackageApp app, string id, Component component)
            {
                App = app;
                Id = id;
                Component = component;
            }
        }

        /// <summary>
        /// Registers aura in package.loaded and as a global.
        /// </summary>
        /// <param name="package">The running package (aura.log names it).</param>
        /// <param name="app">The app for aura.app, null for a console program.</param>
        public static void Register(LuaInterpreter lua, Package package, PackageApp app)
        {
            lua.State.L_RequireF(LIB_NAME, l => Open(l, package, app), true);

            // L_RequireF leaves the library table on the stack
            lua.State.Pop(1);
        }

        private static int Open(ILuaState lua, Package package, PackageApp app)
        {
            lua.L_NewLib(new NameFuncPair[]
            {
                new NameFuncPair("log", l => Log(l, package)),
            });

            SetObject(lua, "system", SystemObject());
            SetObject(lua, "user", UserObject());
            SetObject(lua, "network", NetworkObject());
            SetObject(lua, "memory", MemoryObject());
            SetObject(lua, "display", DisplayObject());
            SetObject(lua, "desktop", DesktopObject());
            SetObject(lua, "theme", ThemeObject());
            SetObject(lua, "settings", SettingsObject());
            SetObject(lua, "fs", FsObject());
            SetObject(lua, "disks", DisksObject());
            SetObject(lua, "clipboard", ClipboardObject());
            SetObject(lua, "shell", ShellObject());
            SetObject(lua, "packages", PackagesObject());
            SetObject(lua, "processes", ProcessesObject(app));

            if (app != null)
            {
                NewMetaTable(lua, AppType, AppIndex, AppNewIndex);
                NewMetaTable(lua, ControlType, ControlIndex, ControlNewIndex);

                lua.NewUserData(app);
                lua.L_SetMetaTable(AppType);
                lua.SetField(-2, "app");
            }

            return 1;
        }

        private static void SetObject(ILuaState lua, string name, LuaObject value)
        {
            value.Push(lua);
            lua.SetField(-2, name);
        }

        private static void NewMetaTable(ILuaState lua, string name, CSharpFunctionDelegate index, CSharpFunctionDelegate newIndex)
        {
            lua.L_NewMetaTable(name);
            lua.PushCSharpFunction(index);
            lua.SetField(-2, "__index");
            lua.PushCSharpFunction(newIndex);
            lua.SetField(-2, "__newindex");
            lua.Pop(1);
        }

        /// <summary>
        /// aura.log(text): a line in the OS log, after the package's name.
        /// </summary>
        private static int Log(ILuaState lua, Package package)
        {
            Logs.DoOSLog("[" + package.Name + "] " + LuaObject.CheckText(lua, 1));
            return 0;
        }

        #region Modules

        private static LuaObject SystemObject()
        {
            return new LuaObject("aura.system")
                .Property("version", lua => Push(lua, Kernel.Version ?? ""))
                .Property("revision", lua => Push(lua, Kernel.Revision ?? ""))
                .Property("installed", lua => Push(lua, Kernel.Installed))
                // Milliseconds since boot.
                .Property("uptime", lua => Push(lua, Environment.TickCount64))
                .Property("computerName", lua => Push(lua, Kernel.ComputerName ?? ""), lua =>
                {
                    Kernel.ComputerName = LuaObject.CheckText(lua, 3);

                    if (!string.IsNullOrEmpty(Kernel.ComputerName))
                    {
                        Cosmos.Kernel.System.Network.DnsConfig.HostName = Kernel.ComputerName;
                    }

                    return 0;
                })
                // Raises an error when os.json cannot be downloaded: call it with pcall.
                .Function("latestRelease", lua =>
                {
                    (string version, string revision, string url) = AuraVersion.GetLastVersionInfo();

                    LuaObject.PushText(lua, version);
                    LuaObject.PushText(lua, revision);
                    LuaObject.PushText(lua, url);
                    return 3;
                })
                .Function("compareVersions", lua => Push(lua, Math.Sign(AuraVersion.CompareVersions(LuaObject.CheckText(lua, 1), LuaObject.CheckText(lua, 2)))))
                .Function("compareRevisions", lua => Push(lua, Math.Sign(AuraVersion.CompareRevisions(LuaObject.CheckText(lua, 1), LuaObject.CheckText(lua, 2)))));
        }

        private static LuaObject UserObject()
        {
            return new LuaObject("aura.user")
                .Property("name", lua => Push(lua, Kernel.userLogged ?? ""), lua =>
                {
                    Kernel.userLogged = LuaObject.CheckText(lua, 3);
                    return 0;
                })
                // The prompt's sign of the user's level.
                .Property("level", lua => Push(lua, UserLevel.TypeUser ?? ""))
                // The user's folder (cd ~), nil when there is none (live mode).
                .Property("directory", lua => Text(lua, Kernel.UserDirectory));
        }

        private static LuaObject NetworkObject()
        {
            return new LuaObject("aura.network")
                .Function("isConfigured", lua => Push(lua, NetworkHelper.IsConfigured));
        }

        /// <summary>
        /// The page allocator and the garbage collector. Each read is a fresh value.
        /// </summary>
        private static LuaObject MemoryObject()
        {
            return new LuaObject("aura.memory")
                // GEN3-GAP(meminfo): the page allocator's pool is the largest usable memory-map region only.
                .Property("totalPages", lua => Push(lua, (long)MemoryDiagnostics.TotalPages))
                .Property("freePages", lua => Push(lua, (long)MemoryDiagnostics.FreePages))
                .Property("pageSize", lua => Push(lua, (long)MemoryDiagnostics.PageSizeBytes))
                .Property("liveHeap", lua => Push(lua, GC.GetTotalMemory(false)))
                .Property("collections", lua => Push(lua, MemoryDiagnostics.TotalCollections))
                .Property("objectsFreed", lua => Push(lua, MemoryDiagnostics.TotalObjectsFreed))
                .Property("gcTimePercent", lua => Push(lua, MemoryDiagnostics.GcTimePercent))
                // Objects the kernel's last periodic collection freed (Kernel.Run).
                .Property("lastFreed", lua => Push(lua, Kernel.FreeCount))
                // lastCollection(): the last collection's figures. It allocates: read it when collections changed.
                .Function("lastCollection", lua =>
                {
                    GCMemoryInfo info = GC.GetGCMemoryInfo();

                    lua.CreateTable(0, 4);
                    lua.PushInteger(info.HeapSizeBytes);
                    lua.SetField(-2, "heapSize");
                    lua.PushInteger(info.TotalCommittedBytes);
                    lua.SetField(-2, "committed");
                    lua.PushInteger(info.FragmentedBytes);
                    lua.SetField(-2, "fragmented");
                    lua.PushInteger(info.PinnedObjectsCount);
                    lua.SetField(-2, "pinnedObjects");
                    return 1;
                });
        }

        /// <summary>
        /// The display mode (Kernel.Canvas), not the UI size, which is divided by the scale.
        /// </summary>
        private static LuaObject DisplayObject()
        {
            return new LuaObject("aura.display")
                .Property("width", lua => Push(lua, Kernel.Canvas != null ? Kernel.Canvas.Width : 0))
                .Property("height", lua => Push(lua, Kernel.Canvas != null ? Kernel.Canvas.Height : 0))
                .Property("scale", lua => Push(lua, Kernel.ScreenScale))
                // GEN3-GAP(display-mode): only VMware SVGA II lists modes; on GOP/virtio-gpu the list
                // holds just the mode limine.conf set at boot.
                .Function("modes", lua =>
                {
                    lua.NewTable();

                    if (Kernel.Canvas != null)
                    {
                        int i = 1;

                        foreach (Mode mode in Kernel.Canvas.AvailableModes)
                        {
                            lua.CreateTable(0, 2);
                            lua.PushInteger(mode.Width);
                            lua.SetField(-2, "width");
                            lua.PushInteger(mode.Height);
                            lua.SetField(-2, "height");
                            lua.RawSetI(-2, i++);
                        }
                    }

                    return 1;
                })
                .Function("scales", lua =>
                {
                    lua.CreateTable(Explorer.Scales.Length, 0);

                    for (int i = 0; i < Explorer.Scales.Length; i++)
                    {
                        lua.PushInteger(Explorer.Scales[i]);
                        lua.RawSetI(-2, i + 1);
                    }

                    return 1;
                })
                // setMode(width, height, scale): true, or false and why the display or memory refused it.
                .Function("setMode", lua =>
                {
                    string error;
                    bool changed = Explorer.ChangeResolution((int)lua.L_CheckInteger(1), (int)lua.L_CheckInteger(2), (int)lua.L_CheckInteger(3), out error);

                    lua.PushBoolean(changed);
                    LuaObject.PushText(lua, error);
                    return 2;
                });
        }

        private static LuaObject DesktopObject()
        {
            return new LuaObject("aura.desktop")
                .Property("wallpaper", lua => Text(lua, Explorer.Desktop.GetWallpaperPath()))
                // Raises an error for a BMP the loader rejects; a missing file shows the default wallpaper.
                .Function("setWallpaper", lua =>
                {
                    Explorer.Desktop.SetWallpaper(LuaObject.CheckText(lua, 1));
                    return 0;
                })
                .Property("windowsAlpha", lua => Push(lua, Explorer.WindowManager.WindowsTransparency), lua =>
                {
                    Explorer.WindowManager.WindowsTransparency = CheckByte(lua, 3);
                    return 0;
                })
                .Property("taskbarAlpha", lua => Push(lua, Explorer.WindowManager.TaskbarTransparency), lua =>
                {
                    Explorer.WindowManager.TaskbarTransparency = CheckByte(lua, 3);
                    return 0;
                })
                .Property("guiDebug", lua => Push(lua, Kernel.GuiDebug), lua =>
                {
                    Kernel.GuiDebug = lua.ToBoolean(3);
                    return 0;
                })
                // The frames the desktop drew in the last second.
                .Property("fps", lua => Push(lua, Kernel.Fps))
                // open(path): a folder in the File Explorer, a file in its app; true, or false and why.
                .Function("open", lua =>
                {
                    string path = AuraPath.Resolve(LuaObject.CheckText(lua, 1));
                    return Try(lua, () => Open(path));
                })
                // start(name, ...): opens the app package with that name and arguments; true, or false and why.
                .Function("start", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    List<string> args = new List<string>();

                    for (int i = 2; i <= lua.GetTop(); i++)
                    {
                        // L_ToString pushes the text too (luaL_tolstring)
                        args.Add(LuaText.Decode(lua.L_ToString(i)));
                        lua.Pop(1);
                    }

                    return Try(lua, () =>
                    {
                        Package package = Kernel.PackageManager.Find(name);

                        if (package == null || !package.IsApp)
                        {
                            throw new InvalidOperationException("No app is named '" + name + "'.");
                        }

                        Kernel.ApplicationManager.StartPackage(package, args);
                    });
                });
        }

        /// <summary>
        /// Opens a folder in the File Explorer, a file in its app (ApplicationManager.StartFileApplication).
        /// </summary>
        private static void Open(string path)
        {
            if (Directory.Exists(path))
            {
                Kernel.ApplicationManager.OpenFolder(AuraPath.AsDirectory(path));
            }
            else if (File.Exists(path))
            {
                Kernel.ApplicationManager.StartFileApplication(Path.GetFileName(path), Path.GetDirectoryName(path));
            }
            else
            {
                throw new FileNotFoundException("'" + path + "' does not exist.");
            }
        }

        /// <summary>
        /// The theme files, loaded at boot: a change shows after a reboot.
        /// </summary>
        private static LuaObject ThemeObject()
        {
            return new LuaObject("aura.theme")
                .Property("bmpPath", lua => Text(lua, Kernel.ThemeManager.BmpPath), lua =>
                {
                    Kernel.ThemeManager.BmpPath = LuaObject.CheckText(lua, 3);
                    return 0;
                })
                .Property("xmlPath", lua => Text(lua, Kernel.ThemeManager.XmlPath), lua =>
                {
                    Kernel.ThemeManager.XmlPath = LuaObject.CheckText(lua, 3);
                    return 0;
                });
        }

        /// <summary>
        /// settings.ini, which only an installed system has.
        /// </summary>
        private static LuaObject SettingsObject()
        {
            return new LuaObject("aura.settings")
                // get(key): the value, nil when unset or not installed.
                .Function("get", lua =>
                {
                    string key = LuaObject.CheckText(lua, 1);
                    return Text(lua, Kernel.Installed ? new Settings(AuraPaths.SettingsIni).GetValue(key) : null);
                })
                // save({ key = value, ... }): writes the pairs in one go; booleans as true/false.
                .Function("save", lua =>
                {
                    lua.L_CheckType(1, LuaType.LUA_TTABLE);

                    if (!Kernel.Installed)
                    {
                        return LuaObject.Error(lua, "Aura is not installed: there is no settings.ini");
                    }

                    Settings config = new Settings(AuraPaths.SettingsIni);

                    lua.PushNil();
                    while (lua.Next(1))
                    {
                        if (lua.Type(-2) != LuaType.LUA_TSTRING)
                        {
                            return LuaObject.Error(lua, "settings keys are strings");
                        }

                        config.EditValue(LuaText.Decode(lua.ToString(-2)), SettingText(lua, -1));
                        lua.Pop(1);
                    }

                    config.Push();
                    return 0;
                });
        }

        private static LuaObject FsObject()
        {
            return new LuaObject("aura.fs")
                // The shell's current directory (cd), always ending with '/'. Setting it is a cd: an
                // error for a folder that does not exist.
                .Property("currentDirectory", lua => Push(lua, Kernel.CurrentDirectory), lua =>
                {
                    string error;

                    if (!CurrentPath.Set(LuaObject.CheckText(lua, 3), out error))
                    {
                        return LuaObject.Error(lua, error);
                    }

                    return 0;
                })
                // resolve(path): absolute, from the current directory; gen2 paths (0:\Users) are converted.
                .Function("resolve", lua => Push(lua, AuraPath.Resolve(LuaObject.CheckText(lua, 1))))
                .Function("fileExists", lua => Push(lua, File.Exists(AuraPath.Resolve(LuaObject.CheckText(lua, 1)))))
                .Function("directoryExists", lua => Push(lua, Directory.Exists(AuraPath.Resolve(LuaObject.CheckText(lua, 1)))))
                .Function("readText", ReadText)
                .Function("writeText", WriteText)
                .Function("list", ListDirectory)
                .Function("volumes", ListVolumes)
                // space(path): the free and total bytes of the volume holding the path, nil for none.
                .Function("space", lua =>
                {
                    VfsMount mount = Volumes.MountOf(AuraPath.Resolve(LuaObject.CheckText(lua, 1)));
                    ulong free, total;

                    if (mount == null || !Volumes.TryGetSpace(mount.MountPoint, out free, out total))
                    {
                        lua.PushNil();
                        return 1;
                    }

                    lua.PushInteger((long)free);
                    lua.PushInteger((long)total);
                    return 2;
                })
                // createDirectory(path): with its missing parents; true, or false and why.
                .Function("createDirectory", lua =>
                {
                    string path = AuraPath.Resolve(LuaObject.CheckText(lua, 1));
                    return Try(lua, () =>
                    {
                        if (File.Exists(path))
                        {
                            throw new IOException("'" + Path.GetFileName(path) + "' is a file.");
                        }

                        Directory.CreateDirectory(path);
                    });
                })
                // delete(path): a file, or a folder and everything in it; true, or false and why.
                .Function("delete", lua =>
                {
                    string path = AuraPath.Resolve(LuaObject.CheckText(lua, 1));
                    return Try(lua, () => Entries.Delete(path));
                })
                // copy(source, destination): to a path that does not exist yet; true, or false and why.
                .Function("copy", lua =>
                {
                    string source = AuraPath.Resolve(LuaObject.CheckText(lua, 1));
                    string destination = AuraPath.Resolve(LuaObject.CheckText(lua, 2));
                    return Try(lua, () => Entries.Copy(source, destination));
                })
                // move(source, destination): moves or renames; true, or false and why.
                .Function("move", lua =>
                {
                    string source = AuraPath.Resolve(LuaObject.CheckText(lua, 1));
                    string destination = AuraPath.Resolve(LuaObject.CheckText(lua, 2));
                    return Try(lua, () => Entries.Move(source, destination));
                })
                // The path the desktop's and the File Explorer's Copy keep, nil for none.
                .Property("clipboard", lua => Text(lua, Kernel.Clipboard), lua =>
                {
                    Kernel.Clipboard = lua.IsNoneOrNil(3) ? null : AuraPath.Resolve(LuaObject.CheckText(lua, 3));
                    return 0;
                });
        }

        /// <summary>
        /// aura.fs.list(path): the entries of a folder, { name = , directory = , size = } (bytes, 0 for
        /// a folder), in no order; or nil and why. At "/" they are the volumes.
        /// </summary>
        private static int ListDirectory(ILuaState lua)
        {
            string path = AuraPath.Resolve(LuaObject.CheckText(lua, 1));
            string[] directories = null;
            string[] files = null;
            string error = null;

            try
            {
                if (Directory.Exists(path))
                {
                    directories = Directory.GetDirectories(path);
                    files = Directory.GetFiles(path);
                }
                else
                {
                    error = File.Exists(path) ? "'" + path + "' is a file." : "The folder '" + path + "' does not exist.";
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (error != null)
            {
                lua.PushNil();
                LuaObject.PushText(lua, error);
                return 2;
            }

            lua.CreateTable(directories.Length + files.Length, 0);
            int count = 0;

            // Both give full paths.
            for (int i = 0; i < directories.Length; i++)
            {
                PushEntry(lua, Path.GetFileName(directories[i].TrimEnd(AuraPath.Separator)), true, 0);
                lua.RawSetI(-2, ++count);
            }

            for (int i = 0; i < files.Length; i++)
            {
                PushEntry(lua, Path.GetFileName(files[i]), false, FileSize(files[i]));
                lua.RawSetI(-2, ++count);
            }

            return 1;
        }

        private static void PushEntry(ILuaState lua, string name, bool directory, long size)
        {
            lua.CreateTable(0, 3);
            SetText(lua, "name", name);
            lua.PushBoolean(directory);
            lua.SetField(-2, "directory");
            lua.PushInteger(size);
            lua.SetField(-2, "size");
        }

        /// <summary>
        /// The size of a file in bytes, 0 when unknown. The VFS stat, rather than a FileInfo.
        /// </summary>
        private static long FileSize(string fullPath)
        {
            VfsStat stat;

            try
            {
                return VfsManager.TryStat(fullPath, out stat) ? (long)stat.Size : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// aura.fs.volumes(): the mounted volumes, { path = "/0/", filesystem = "FAT32", label = }
        /// ("" for none), by mount point.
        /// </summary>
        private static int ListVolumes(ILuaState lua)
        {
            IReadOnlyList<VfsMount> mounts = VfsManager.Mounts;
            List<VfsMount> sorted = new List<VfsMount>(mounts.Count);

            for (int i = 0; i < mounts.Count; i++)
            {
                sorted.Add(mounts[i]);
            }

            // "/2" before "/10": by length first.
            sorted.Sort((a, b) => a.MountPoint.Length != b.MountPoint.Length
                ? a.MountPoint.Length - b.MountPoint.Length
                : string.CompareOrdinal(a.MountPoint, b.MountPoint));

            lua.CreateTable(sorted.Count, 0);

            for (int i = 0; i < sorted.Count; i++)
            {
                VfsMount mount = sorted[i];
                string label = "";
                string filesystem = mount.Partition != null ? Disks.DetectFilesystem(mount.Partition, out label) : mount.Name;

                lua.CreateTable(0, 3);
                SetText(lua, "path", AuraPath.AsDirectory(mount.MountPoint));
                SetText(lua, "filesystem", filesystem);
                SetText(lua, "label", label);
                lua.RawSetI(-2, i + 1);
            }

            return 1;
        }

        /// <summary>
        /// aura.fs.readText(path): the whole file as text (UTF-8, without its BOM), or nil and why.
        /// GEN3-GAP(lua-host): io's read takes the file a byte at a time, through an unbuffered FileStream.
        /// </summary>
        private static int ReadText(ILuaState lua)
        {
            string path = AuraPath.Resolve(LuaObject.CheckText(lua, 1));
            string text = null;
            string error = null;

            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (error != null)
            {
                lua.PushNil();
                LuaObject.PushText(lua, error);
                return 2;
            }

            return Push(lua, text);
        }

        /// <summary>
        /// aura.fs.writeText(path, text): replaces the file with the text (UTF-8), true, or false and why.
        /// </summary>
        private static int WriteText(ILuaState lua)
        {
            string path = AuraPath.Resolve(LuaObject.CheckText(lua, 1));
            string text = LuaObject.CheckText(lua, 2);
            string error = null;

            try
            {
                File.WriteAllText(path, text);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            lua.PushBoolean(error == null);
            LuaObject.PushText(lua, error);
            return 2;
        }

        /// <summary>
        /// The disks and their partitions (Disks), as the Disk Manager changes them. A disk is named by
        /// its device name, a partition by its disk and the sector it starts at. The actions run on the
        /// UI thread: the desktop waits for them.
        /// </summary>
        private static LuaObject DisksObject()
        {
            return new LuaObject("aura.disks")
                .Function("list", ListDisks)
                // createTable(disk, "MBR" or "GPT"): a new, empty partition table; true, or false and why.
                .Function("createTable", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    string table = LuaObject.CheckText(lua, 2);
                    lua.L_ArgCheck(table == "MBR" || table == "GPT", 2, "\"MBR\" or \"GPT\" expected");
                    return Try(lua, () => Disks.CreateTable(Disks.FindDisk(name), table == "GPT"));
                })
                // create(disk, start, sectors, filesystem[, label]): true and where the partition starts, or
                // false and why.
                .Function("create", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    ulong start = CheckSector(lua, 2);
                    ulong sectors = CheckSector(lua, 3);
                    string filesystem = LuaObject.CheckText(lua, 4);
                    string label = lua.IsNoneOrNil(5) ? null : LuaObject.CheckText(lua, 5);
                    ulong begin = 0;
                    string error = null;

                    try
                    {
                        begin = Disks.CreatePartition(Disks.FindDisk(name), start, sectors, filesystem, label);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }

                    lua.PushBoolean(error == null);

                    if (error == null)
                    {
                        lua.PushInteger((long)begin);
                    }
                    else
                    {
                        LuaObject.PushText(lua, error);
                    }

                    return 2;
                })
                // delete(disk, start): true, or false and why.
                .Function("delete", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    ulong start = CheckSector(lua, 2);
                    return Try(lua, () =>
                    {
                        IBlockDevice disk = Disks.FindDisk(name);
                        Disks.DeletePartition(disk, Disks.FindPartition(disk, start));
                    });
                })
                // format(disk, start, filesystem[, label]): true, or false and why.
                .Function("format", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    ulong start = CheckSector(lua, 2);
                    string filesystem = LuaObject.CheckText(lua, 3);
                    string label = lua.IsNoneOrNil(4) ? null : LuaObject.CheckText(lua, 4);
                    return Try(lua, () =>
                    {
                        IBlockDevice disk = Disks.FindDisk(name);
                        Disks.Format(disk, Disks.FindPartition(disk, start), filesystem, label);
                    });
                })
                // resize(disk, start, newStart, newSectors): moves and resizes; true, or false and why.
                .Function("resize", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    ulong start = CheckSector(lua, 2);
                    ulong newStart = CheckSector(lua, 3);
                    ulong newSectors = CheckSector(lua, 4);
                    return Try(lua, () =>
                    {
                        IBlockDevice disk = Disks.FindDisk(name);
                        Disks.Resize(disk, Disks.FindPartition(disk, start), newStart, newSectors);
                    });
                })
                // setLabel(disk, start, label): a FAT or ext2 volume's label; true, or false and why.
                .Function("setLabel", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    ulong start = CheckSector(lua, 2);
                    string label = LuaObject.CheckText(lua, 3);
                    return Try(lua, () => Disks.SetLabel(Disks.FindPartition(Disks.FindDisk(name), start), label));
                })
                // mount(disk, start): true and the mount point ("/1/"), or false and why.
                .Function("mount", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    ulong start = CheckSector(lua, 2);
                    string mountPoint = null;
                    string error = null;

                    try
                    {
                        mountPoint = Disks.Mount(Disks.FindPartition(Disks.FindDisk(name), start));
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }

                    lua.PushBoolean(error == null);
                    LuaObject.PushText(lua, error ?? mountPoint);
                    return 2;
                })
                // unmount(disk, start): true, or false and why.
                .Function("unmount", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    ulong start = CheckSector(lua, 2);
                    return Try(lua, () => Disks.Unmount(Disks.FindPartition(Disks.FindDisk(name), start)));
                });
        }

        /// <summary>
        /// aura.disks.list(): the disks, each { name = , size = , sectors = , sectorSize = , table = ,
        /// usableStart = , usableEnd = , extended = , primaries = , error = , partitions = }.
        /// </summary>
        private static int ListDisks(ILuaState lua)
        {
            List<DiskInfo> disks = Disks.List();
            lua.CreateTable(disks.Count, 0);

            for (int i = 0; i < disks.Count; i++)
            {
                DiskInfo disk = disks[i];
                IBlockDevice device = disk.Device;

                lua.CreateTable(0, 11);
                SetText(lua, "name", device.Name);
                SetInteger(lua, "size", (long)(device.BlockCount * device.BlockSize));
                SetInteger(lua, "sectors", (long)device.BlockCount);
                SetInteger(lua, "sectorSize", (long)device.BlockSize);
                SetText(lua, "table", TableName(disk.Table));
                SetInteger(lua, "usableStart", (long)disk.FirstUsable);
                SetInteger(lua, "usableEnd", (long)disk.EndUsable);
                SetInteger(lua, "primaries", disk.PrimaryCount);

                if (disk.ExtendedSectors > 0)
                {
                    lua.CreateTable(0, 2);
                    SetInteger(lua, "start", (long)disk.ExtendedStart);
                    SetInteger(lua, "sectors", (long)disk.ExtendedSectors);
                    lua.SetField(-2, "extended");
                }

                if (disk.Error != null)
                {
                    SetText(lua, "error", disk.Error);
                }

                lua.CreateTable(disk.Partitions.Count, 0);

                for (int j = 0; j < disk.Partitions.Count; j++)
                {
                    PushPartition(lua, disk.Partitions[j]);
                    lua.RawSetI(-2, j + 1);
                }

                lua.SetField(-2, "partitions");
                lua.RawSetI(-2, i + 1);
            }

            return 1;
        }

        /// <summary>
        /// { name = , start = , sectors = , size = , volumeSectors = , filesystem = , label = , mountPoint = ,
        /// system = , logical = , boot = , mbrType = , gptType = }
        /// </summary>
        private static void PushPartition(ILuaState lua, PartitionInfo info)
        {
            Cosmos.Kernel.System.Storage.Partition partition = info.Partition;

            lua.CreateTable(0, 13);
            SetText(lua, "name", partition.Name);
            SetInteger(lua, "start", (long)partition.StartSector);
            SetInteger(lua, "sectors", (long)partition.BlockCount);
            SetInteger(lua, "size", (long)(partition.BlockCount * partition.BlockSize));
            SetInteger(lua, "volumeSectors", (long)info.VolumeSectors);
            SetText(lua, "filesystem", info.Filesystem);
            SetText(lua, "label", info.Label);

            if (info.MountPoint != null)
            {
                SetText(lua, "mountPoint", info.MountPoint);
            }

            lua.PushBoolean(info.System);
            lua.SetField(-2, "system");
            lua.PushBoolean(info.Logical);
            lua.SetField(-2, "logical");
            lua.PushBoolean(info.Boot);
            lua.SetField(-2, "boot");

            if (info.MbrType >= 0)
            {
                SetInteger(lua, "mbrType", info.MbrType);
            }

            if (info.GptType.HasValue)
            {
                SetText(lua, "gptType", info.GptType.Value.ToString("D").ToUpperInvariant());
            }
        }

        private static string TableName(PartitionTable table)
        {
            switch (table)
            {
                case PartitionTable.Mbr:
                    return "MBR";
                case PartitionTable.Gpt:
                    return "GPT";
                case PartitionTable.Whole:
                    return "whole";
                default:
                    return "none";
            }
        }

        /// <summary>
        /// A Canvas's cursor by its name in Lua: the names of CursorState, the first letter small.
        /// </summary>
        private static string CursorName(Aura_OS.System.Input.CursorState cursor)
        {
            switch (cursor)
            {
                case Aura_OS.System.Input.CursorState.ResizeHorizontal:
                    return "resizeHorizontal";
                case Aura_OS.System.Input.CursorState.ResizeVertical:
                    return "resizeVertical";
                case Aura_OS.System.Input.CursorState.Grab:
                    return "grab";
                default:
                    return "normal";
            }
        }

        private static bool TryParseCursor(string name, out Aura_OS.System.Input.CursorState cursor)
        {
            switch (name)
            {
                case "normal":
                    cursor = Aura_OS.System.Input.CursorState.Normal;
                    return true;
                case "resizeHorizontal":
                    cursor = Aura_OS.System.Input.CursorState.ResizeHorizontal;
                    return true;
                case "resizeVertical":
                    cursor = Aura_OS.System.Input.CursorState.ResizeVertical;
                    return true;
                case "grab":
                    cursor = Aura_OS.System.Input.CursorState.Grab;
                    return true;
            }

            cursor = Aura_OS.System.Input.CursorState.Normal;
            return false;
        }

        /// <summary>
        /// A sector number or count argument: a whole number, 0 or more.
        /// </summary>
        private static ulong CheckSector(ILuaState lua, int index)
        {
            long value = lua.L_CheckInteger(index);
            lua.L_ArgCheck(value >= 0, index, "a sector is 0 or more");
            return (ulong)value;
        }

        /// <summary>
        /// The text clipboard, Ctrl+C and Ctrl+V's (the desktop's copied file is aura.fs.clipboard).
        /// </summary>
        private static LuaObject ClipboardObject()
        {
            return new LuaObject("aura.clipboard")
                // The text Ctrl+V pastes, nil for none. Setting it copies a text, as Ctrl+C does.
                .Property("text", lua => Text(lua, TextClipboard.Text), lua =>
                {
                    TextClipboard.Copy(LuaObject.CheckText(lua, 3));
                    return 0;
                })
                // The texts copied, newest first: text, then the ones before it.
                .Function("history", lua =>
                {
                    IReadOnlyList<string> history = TextClipboard.History;
                    lua.CreateTable(history.Count, 0);

                    for (int i = 0; i < history.Count; i++)
                    {
                        LuaObject.PushText(lua, history[i]);
                        lua.RawSetI(-2, i + 1);
                    }

                    return 1;
                });
        }

        /// <summary>
        /// aura.shell.open(console): a shell in one of the app's Console controls, whose execute(line)
        /// runs a command line. While the app is focused, Console.Out (the commands' output) writes
        /// into the console; clear empties it, exit closes the app.
        /// </summary>
        private static LuaObject ShellObject()
        {
            return new LuaObject("aura.shell")
                .Function("open", lua =>
                {
                    Control control = (Control)lua.L_CheckUData(1, ControlType);
                    UIConsole console = control.Component as UIConsole;

                    if (console == null)
                    {
                        return LuaObject.Error(lua, "'" + control.Id + "' is not a Console");
                    }

                    ShellSession session = control.App.OpenShell(console);

                    new LuaObject("shell")
                        .Function("execute", l =>
                        {
                            session.Execute(LuaObject.CheckText(l, 1));
                            return 0;
                        })
                        .Push(lua);
                    return 1;
                });
        }

        /// <summary>
        /// The packages Aura has and the online repository's (Kernel.PackageManager), as pkg manages
        /// them. The downloads run on the UI thread: the desktop waits for them.
        /// </summary>
        private static LuaObject PackagesObject()
        {
            return new LuaObject("aura.packages")
                .Property("repository", lua => Push(lua, Kernel.PackageManager.RepositoryUrl))
                .Property("defaultRepository", lua => Push(lua, PackageManager.DefaultRepository))
                // setRepository(url): true, or false and why; settings.ini keeps it on an installed Aura.
                .Function("setRepository", lua =>
                {
                    string url = LuaObject.CheckText(lua, 1);
                    return Try(lua, () => Kernel.PackageManager.SetRepository(url));
                })
                // list(): the packages Aura has, built in or installed.
                .Function("list", lua =>
                {
                    List<Package> packages = Kernel.PackageManager.Packages;
                    lua.CreateTable(packages.Count, 0);

                    for (int i = 0; i < packages.Count; i++)
                    {
                        Package package = packages[i];

                        lua.CreateTable(0, 8);
                        SetText(lua, "name", package.Name);
                        SetText(lua, "displayName", package.DisplayName);
                        SetText(lua, "version", package.Version);
                        SetText(lua, "author", package.Author);
                        SetText(lua, "description", package.Description);
                        lua.PushBoolean(package.BuiltIn);
                        lua.SetField(-2, "builtIn");
                        lua.PushBoolean(package.IsApp);
                        lua.SetField(-2, "app");
                        lua.PushBoolean(package.InMenu);
                        lua.SetField(-2, "menu");
                        lua.RawSetI(-2, i + 1);
                    }

                    return 1;
                })
                // available(): the repository's packages, as the last update() read them.
                .Function("available", lua =>
                {
                    List<RepositoryPackage> entries = Kernel.PackageManager.Repository;
                    lua.CreateTable(entries.Count, 0);

                    for (int i = 0; i < entries.Count; i++)
                    {
                        RepositoryPackage entry = entries[i];

                        lua.CreateTable(0, 6);
                        SetText(lua, "name", entry.Name);
                        SetText(lua, "displayName", entry.DisplayName ?? entry.Name);
                        SetText(lua, "version", entry.Version);
                        SetText(lua, "author", entry.Author);
                        SetText(lua, "description", entry.Description);
                        SetText(lua, "link", entry.Link);
                        lua.RawSetI(-2, i + 1);
                    }

                    return 1;
                })
                // update(): downloads the repository's package list; true, or false and why.
                .Function("update", lua => Try(lua, () => Kernel.PackageManager.Update()))
                // add(name): downloads a package of the list and installs it; true, or false and why.
                .Function("add", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    return Try(lua, () =>
                    {
                        bool saved;
                        Kernel.PackageManager.Add(name, out saved);
                    });
                })
                // remove(name): removes a downloaded package; true, or false and why.
                .Function("remove", lua =>
                {
                    string name = LuaObject.CheckText(lua, 1);
                    return Try(lua, () => Kernel.PackageManager.Remove(name));
                });
        }

        /// <summary>
        /// Aura's processes and the kernel's threads, as the Task Manager shows them. Every process runs
        /// on the main loop's thread: ProcessManager.Charge splits its time between them. The times are
        /// in nanoseconds; their growth over the clock's is a share of the CPU.
        /// </summary>
        /// <param name="caller">The calling app, null for a console program: it closes itself once its handler returns.</param>
        private static LuaObject ProcessesObject(PackageApp caller)
        {
            return new LuaObject("aura.processes")
                // list(): the processes, in their start order.
                .Function("list", lua =>
                {
                    ProcessManager manager = Kernel.ProcessManager;
                    manager.Flush();

                    List<Process> processes = manager.Processes;
                    lua.CreateTable(processes.Count, 0);

                    for (int i = 0; i < processes.Count; i++)
                    {
                        Process process = processes[i];
                        Application app = process as Application;
                        PackageApp packageApp = process as PackageApp;

                        lua.CreateTable(0, 8);
                        SetInteger(lua, "id", process.ID);
                        SetText(lua, "name", process.Name);
                        lua.PushBoolean(app != null);
                        lua.SetField(-2, "app");
                        lua.PushBoolean(process.Running);
                        lua.SetField(-2, "running");
                        lua.PushBoolean(app != null && app.Focused);
                        lua.SetField(-2, "focused");
                        LuaObject.PushText(lua, packageApp != null ? packageApp.Package.Name : null);
                        lua.SetField(-2, "package");
                        LuaObject.PushText(lua, app != null && app.Layout != null ? app.Layout.Icon : null);
                        lua.SetField(-2, "icon");
                        SetInteger(lua, "cpuTime", ProcessManager.Nanoseconds(process.CpuTime));
                        lua.RawSetI(-2, i + 1);
                    }

                    return 1;
                })
                // times(): { clock = , system = , threads = }, read together.
                .Function("times", lua =>
                {
                    ProcessManager manager = Kernel.ProcessManager;
                    manager.Flush();

                    lua.CreateTable(0, 3);
                    SetInteger(lua, "clock", ProcessManager.Nanoseconds(Stopwatch.GetTimestamp()));
                    SetInteger(lua, "system", ProcessManager.Nanoseconds(manager.SystemTime));
                    SetInteger(lua, "threads", (long)SchedulerDiagnostics.BusyCpuTimeNs);
                    return 1;
                })
                // threads(): the kernel's threads, in their registry's order.
                .Function("threads", lua =>
                {
                    int slots = SchedulerDiagnostics.ThreadSlotCount;
                    int count = 0;
                    lua.CreateTable(SchedulerDiagnostics.ThreadCount, 0);

                    for (int slot = 0; slot < slots; slot++)
                    {
                        KernelThreadInfo thread;

                        if (!SchedulerDiagnostics.TryGetThreadInSlot(slot, out thread) || thread.State == KernelThreadState.Dead)
                        {
                            continue;
                        }

                        lua.CreateTable(0, 8);
                        SetInteger(lua, "id", thread.Id);
                        LuaObject.PushText(lua, ThreadNames.Of(thread.Id));
                        lua.SetField(-2, "name");
                        SetText(lua, "state", ThreadStateName(thread.State));
                        lua.PushBoolean(thread.IsIdle);
                        lua.SetField(-2, "main");
                        lua.PushBoolean(thread.IsManaged);
                        lua.SetField(-2, "managed");
                        SetInteger(lua, "cpuTime", (long)thread.TotalRuntimeNs);
                        SetInteger(lua, "stack", (long)thread.StackSizeBytes);

                        if (thread.HasPriority)
                        {
                            SetInteger(lua, "priority", thread.Priority);
                        }

                        lua.RawSetI(-2, ++count);
                    }

                    return 1;
                })
                // close(id): closes an app; true, or false and why.
                .Function("close", lua =>
                {
                    long id = lua.L_CheckInteger(1);
                    return Try(lua, () =>
                    {
                        Application app = AppOfProcess(id);

                        if (app == caller)
                        {
                            // Disposing it would end the handler running now: as os.exit.
                            caller.RequestExit();
                        }
                        else
                        {
                            app.Dispose();
                        }
                    });
                })
                // switchTo(id): an app's window over the others, focused; true, or false and why.
                .Function("switchTo", lua =>
                {
                    long id = lua.L_CheckInteger(1);
                    return Try(lua, () => AppOfProcess(id).SwitchTo());
                });
        }

        /// <summary>
        /// The app process with that id.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is none, or the process is not an app.</exception>
        private static Application AppOfProcess(long id)
        {
            Process process = id >= 0 && id <= uint.MaxValue ? Kernel.ProcessManager.GetProcessByPid((uint)id) : null;

            if (process == null)
            {
                throw new InvalidOperationException("There is no process " + id + ".");
            }

            Application app = process as Application;

            if (app == null)
            {
                throw new InvalidOperationException(process.Name + " is part of the desktop: it has no window, and runs as long as Aura.");
            }

            return app;
        }

        /// <summary>
        /// A thread's state as aura.processes.threads gives it. A switch: NativeAOT keeps no enum names.
        /// </summary>
        private static string ThreadStateName(KernelThreadState state)
        {
            switch (state)
            {
                case KernelThreadState.Created:
                    return "created";
                case KernelThreadState.Ready:
                    return "ready";
                case KernelThreadState.Running:
                    return "running";
                case KernelThreadState.Blocked:
                    return "blocked";
                case KernelThreadState.Sleeping:
                    return "sleeping";
                default:
                    return "dead";
            }
        }

        #endregion

        #region aura.app

        private static int AppIndex(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            string key = LuaObject.CheckText(lua, 2);

            switch (key)
            {
                case "find":
                    lua.PushCSharpFunction(AppFind);
                    return 1;
                case "on":
                    lua.PushCSharpFunction(AppOn);
                    return 1;
                case "every":
                    lua.PushCSharpFunction(AppEvery);
                    return 1;
                case "after":
                    lua.PushCSharpFunction(AppAfter);
                    return 1;
                case "onKey":
                    lua.PushCSharpFunction(AppOnKey);
                    return 1;
                case "onResize":
                    lua.PushCSharpFunction(AppOnResize);
                    return 1;
                case "fit":
                    lua.PushCSharpFunction(AppFit);
                    return 1;
                case "title":
                    return Text(lua, app.Window.Name);
                case "focused":
                    lua.PushBoolean(app.Focused);
                    return 1;
            }

            return LuaObject.Error(lua, "the app has no field '" + key + "'");
        }

        private static int AppNewIndex(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            string key = LuaObject.CheckText(lua, 2);

            if (key == "title")
            {
                app.SetTitle(LuaObject.CheckText(lua, 3));
                return 0;
            }

            return LuaObject.Error(lua, "the app has no field '" + key + "' to set");
        }

        /// <summary>
        /// app:find(id): the element with that id in the layout file.
        /// </summary>
        private static int AppFind(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            string id = LuaObject.CheckText(lua, 2);

            // Throws (a Lua error) for an id the file does not have.
            Component component = app.Layout.FindComponent(id);

            lua.NewUserData(new Control(app, id, component));
            lua.L_SetMetaTable(ControlType);
            return 1;
        }

        /// <summary>
        /// app:on(name, handler): calls handler on the layout file's event of that name
        /// (onClick="name"...). A later call replaces it.
        /// </summary>
        private static int AppOn(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            string name = LuaObject.CheckText(lua, 2);
            lua.L_CheckType(3, LuaType.LUA_TFUNCTION);

            lua.PushValue(3);
            app.SetHandler(name, lua.L_Ref(LuaDef.LUA_REGISTRYINDEX));
            return 0;
        }

        /// <summary>
        /// app:every(milliseconds, handler): calls handler() every that many milliseconds, on the UI
        /// thread, until the app closes or the handler raises an error.
        /// </summary>
        private static int AppEvery(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            long interval = lua.L_CheckInteger(2);
            lua.L_ArgCheck(interval > 0, 2, "a positive number of milliseconds expected");
            lua.L_CheckType(3, LuaType.LUA_TFUNCTION);

            lua.PushValue(3);
            app.AddTimer(interval, lua.L_Ref(LuaDef.LUA_REGISTRYINDEX));
            return 0;
        }

        /// <summary>
        /// app:after(milliseconds, handler): calls handler() once, that many milliseconds from now and
        /// after the window was drawn, on the UI thread.
        /// </summary>
        private static int AppAfter(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            long delay = lua.L_CheckInteger(2);
            lua.L_ArgCheck(delay >= 0, 2, "a number of milliseconds expected");
            lua.L_CheckType(3, LuaType.LUA_TFUNCTION);

            lua.PushValue(3);
            app.AddOneShotTimer(delay, lua.L_Ref(LuaDef.LUA_REGISTRYINDEX));
            return 0;
        }

        /// <summary>
        /// app:onKey(handler): calls handler(key) with each key typed while the app is focused. A later
        /// call replaces it.
        /// </summary>
        private static int AppOnKey(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            lua.L_CheckType(2, LuaType.LUA_TFUNCTION);

            lua.PushValue(2);
            app.SetKeyHandler(lua.L_Ref(LuaDef.LUA_REGISTRYINDEX));
            return 0;
        }

        /// <summary>
        /// app:onResize(handler): calls handler() once the window was resized and its controls placed
        /// again. A later call replaces it.
        /// </summary>
        private static int AppOnResize(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            lua.L_CheckType(2, LuaType.LUA_TFUNCTION);

            lua.PushValue(2);
            app.SetResizeHandler(lua.L_Ref(LuaDef.LUA_REGISTRYINDEX));
            return 0;
        }

        /// <summary>
        /// app:fit(): resizes the window to its elements, as wide as its title at least, and no larger
        /// than the screen has room for.
        /// </summary>
        private static int AppFit(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            app.FitToLayout();
            return 0;
        }

        #endregion

        #region Controls

        private static int ControlIndex(ILuaState lua)
        {
            Control control = (Control)lua.L_CheckUData(1, ControlType);
            string key = LuaObject.CheckText(lua, 2);
            Component component = control.Component;

            switch (key)
            {
                case "id":
                    LuaObject.PushText(lua, control.Id);
                    return 1;
                case "visible":
                    lua.PushBoolean(control.App.Layout.IsVisible(control.Id));
                    return 1;
                case "width":
                    if (component != null)
                    {
                        lua.PushInteger(component.Width);
                        return 1;
                    }
                    break;
                case "height":
                    if (component != null)
                    {
                        lua.PushInteger(component.Height);
                        return 1;
                    }
                    break;
                case "clear":
                    if (component is UIConsole)
                    {
                        lua.PushCSharpFunction(ConsoleMethod(key));
                        return 1;
                    }
                    if (component is Surface)
                    {
                        lua.PushCSharpFunction(CanvasMethod(key));
                        return 1;
                    }
                    break;
                case "fillRect":
                case "drawText":
                    if (component is Surface)
                    {
                        lua.PushCSharpFunction(CanvasMethod(key));
                        return 1;
                    }
                    break;
                case "write":
                case "writeLine":
                case "scrollUp":
                case "scrollDown":
                case "scrollToEnd":
                case "clearSelection":
                    if (component is UIConsole)
                    {
                        lua.PushCSharpFunction(ConsoleMethod(key));
                        return 1;
                    }
                    break;
                case "focus":
                    if (component is TextBox || component is ListBox)
                    {
                        lua.PushCSharpFunction(ControlFocus);
                        return 1;
                    }
                    break;
                case "focused":
                    if (component is TextBox || component is ListBox)
                    {
                        lua.PushBoolean(ReferenceEquals(Kernel.MouseManager.FocusedComponent, component));
                        return 1;
                    }
                    break;
                case "load":
                    if (component is Picture)
                    {
                        lua.PushCSharpFunction(ImageLoad);
                        return 1;
                    }
                    break;
                case "foreground":
                    if (component is UIConsole foregroundConsole)
                    {
                        return Text(lua, ConsoleColorName(foregroundConsole.Foreground));
                    }
                    break;
                case "input":
                    if (component is UIConsole inputConsole)
                    {
                        return Text(lua, inputConsole.Input);
                    }
                    break;
                case "inputHidden":
                    if (component is UIConsole hiddenConsole)
                    {
                        lua.PushBoolean(hiddenConsole.InputHidden);
                        return 1;
                    }
                    break;
                case "selection":
                    if (component is UIConsole selectionConsole)
                    {
                        return Text(lua, selectionConsole.SelectedText);
                    }
                    break;
                case "clickX":
                    if (component is Surface clickXSurface)
                    {
                        lua.PushInteger(clickXSurface.ClickX);
                        return 1;
                    }
                    break;
                case "clickY":
                    if (component is Surface clickYSurface)
                    {
                        lua.PushInteger(clickYSurface.ClickY);
                        return 1;
                    }
                    break;
                case "mouseX":
                    if (component is Surface mouseXSurface)
                    {
                        lua.PushInteger(mouseXSurface.MouseX);
                        return 1;
                    }
                    break;
                case "mouseY":
                    if (component is Surface mouseYSurface)
                    {
                        lua.PushInteger(mouseYSurface.MouseY);
                        return 1;
                    }
                    break;
                case "pressed":
                    if (component is Surface pressedSurface)
                    {
                        lua.PushBoolean(pressedSurface.Pressed);
                        return 1;
                    }
                    break;
                case "cursor":
                    if (component is Surface cursorSurface)
                    {
                        return Text(lua, CursorName(cursorSurface.Cursor));
                    }
                    break;
                case "text":
                    if (component is Label label)
                    {
                        return Text(lua, label.Text);
                    }
                    if (component is Button button)
                    {
                        return Text(lua, button.Text);
                    }
                    if (component is TextBox textBox)
                    {
                        return Text(lua, textBox.Text);
                    }
                    if (component is Checkbox checkbox)
                    {
                        return Text(lua, checkbox.Text);
                    }
                    break;
                case "color":
                    Color color;
                    if (TryGetColor(component, out color))
                    {
                        LuaObject.PushText(lua, ColorText(color));
                        return 1;
                    }
                    break;
                case "checked":
                    if (component is Checkbox checkedBox)
                    {
                        lua.PushBoolean(checkedBox.Checked);
                        return 1;
                    }
                    break;
                case "value":
                    if (component is Slider slider)
                    {
                        lua.PushInteger(slider.Value);
                        return 1;
                    }
                    break;
                case "items":
                    List<string> items = ItemsOf(component);
                    if (items != null)
                    {
                        lua.CreateTable(items.Count, 0);

                        for (int i = 0; i < items.Count; i++)
                        {
                            LuaObject.PushText(lua, items[i] ?? "");
                            lua.RawSetI(-2, i + 1);
                        }

                        return 1;
                    }
                    break;
                case "selectedIndex":
                    if (component is DropDown dropDown)
                    {
                        lua.PushInteger(dropDown.SelectedIndex);
                        return 1;
                    }
                    if (component is ListBox listBox)
                    {
                        lua.PushInteger(listBox.SelectedIndex);
                        return 1;
                    }
                    break;
                case "selectedItem":
                    if (component is DropDown itemDropDown)
                    {
                        return Text(lua, itemDropDown.SelectedItem);
                    }
                    if (component is ListBox itemListBox)
                    {
                        return Text(lua, itemListBox.SelectedItem);
                    }
                    break;
                case "top":
                    if (component is ListBox topListBox)
                    {
                        lua.PushInteger(topListBox.TopIndex);
                        return 1;
                    }
                    break;
                case "title":
                    if (component is Dialog titleDialog)
                    {
                        return Text(lua, titleDialog.Title);
                    }
                    break;
                case "message":
                    if (component is Dialog dialog)
                    {
                        return Text(lua, dialog.Message);
                    }
                    break;
            }

            return LuaObject.Error(lua, "'" + control.Id + "' has no property '" + key + "'");
        }

        private static int ControlNewIndex(ILuaState lua)
        {
            Control control = (Control)lua.L_CheckUData(1, ControlType);
            string key = LuaObject.CheckText(lua, 2);
            Component component = control.Component;
            bool set = false;

            switch (key)
            {
                case "visible":
                    // Through the layout, which places the elements again: a hidden one takes no space.
                    control.App.Layout.SetVisible(control.Id, lua.ToBoolean(3));
                    return 0;
                case "text":
                    string text = LuaObject.CheckText(lua, 3);
                    if (component is Label label)
                    {
                        label.Text = text;
                        set = true;
                    }
                    else if (component is Button button)
                    {
                        button.Text = text;
                        set = true;
                    }
                    else if (component is TextBox textBox)
                    {
                        textBox.Text = text;
                        set = true;
                    }
                    else if (component is Checkbox checkbox)
                    {
                        checkbox.Text = text;
                        set = true;
                    }
                    break;
                case "color":
                    Color color;
                    string value = LuaObject.CheckText(lua, 3);
                    if (!LayoutLoader.TryParseColor(value, out color))
                    {
                        return LuaObject.Error(lua, "a color is #RRGGBB, #AARRGGBB or a color name, not '" + value + "'");
                    }
                    set = TrySetColor(component, color);
                    break;
                case "checked":
                    if (component is Checkbox checkedBox)
                    {
                        checkedBox.Checked = lua.ToBoolean(3);
                        set = true;
                    }
                    break;
                case "value":
                    if (component is Slider slider)
                    {
                        slider.Value = CheckByte(lua, 3);
                        set = true;
                    }
                    break;
                case "items":
                    if (component is DropDown || component is ListBox)
                    {
                        lua.L_CheckType(3, LuaType.LUA_TTABLE);

                        DropDown itemsDropDown = component as DropDown;
                        ListBox itemsListBox = component as ListBox;

                        // All read first: a wrong item raises an error before the control changes.
                        int count = lua.RawLen(3);
                        List<string> texts = new List<string>(count);
                        List<Bitmap> icons = new List<Bitmap>(count);

                        for (int i = 1; i <= count; i++)
                        {
                            lua.RawGetI(3, i);

                            string iconName;
                            texts.Add(ItemText(lua, out iconName));
                            lua.Pop(1);

                            Bitmap icon = null;

                            if (iconName != null)
                            {
                                if (itemsDropDown != null)
                                {
                                    return LuaObject.Error(lua, "a DropDown's items have no icon");
                                }

                                if (!Kernel.ResourceManager.TryGetIcon(iconName, out icon))
                                {
                                    return LuaObject.Error(lua, "no icon is named '" + iconName + "'");
                                }
                            }

                            icons.Add(icon);
                        }

                        if (itemsDropDown != null)
                        {
                            itemsDropDown.ClearItems();

                            for (int i = 0; i < texts.Count; i++)
                            {
                                itemsDropDown.AddItem(texts[i]);
                            }
                        }
                        else
                        {
                            itemsListBox.ClearItems();

                            for (int i = 0; i < texts.Count; i++)
                            {
                                itemsListBox.AddItem(texts[i], icons[i]);
                            }
                        }

                        set = true;
                    }
                    break;
                case "cursor":
                    if (component is Surface cursorSurface)
                    {
                        string cursorName = LuaObject.CheckText(lua, 3);
                        Aura_OS.System.Input.CursorState cursor;

                        if (!TryParseCursor(cursorName, out cursor))
                        {
                            return LuaObject.Error(lua, "a cursor is \"normal\", \"resizeHorizontal\", \"resizeVertical\" or \"grab\", not '" + cursorName + "'");
                        }

                        cursorSurface.Cursor = cursor;
                        set = true;
                    }
                    break;
                case "selectedIndex":
                    if (component is DropDown dropDown)
                    {
                        dropDown.SelectedIndex = (int)lua.L_CheckInteger(3);
                        set = true;
                    }
                    else if (component is ListBox listBox)
                    {
                        listBox.SelectedIndex = (int)lua.L_CheckInteger(3);
                        set = true;
                    }
                    break;
                case "top":
                    if (component is ListBox topListBox)
                    {
                        topListBox.TopIndex = (int)lua.L_CheckInteger(3);
                        set = true;
                    }
                    break;
                case "title":
                    if (component is Dialog titleDialog)
                    {
                        titleDialog.Title = LuaObject.CheckText(lua, 3);
                        set = true;
                    }
                    break;
                case "message":
                    if (component is Dialog dialog)
                    {
                        dialog.Message = LuaObject.CheckText(lua, 3);
                        set = true;
                    }
                    break;
                case "foreground":
                    if (component is UIConsole foregroundConsole)
                    {
                        string name = LuaObject.CheckText(lua, 3);
                        ConsoleColor consoleColor;
                        if (!TryParseConsoleColor(name, out consoleColor))
                        {
                            return LuaObject.Error(lua, "not a console color: '" + name + "'");
                        }
                        foregroundConsole.Foreground = consoleColor;
                        set = true;
                    }
                    break;
                case "input":
                    if (component is UIConsole inputConsole)
                    {
                        inputConsole.Input = LuaObject.CheckText(lua, 3);
                        set = true;
                    }
                    break;
                case "inputHidden":
                    if (component is UIConsole hiddenConsole)
                    {
                        hiddenConsole.InputHidden = lua.ToBoolean(3);
                        set = true;
                    }
                    break;
                case "state":
                    if (component is Dialog stateDialog)
                    {
                        string state = LuaObject.CheckText(lua, 3);
                        if (state != "information" && state != "error")
                        {
                            return LuaObject.Error(lua, "a dialog state is 'information' or 'error', not '" + state + "'");
                        }
                        stateDialog.SetState(state == "error" ? DialogState.Error : DialogState.Information);
                        set = true;
                    }
                    break;
            }

            if (!set)
            {
                return LuaObject.Error(lua, "'" + control.Id + "' has no property '" + key + "' to set");
            }

            // A dialog keeps its own drawing: redraw it too.
            component.MarkDirty();
            control.App.MarkDirty();
            return 0;
        }

        /// <summary>
        /// The text of the list item on top of the stack, which it leaves there: a string (or any
        /// value, as tostring writes it), or a table { text = , icon = } whose icon is an icon name.
        /// </summary>
        /// <param name="iconName">The item's icon name, null for none.</param>
        private static string ItemText(ILuaState lua, out string iconName)
        {
            iconName = null;

            if (lua.Type(-1) != LuaType.LUA_TTABLE)
            {
                return ToText(lua, -1);
            }

            lua.GetField(-1, "icon");

            if (!lua.IsNoneOrNil(-1))
            {
                iconName = ToText(lua, -1);
            }

            lua.Pop(1);

            lua.GetField(-1, "text");
            string text = lua.IsNoneOrNil(-1) ? "" : ToText(lua, -1);
            lua.Pop(1);

            return text;
        }

        /// <summary>
        /// Any value as tostring writes it.
        /// </summary>
        private static string ToText(ILuaState lua, int index)
        {
            // L_ToString pushes the text too (luaL_tolstring)
            string text = LuaText.Decode(lua.L_ToString(index));
            lua.Pop(1);
            return text;
        }

        /// <summary>
        /// control:focus(): the TextBox or ListBox gets the keys, as a click on it gives them.
        /// </summary>
        private static int ControlFocus(ILuaState lua)
        {
            Control control = (Control)lua.L_CheckUData(1, ControlType);

            if (control.Component is TextBox textBox)
            {
                textBox.Focus();
            }
            else if (control.Component is ListBox listBox)
            {
                listBox.Focus();
            }
            else
            {
                return LuaObject.Error(lua, "'" + control.Id + "' cannot take the keys");
            }

            control.Component.MarkDirty();
            control.App.MarkDirty();
            return 0;
        }

        /// <summary>
        /// image:load(path): shows that BMP file in an Image control, which takes the picture's size;
        /// true, or false and why (no such file, a BMP the gen3 loader does not read).
        /// </summary>
        private static int ImageLoad(ILuaState lua)
        {
            Control control = (Control)lua.L_CheckUData(1, ControlType);
            Picture picture = control.Component as Picture;

            if (picture == null)
            {
                return LuaObject.Error(lua, "'" + control.Id + "' is not an Image");
            }

            string path = AuraPath.Resolve(LuaObject.CheckText(lua, 2));
            Bitmap bitmap = null;
            string error = null;

            try
            {
                bitmap = ImageUtils.LoadBmp(File.ReadAllBytes(path));
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (error == null)
            {
                picture.Image = bitmap;
                control.App.MarkDirty();
            }

            lua.PushBoolean(error == null);
            LuaObject.PushText(lua, error);
            return 2;
        }

        /// <summary>
        /// A Canvas control's method: canvas:clear([color]), canvas:fillRect(x, y, width, height, color),
        /// canvas:drawText(text, x, y, color). The canvas clips what falls outside it.
        /// </summary>
        private static CSharpFunctionDelegate CanvasMethod(string name)
        {
            return lua =>
            {
                Control control = (Control)lua.L_CheckUData(1, ControlType);
                Surface surface = control.Component as Surface;

                if (surface == null)
                {
                    return LuaObject.Error(lua, "'" + control.Id + "' is not a Canvas");
                }

                switch (name)
                {
                    case "clear":
                        surface.Clear(lua.IsNoneOrNil(2) ? surface.Background : CheckColor(lua, 2));
                        break;
                    case "fillRect":
                        surface.DrawFilledRectangle(CheckColor(lua, 6), CheckCoordinate(lua, 2), CheckCoordinate(lua, 3),
                            CheckCoordinate(lua, 4), CheckCoordinate(lua, 5));
                        break;
                    case "drawText":
                        surface.DrawString(LuaObject.CheckText(lua, 2), Kernel.font, CheckColor(lua, 5),
                            CheckCoordinate(lua, 3), CheckCoordinate(lua, 4));
                        break;
                }

                surface.MarkDirty();
                control.App.MarkDirty();
                return 0;
            };
        }

        /// <summary>
        /// A color argument: "#RRGGBB", "#AARRGGBB" or a color name, as in layout files.
        /// </summary>
        private static Color CheckColor(ILuaState lua, int index)
        {
            string value = LuaObject.CheckText(lua, index);
            Color color;

            if (!LayoutLoader.TryParseColor(value, out color))
            {
                LuaObject.Error(lua, "a color is #RRGGBB, #AARRGGBB or a color name, not '" + value + "'");
            }

            return color;
        }

        /// <summary>
        /// A pixel coordinate or size argument, kept far from the int limits: the canvas clips by
        /// adding them.
        /// </summary>
        private static int CheckCoordinate(ILuaState lua, int index)
        {
            return (int)Math.Clamp(lua.L_CheckInteger(index), -100000, 100000);
        }

        /// <summary>
        /// A Console control's method: console:write(text), console:writeLine([text]), console:clear(),
        /// console:scrollUp(), console:scrollDown(), console:scrollToEnd(), console:clearSelection().
        /// </summary>
        private static CSharpFunctionDelegate ConsoleMethod(string name)
        {
            return lua =>
            {
                Control control = (Control)lua.L_CheckUData(1, ControlType);
                UIConsole console = control.Component as UIConsole;

                if (console == null)
                {
                    return LuaObject.Error(lua, "'" + control.Id + "' is not a Console");
                }

                switch (name)
                {
                    case "write":
                        console.Write(LuaObject.CheckText(lua, 2));
                        break;
                    case "writeLine":
                        console.WriteLine(lua.IsNoneOrNil(2) ? "" : LuaObject.CheckText(lua, 2));
                        break;
                    case "clear":
                        console.ClearText();
                        break;
                    case "scrollUp":
                        console.ScrollUp();
                        break;
                    case "scrollDown":
                        console.ScrollDown();
                        break;
                    case "scrollToEnd":
                        console.ScrollToEnd();
                        break;
                    case "clearSelection":
                        console.ClearSelection();
                        break;
                }

                console.MarkDirty();
                control.App.MarkDirty();
                return 0;
            };
        }

        /// <summary>
        /// A console color by name: black, darkBlue, darkGreen, darkCyan, darkRed, darkMagenta,
        /// darkYellow, gray, darkGray, blue, green, cyan, red, magenta, yellow, white. A table, not
        /// Enum.Parse: NativeAOT keeps no enum names.
        /// </summary>
        private static bool TryParseConsoleColor(string name, out ConsoleColor color)
        {
            for (int i = 0; i < ConsoleColorNames.Length; i++)
            {
                if (ConsoleColorNames[i] == name)
                {
                    color = (ConsoleColor)i;
                    return true;
                }
            }

            color = ConsoleColor.White;
            return false;
        }

        private static string ConsoleColorName(ConsoleColor color)
        {
            int index = (int)color;
            return index >= 0 && index < ConsoleColorNames.Length ? ConsoleColorNames[index] : null;
        }

        // In ConsoleColor order (Black = 0 ... White = 15).
        private static readonly string[] ConsoleColorNames =
        {
            "black", "darkBlue", "darkGreen", "darkCyan", "darkRed", "darkMagenta", "darkYellow", "gray",
            "darkGray", "blue", "green", "cyan", "red", "magenta", "yellow", "white",
        };

        /// <summary>
        /// The items of a DropDown or a ListBox, null for another control.
        /// </summary>
        private static List<string> ItemsOf(Component component)
        {
            if (component is DropDown dropDown)
            {
                return dropDown.Items;
            }

            if (component is ListBox listBox)
            {
                return listBox.Items;
            }

            return null;
        }

        private static bool TryGetColor(Component component, out Color color)
        {
            if (component is Label label)
            {
                color = label.TextColor;
                return true;
            }

            if (component is Button button)
            {
                color = button.TextColor;
                return true;
            }

            if (component is Checkbox checkbox)
            {
                color = checkbox.TextColor;
                return true;
            }

            color = Color.Black;
            return false;
        }

        private static bool TrySetColor(Component component, Color color)
        {
            if (component is Label label)
            {
                label.TextColor = color;
                return true;
            }

            if (component is Button button)
            {
                button.TextColor = color;
                return true;
            }

            if (component is Checkbox checkbox)
            {
                checkbox.TextColor = color;
                return true;
            }

            return false;
        }

        /// <summary>
        /// "#RRGGBB", or "#AARRGGBB" when not opaque: what color accepts.
        /// </summary>
        private static string ColorText(Color color)
        {
            if (color.A == 255)
            {
                return "#" + (color.ToArgb() & 0xFFFFFF).ToString("X6");
            }

            return "#" + unchecked((uint)color.ToArgb()).ToString("X8");
        }

        #endregion

        #region Values

        private static int Push(ILuaState lua, string value)
        {
            LuaObject.PushText(lua, value);
            return 1;
        }

        private static int Push(ILuaState lua, bool value)
        {
            lua.PushBoolean(value);
            return 1;
        }

        private static int Push(ILuaState lua, long value)
        {
            lua.PushInteger(value);
            return 1;
        }

        /// <summary>
        /// A string, nil for null.
        /// </summary>
        private static int Text(ILuaState lua, string value)
        {
            LuaObject.PushText(lua, value);
            return 1;
        }

        /// <summary>
        /// Sets a field of the table on top of the stack to an integer.
        /// </summary>
        private static void SetInteger(ILuaState lua, string key, long value)
        {
            lua.PushInteger(value);
            lua.SetField(-2, key);
        }

        /// <summary>
        /// Sets a field of the table on top of the stack to a string, "" for null.
        /// </summary>
        private static void SetText(ILuaState lua, string key, string value)
        {
            LuaObject.PushText(lua, value ?? "");
            lua.SetField(-2, key);
        }

        /// <summary>
        /// Runs a kernel action: true, or false and the message of what it threw. Read the arguments
        /// first: their Lua errors must not be caught here.
        /// </summary>
        private static int Try(ILuaState lua, Action action)
        {
            string error = null;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            lua.PushBoolean(error == null);
            LuaObject.PushText(lua, error);
            return 2;
        }

        /// <summary>
        /// An integer argument from 0 to 255 (alphas, slider values).
        /// </summary>
        private static byte CheckByte(ILuaState lua, int index)
        {
            long value = lua.L_CheckInteger(index);
            lua.L_ArgCheck(value >= 0 && value <= 255, index, "0 to 255 expected");
            return (byte)value;
        }

        /// <summary>
        /// A settings.ini value: a boolean as true/false, a whole number without decimals.
        /// </summary>
        private static string SettingText(ILuaState lua, int index)
        {
            switch (lua.Type(index))
            {
                case LuaType.LUA_TBOOLEAN:
                    return lua.ToBoolean(index) ? "true" : "false";
                case LuaType.LUA_TNUMBER:
                    double number = lua.ToNumber(index);
                    return number == Math.Floor(number) && Math.Abs(number) < 1e15
                        ? ((long)number).ToString(CultureInfo.InvariantCulture)
                        : number.ToString(CultureInfo.InvariantCulture);
                case LuaType.LUA_TSTRING:
                    return LuaText.Decode(lua.ToString(index));
            }

            LuaObject.Error(lua, "a setting is a string, a number or a boolean");
            return null;
        }

        #endregion
    }
}
