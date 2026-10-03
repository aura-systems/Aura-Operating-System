/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lua aura library: what a package's code reaches of the kernel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Cosmos.Executable.Lua;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Graphics.UI.GUI.Layout;
using Aura_OS.System.Network;
using Aura_OS.System.Processing.Applications;
using AuraVersion = Aura_OS.System.Network.Version;

namespace Aura_OS.System.Processing.Lua
{
    /// <summary>
    /// The aura library (global, and require "aura"): the app's window and controls, the system and
    /// the network. SRC/Packages/README.md is its reference; keep both in step.
    /// </summary>
    internal static class LuaAuraLib
    {
        public const string LIB_NAME = "aura";

        private const string AppType = "aura.app";
        private const string ControlType = "aura.control";

        /// <summary>
        /// A control found by id: what a control userdata holds.
        /// </summary>
        private sealed class Control
        {
            public readonly PackageApp App;
            public readonly string Id;
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

            lua.L_NewLib(new NameFuncPair[]
            {
                new NameFuncPair("latestRelease", SystemLatestRelease),
                new NameFuncPair("compareVersions", SystemCompareVersions),
                new NameFuncPair("compareRevisions", SystemCompareRevisions),
            });
            lua.PushString(Kernel.Version ?? "");
            lua.SetField(-2, "version");
            lua.PushString(Kernel.Revision ?? "");
            lua.SetField(-2, "revision");
            lua.SetField(-2, "system");

            lua.L_NewLib(new NameFuncPair[]
            {
                new NameFuncPair("isConfigured", NetworkIsConfigured),
            });
            lua.SetField(-2, "network");

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

        private static void NewMetaTable(ILuaState lua, string name, CSharpFunctionDelegate index, CSharpFunctionDelegate newIndex)
        {
            lua.L_NewMetaTable(name);
            lua.PushCSharpFunction(index);
            lua.SetField(-2, "__index");
            lua.PushCSharpFunction(newIndex);
            lua.SetField(-2, "__newindex");
            lua.Pop(1);
        }

        #region aura

        /// <summary>
        /// aura.log(text): a line in the OS log, after the package's name.
        /// </summary>
        private static int Log(ILuaState lua, Package package)
        {
            Logs.DoOSLog("[" + package.Name + "] " + lua.L_CheckString(1));
            return 0;
        }

        #endregion

        #region aura.system

        /// <summary>
        /// aura.system.latestRelease(): version, revision and url of the last release (os.json).
        /// Raises an error when it cannot be downloaded: call it with pcall.
        /// </summary>
        private static int SystemLatestRelease(ILuaState lua)
        {
            (string version, string revision, string url) = AuraVersion.GetLastVersionInfo();

            PushText(lua, version);
            PushText(lua, revision);
            PushText(lua, url);
            return 3;
        }

        /// <summary>
        /// aura.system.compareVersions(a, b): -1, 0 or 1 ("0.8.0" against "0.10.0").
        /// </summary>
        private static int SystemCompareVersions(ILuaState lua)
        {
            lua.PushInteger(Math.Sign(AuraVersion.CompareVersions(lua.L_CheckString(1), lua.L_CheckString(2))));
            return 1;
        }

        /// <summary>
        /// aura.system.compareRevisions(a, b): -1, 0 or 1; 0 when either is not a number.
        /// </summary>
        private static int SystemCompareRevisions(ILuaState lua)
        {
            lua.PushInteger(Math.Sign(AuraVersion.CompareRevisions(lua.L_CheckString(1), lua.L_CheckString(2))));
            return 1;
        }

        #endregion

        #region aura.network

        /// <summary>
        /// aura.network.isConfigured(): true when an adapter has an address.
        /// </summary>
        private static int NetworkIsConfigured(ILuaState lua)
        {
            lua.PushBoolean(NetworkHelper.IsConfigured);
            return 1;
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
                    PushText(lua, app.Window.Name);
                    return 1;
            }

            return Error(lua, "the app has no field '" + key + "'");
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

            return Error(lua, "the app has no field '" + key + "' to set");
        }

        /// <summary>
        /// app:find(id): the control with that id in the layout file.
        /// </summary>
        private static int AppFind(ILuaState lua)
        {
            PackageApp app = (PackageApp)lua.L_CheckUData(1, AppType);
            string id = lua.L_CheckString(2);

            // Throws (a Lua error) for an id the file does not have.
            Component component = app.Layout.Find<Component>(id);

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
                    lua.PushBoolean(component.Visible);
                    return 1;
                case "text":
                    if (component is Label label)
                    {
                        PushText(lua, label.Text);
                        return 1;
                    }
                    if (component is Button button)
                    {
                        PushText(lua, button.Text);
                        return 1;
                    }
                    if (component is TextBox textBox)
                    {
                        PushText(lua, textBox.Text);
                        return 1;
                    }
                    if (component is Checkbox checkbox)
                    {
                        PushText(lua, checkbox.Text);
                        return 1;
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
                        PushText(lua, itemDropDown.SelectedItem);
                        return 1;
                    }
                    break;
                case "title":
                    if (component is Dialog titleDialog)
                    {
                        PushText(lua, titleDialog.Title);
                        return 1;
                    }
                    break;
                case "message":
                    if (component is Dialog dialog)
                    {
                        PushText(lua, dialog.Message);
                        return 1;
                    }
                    break;
            }

            return Error(lua, "'" + control.Id + "' has no property '" + key + "'");
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
                    component.Visible = lua.ToBoolean(3);
                    set = true;
                    break;
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
                        return Error(lua, "a color is #RRGGBB, #AARRGGBB or a color name, not '" + value + "'");
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
                        slider.Value = (int)lua.L_CheckInteger(3);
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
                            return Error(lua, "a dialog state is 'information' or 'error', not '" + state + "'");
                        }
                        stateDialog.SetState(state == "error" ? DialogState.Error : DialogState.Information);
                        set = true;
                    }
                    break;
            }

            if (!set)
            {
                return Error(lua, "'" + control.Id + "' has no property '" + key + "' to set");
            }

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

        /// <summary>
        /// A string, nil for null.
        /// </summary>
        private static void PushText(ILuaState lua, string text)
        {
            if (text == null)
            {
                lua.PushNil();
            }
            else
            {
                lua.PushString(text);
            }
        }

        /// <summary>
        /// Raises a Lua error with the caller's position, as luaL_error does (without its format).
        /// </summary>
        private static int Error(ILuaState lua, string message)
        {
            lua.L_Where(1);
            lua.PushString(message);
            lua.Concat(2);
            return lua.Error();
        }
    }
}
