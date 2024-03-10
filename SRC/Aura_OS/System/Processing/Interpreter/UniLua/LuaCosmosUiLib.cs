using System;
using Aura_OS;
using Aura_OS.System.Graphics.UI;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Applications;
using UniLua;

namespace UniLua
{
    internal class LuaCosmosUiLib
    {
        public const string LIB_NAME = "cosmos.ui";

        public static int OpenLib(ILuaState lua)
        {
            NameFuncPair[] define = new NameFuncPair[]
            {
                new NameFuncPair("createWindow", LUA_createWindow),
                new NameFuncPair("createButton", LUA_createButton),

            };

            lua.L_NewLib(define);
            return 1;
        }

        private static int LUA_createWindow(ILuaState lua)
        {
            string title = lua.ToString(1);
            int width = lua.ToInteger(2);
            int height = lua.ToInteger(3);

            LuaApp app = new LuaApp(title, width, height, lua.GetPid());
            Kernel.ApplicationManager.StartApplication(app);

            lua.PushInteger((int)app.ID);

            return 1;
        }

        private static int LUA_createButton(ILuaState lua)
        {
            int pid = lua.ToInteger(1);
            string text = lua.ToString(2);
            ILuaState click = lua.ToThread(3);
            int x = lua.ToInteger(4);
            int y = lua.ToInteger(5);

            LuaApp app = Kernel.ApplicationManager.GetApplicationByPid((uint)pid) as LuaApp;
            Button button = new Button(text, x, y + app.Window.TopBar.Height + 3, text.Length * Kernel.font.Width + 6, Kernel.font.Height + 6);
            button.Click = new Action(() =>
            {
                if (click != null)
                {
                    click.Resume(lua, 0);
                }
            });

            app.AddChild(button);
            app.MarkDirty();

            return 1;
        }

    }
}
