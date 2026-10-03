/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lua aura library: what a package's code reaches of the kernel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using Cosmos.Executable.Lua;
using Cosmos.Kernel.System.Graphics;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Graphics.UI.GUI.Layout;
using Aura_OS.System.Network;
using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Utils;
using AuraVersion = Aura_OS.System.Network.Version;

namespace Aura_OS.System.Processing.Lua
{
    /// <summary>
    /// The aura library (global, and require "aura"): the app's window and controls, and the system's
    /// settings. SRC/Packages/README.md is its reference; keep both in step.
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
            SetObject(lua, "display", DisplayObject());
            SetObject(lua, "desktop", DesktopObject());
            SetObject(lua, "theme", ThemeObject());
            SetObject(lua, "settings", SettingsObject());
            SetObject(lua, "fs", FsObject());

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
            Logs.DoOSLog("[" + package.Name + "] " + lua.L_CheckString(1));
            return 0;
        }

        #region Modules

        private static LuaObject SystemObject()
        {
            return new LuaObject("aura.system")
                .Property("version", lua => Push(lua, Kernel.Version ?? ""))
                .Property("revision", lua => Push(lua, Kernel.Revision ?? ""))
                .Property("installed", lua => Push(lua, Kernel.Installed))
                .Property("computerName", lua => Push(lua, Kernel.ComputerName ?? ""), lua =>
                {
                    Kernel.ComputerName = lua.L_CheckString(3);

                    if (!string.IsNullOrEmpty(Kernel.ComputerName))
                    {
                        Cosmos.Kernel.System.Network.Config.DnsConfig.HostName = Kernel.ComputerName;
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
                .Function("compareVersions", lua => Push(lua, Math.Sign(AuraVersion.CompareVersions(lua.L_CheckString(1), lua.L_CheckString(2)))))
                .Function("compareRevisions", lua => Push(lua, Math.Sign(AuraVersion.CompareRevisions(lua.L_CheckString(1), lua.L_CheckString(2)))));
        }

        private static LuaObject UserObject()
        {
            return new LuaObject("aura.user")
                .Property("name", lua => Push(lua, Kernel.userLogged ?? ""), lua =>
                {
                    Kernel.userLogged = lua.L_CheckString(3);
                    return 0;
                });
        }

        private static LuaObject NetworkObject()
        {
            return new LuaObject("aura.network")
                .Function("isConfigured", lua => Push(lua, NetworkHelper.IsConfigured));
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
                    Explorer.Desktop.SetWallpaper(lua.L_CheckString(1));
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
                });
        }

        /// <summary>
        /// The theme files, loaded at boot: a change shows after a reboot.
        /// </summary>
        private static LuaObject ThemeObject()
        {
            return new LuaObject("aura.theme")
                .Property("bmpPath", lua => Text(lua, Kernel.ThemeManager.BmpPath), lua =>
                {
                    Kernel.ThemeManager.BmpPath = lua.L_CheckString(3);
                    return 0;
                })
                .Property("xmlPath", lua => Text(lua, Kernel.ThemeManager.XmlPath), lua =>
                {
                    Kernel.ThemeManager.XmlPath = lua.L_CheckString(3);
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
                    string key = lua.L_CheckString(1);
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

                        config.EditValue(lua.ToString(-2), SettingText(lua, -1));
                        lua.Pop(1);
                    }

                    config.Push();
                    return 0;
                });
        }

        private static LuaObject FsObject()
        {
            return new LuaObject("aura.fs")
                // resolve(path): absolute, from the current directory; gen2 paths (0:\Users) are converted.
                .Function("resolve", lua => Push(lua, AuraPath.Resolve(lua.L_CheckString(1))))
                .Function("fileExists", lua => Push(lua, File.Exists(lua.L_CheckString(1))));
        }

        #endregion

        #region aura.app

        private static int AppIndex(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            string key = lua.L_CheckString(2);

            switch (key)
            {
                case "find":
                    lua.PushCSharpFunction(AppFind);
                    return 1;
                case "on":
                    lua.PushCSharpFunction(AppOn);
                    return 1;
                case "title":
                    return Text(lua, app.Window.Name);
            }

            return LuaObject.Error(lua, "the app has no field '" + key + "'");
        }

        private static int AppNewIndex(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            string key = lua.L_CheckString(2);

            if (key == "title")
            {
                app.SetTitle(lua.L_CheckString(3));
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
            string id = lua.L_CheckString(2);

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
            string name = lua.L_CheckString(2);
            lua.L_CheckType(3, LuaType.LUA_TFUNCTION);

            lua.PushValue(3);
            app.SetHandler(name, lua.L_Ref(LuaDef.LUA_REGISTRYINDEX));
            return 0;
        }

        #endregion

        #region Controls

        private static int ControlIndex(ILuaState lua)
        {
            Control control = (Control)lua.L_CheckUData(1, ControlType);
            string key = lua.L_CheckString(2);
            Component component = control.Component;

            switch (key)
            {
                case "id":
                    lua.PushString(control.Id);
                    return 1;
                case "visible":
                    lua.PushBoolean(control.App.Layout.IsVisible(control.Id));
                    return 1;
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
                        lua.PushString(ColorText(color));
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
                    if (component is DropDown itemsDropDown)
                    {
                        lua.CreateTable(itemsDropDown.Items.Count, 0);

                        for (int i = 0; i < itemsDropDown.Items.Count; i++)
                        {
                            lua.PushString(itemsDropDown.Items[i] ?? "");
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
                    break;
                case "selectedItem":
                    if (component is DropDown itemDropDown)
                    {
                        return Text(lua, itemDropDown.SelectedItem);
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
            string key = lua.L_CheckString(2);
            Component component = control.Component;
            bool set = false;

            switch (key)
            {
                case "visible":
                    // Through the layout, which places the elements again: a hidden one takes no space.
                    control.App.Layout.SetVisible(control.Id, lua.ToBoolean(3));
                    return 0;
                case "text":
                    string text = lua.L_CheckString(3);
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
                    string value = lua.L_CheckString(3);
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
                    if (component is DropDown itemsDropDown)
                    {
                        lua.L_CheckType(3, LuaType.LUA_TTABLE);
                        itemsDropDown.ClearItems();

                        int count = lua.RawLen(3);

                        for (int i = 1; i <= count; i++)
                        {
                            // L_ToString pushes the text too (luaL_tolstring)
                            lua.RawGetI(3, i);
                            itemsDropDown.AddItem(lua.L_ToString(-1));
                            lua.Pop(2);
                        }

                        set = true;
                    }
                    break;
                case "selectedIndex":
                    if (component is DropDown dropDown)
                    {
                        dropDown.SelectedIndex = (int)lua.L_CheckInteger(3);
                        set = true;
                    }
                    break;
                case "title":
                    if (component is Dialog titleDialog)
                    {
                        titleDialog.Title = lua.L_CheckString(3);
                        set = true;
                    }
                    break;
                case "message":
                    if (component is Dialog dialog)
                    {
                        dialog.Message = lua.L_CheckString(3);
                        set = true;
                    }
                    break;
                case "state":
                    if (component is Dialog stateDialog)
                    {
                        string state = lua.L_CheckString(3);
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
            lua.PushString(value);
            return 1;
        }

        private static int Push(ILuaState lua, bool value)
        {
            lua.PushBoolean(value);
            return 1;
        }

        private static int Push(ILuaState lua, int value)
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
                    return lua.ToString(index);
            }

            LuaObject.Error(lua, "a setting is a string, a number or a boolean");
            return null;
        }

        #endregion
    }
}
