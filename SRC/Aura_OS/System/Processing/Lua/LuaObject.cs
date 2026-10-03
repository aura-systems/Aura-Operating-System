/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lua object whose fields are C# getters, setters and functions
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using Cosmos.Executable.Lua;

namespace Aura_OS.System.Processing.Lua
{
    /// <summary>
    /// A Lua value (a userdata) whose fields are C# getters, setters and functions: the aura modules
    /// (aura.system, aura.display...) are made of these. Reading or setting a field it does not have,
    /// or setting a read-only one, is an error, so a typo does not go unnoticed.
    /// </summary>
    internal sealed class LuaObject
    {
        private const string TypeName = "aura.object";

        /// <summary>
        /// Its name in error messages ("aura.display").
        /// </summary>
        public readonly string Name;

        // Push the field's value, return 1.
        private readonly Dictionary<string, CSharpFunctionDelegate> _getters = new Dictionary<string, CSharpFunctionDelegate>();

        // Read the new value at index 3, return 0.
        private readonly Dictionary<string, CSharpFunctionDelegate> _setters = new Dictionary<string, CSharpFunctionDelegate>();

        public LuaObject(string name)
        {
            Name = name;
        }

        /// <summary>
        /// A function field, called with a dot: aura.display.modes().
        /// </summary>
        public LuaObject Function(string name, CSharpFunctionDelegate function)
        {
            _getters[name] = lua =>
            {
                lua.PushCSharpFunction(function);
                return 1;
            };

            return this;
        }

        /// <summary>
        /// A field: get pushes its value and returns 1; set, null for a read-only field, reads the new
        /// value at index 3 and returns 0.
        /// </summary>
        public LuaObject Property(string name, CSharpFunctionDelegate get, CSharpFunctionDelegate set = null)
        {
            _getters[name] = get;

            if (set != null)
            {
                _setters[name] = set;
            }

            return this;
        }

        /// <summary>
        /// Pushes the object.
        /// </summary>
        public void Push(ILuaState lua)
        {
            lua.NewUserData(this);

            // Created once per interpreter, shared by every object.
            if (lua.L_NewMetaTable(TypeName))
            {
                lua.PushCSharpFunction(Index);
                lua.SetField(-2, "__index");
                lua.PushCSharpFunction(NewIndex);
                lua.SetField(-2, "__newindex");
            }

            lua.SetMetaTable(-2);
        }

        private static int Index(ILuaState lua)
        {
            LuaObject self = (LuaObject)lua.L_CheckUData(1, TypeName);
            string key = CheckText(lua, 2);
            CSharpFunctionDelegate get;

            if (self._getters.TryGetValue(key, out get))
            {
                return get(lua);
            }

            return Error(lua, self.Name + " has no field '" + key + "'");
        }

        private static int NewIndex(ILuaState lua)
        {
            LuaObject self = (LuaObject)lua.L_CheckUData(1, TypeName);
            string key = CheckText(lua, 2);
            CSharpFunctionDelegate set;

            if (self._setters.TryGetValue(key, out set))
            {
                return set(lua);
            }

            if (self._getters.ContainsKey(key))
            {
                return Error(lua, self.Name + "." + key + " is read-only");
            }

            return Error(lua, self.Name + " has no field '" + key + "'");
        }

        /// <summary>
        /// Text as a Lua string (its UTF-8 bytes, LuaText.Encode), nil for null.
        /// </summary>
        public static void PushText(ILuaState lua, string text)
        {
            if (text == null)
            {
                lua.PushNil();
            }
            else
            {
                lua.PushString(LuaText.Encode(text));
            }
        }

        /// <summary>
        /// A string argument as text (LuaText.Decode): on ILuaState a Lua string is its bytes.
        /// </summary>
        public static string CheckText(ILuaState lua, int index)
        {
            return LuaText.Decode(lua.L_CheckString(index));
        }

        /// <summary>
        /// Raises a Lua error with the caller's position, as luaL_error does (without its format).
        /// </summary>
        public static int Error(ILuaState lua, string message)
        {
            lua.L_Where(1);
            lua.PushString(LuaText.Encode(message));
            lua.Concat(2);
            return lua.Error();
        }
    }
}
