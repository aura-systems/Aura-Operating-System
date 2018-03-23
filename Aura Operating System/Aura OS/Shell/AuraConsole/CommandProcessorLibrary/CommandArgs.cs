using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework
{
    public class CommandArgs
    {
        private static List<string> args = new List<string>();

        public CommandArgs()
        {
            new List<string>(0);
            if (CommandUtils.DebugMode)
                Console.WriteLine("Set CommandArgs array length to 0.");
        }

        public CommandArgs(string[] argsx)
        {
            args = new List<string>(argsx.Length);
            if (argsx.Length > 0)
            {
                foreach (string s in argsx)
                {
                    args.Add(s);
                }
            }
            if (CommandUtils.DebugMode)
                Console.WriteLine($"There are \"{Count()}\" args in the argument array!");
        }

        #region Normal
        /// <summary>
        /// Checks if the argument array is empty.
        /// </summary>
        /// <returns>Whether the argument array is empty.</returns>
        public bool IsEmpty()
        {
            if (Count() > 0 || Count() != 0) return false;
            return true;
        }

        /// <summary>
        /// The total number of arguments in the array.
        /// </summary>
        /// <returns>The total number of arguments in the array.</returns>
        public int Count()
        {
            return args.Count;
        }

        /// <summary>
        /// Gets an argument from the position in the argument array.
        /// </summary>
        /// <param name="pos">The location within the argument array.</param>
        /// <returns>The value from that position in the argument array.</returns>
        public string GetArgAtPosition(int pos)
        {
            if (!(IsEmpty()))
            {
                var x = args.ToArray();
                return x[pos];
            }
            return "";
        }
        #endregion

        #region Contains
        /// <summary>
        /// Checks if the argument array has the following argument anywhere in the array.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <returns>Whether the argument was found in the array.</returns>
        public bool ContainsArg(string arg)
        {
            if (!(IsEmpty()))
            {
                if (args.Contains(arg)) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the argument array has the following argument at the specified position within the array.
        /// </summary>
        /// <param name="pos">The position in the array where you think the argument can be found.</param>
        /// <param name="arg">The argument to check at the position.</param>
        /// <returns>Whether the argument was found in the array at the specified position.</returns>
        public bool ContainsArg(int pos, string arg)
        {
            if (!(IsEmpty()))
            {
                var arr = args.ToArray();
                if (arr[pos].Contains(arg)) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the argument array has the following argument anywhere in the array.
        /// </summary>
        /// <param name="value">The switch to look for in the array.</param>
        /// <returns>Whether a switch was found in the array.</returns>
        public bool ContainsSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                if (ContainsArg($"-{value}") || ContainsArg($"/{value}")) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the argument array has the following argument at the specified position within the array.
        /// </summary>
        /// <param name="pos">The position within the array.</param>
        /// <param name="value">The switch to look for in the array.</param>
        /// <returns>Whether the value was found in the array at the specified position.</returns>
        public bool ContainsSwitch(int pos, string value)
        {
            if (!(IsEmpty()))
            {
                if (ContainsArg(pos, $"-{value}") || ContainsArg(pos, $"/{value}")) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the argument array has the following argument anywhere in the array.
        /// </summary>
        /// <param name="segment">The segmented switch to look for in the array.</param>
        /// <returns>Whether the array contains the segmented switch.</returns>
        public bool ContainsSegmentedSwitch(string segment)
        {
            if (!(IsEmpty()))
            {
                if (ContainsSwitch($"{segment}:")) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the argument array has the following argument at the specified location within the array.
        /// </summary>
        /// <param name="pos">The position within the array.</param>
        /// <param name="segment">The segmented switch to look for in the array.</param>
        /// <returns>Whether the array contains the segmented switch.</returns>
        public bool ContainsSegmentedSwitch(int pos, string segment)
        {
            if (!(IsEmpty()))
            {
                if (ContainsSwitch(pos, $"{segment}:")) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the argument array has the following argument anywhere within the argument array.
        /// </summary>
        /// <param name="valued">The valued switch to look for in the array.</param>
        /// <returns>Whether the array contains the valued switch.</returns>
        public bool ContainsValuedSwitch(string valued)
        {
            if (!(IsEmpty()))
            {
                if (ContainsSwitch($"{valued}=")) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the argument array has the following argument at the specified location within the array.
        /// </summary>
        /// <param name="pos">The position within the array.</param>
        /// <param name="valued">The valued swith to look for in the array.</param>
        /// <returns>Whether the array contains the valued switch.</returns>
        public bool ContainsValuedSwitch(int pos, string valued)
        {
            if (!(IsEmpty()))
            {
                if (ContainsSwitch(pos, $"{valued}=")) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks the argument array has the following argument anywhere within the array.
        /// </summary>
        /// <param name="variable">The variable to look for in the array.</param>
        /// <returns>Whether the array contains the variable.</returns>
        public bool ContainsVariable(CommandArgVariable variable)
        {
            if (!(IsEmpty()))
            {
                if (ContainsArg($"{variable.GetHeader()}{variable.GetValue()}")) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks the argument array has the following argument at the specified location within the array.
        /// </summary>
        /// <param name="pos">The position within the array.</param>
        /// <param name="variable">The variable to look for in the array.</param>
        /// <returns>Whether the array contains the variable.</returns>
        public bool ContainsVariable(int pos, CommandArgVariable variable)
        {
            if (!(IsEmpty()))
            {
                if (ContainsArg(pos, $"{variable.GetHeader()}{variable.GetValue()}")) return true;
            }
            return false;
        }
        #endregion

        #region IndexOf
        /// <summary>
        /// Gets the index from the argument index based on the value.
        /// </summary>
        /// <param name="arg">The argument in the array.</param>
        /// <returns>The index of the argument.</returns>
        public int IndexOfArg(string arg)
        {
            if (!(IsEmpty()))
            {
                int value = 0;
                for (int i = 0; i == args.Count; i++)
                {
                    var x = args.ToArray();
                    if (x[i] == arg || x[i].Contains(arg))
                    {
                        value = i;
                        break;
                    }
                }
                return value;
            }
            return 0;
        }

        /// <summary>
        /// Gets the index from the argument index based on the value.
        /// </summary>
        /// <param name="value">The value in the array.</param>
        /// <returns>The index of the value.</returns>
        public int IndexOfSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                if (ContainsArg($"-{value}"))
                {
                    return args.IndexOf($"-{value}");
                }
                else if (ContainsArg($"/{value}"))
                {
                    return args.IndexOf($"/{value}");
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the index from the argument index based on the value.
        /// </summary>
        /// <param name="value">The value in the array.</param>
        /// <returns>The index of the value.</returns>
        public int IndexOfSegmentedSwitch(string value)
        {
            if (!(IsEmpty()))
                return IndexOfSwitch($"{value}:");
            return 0;
        }

        /// <summary>
        /// Gets the index from the argument index based on the value.
        /// </summary>
        /// <param name="value">The value in the array.</param>
        /// <returns>The index of the value.</returns>
        public int IndexOfValuedSwitch(string value)
        {
            if (!(IsEmpty()))
                return IndexOfSwitch($"{value}=");
            return 0;
        }

        /// <summary>
        /// Gets the index from the argument index based on the value.
        /// </summary>
        /// <param name="variable">The value in the array.</param>
        /// <returns>The index of the value.</returns>
        public int IndexOfVariable(CommandArgVariable variable)
        {
            if (!(IsEmpty()))
                return IndexOfArg($"{variable.GetHeader()}{variable.GetValue()}");
            return 0;
        }
        #endregion

        #region ValueOf
        /// <summary>
        /// Gets the value of the segmented switch.
        /// </summary>
        /// <param name="value">the key of the segment.</param>
        /// <returns>The value the was returned by the key.</returns>
        public string ValueOfSegmentedSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                if (ContainsSwitch(value))
                {
                    var arr = args.ToArray();
                    var dat = arr[IndexOfSegmentedSwitch(value)];
                    var spl = dat.Split(':');
                    var fnl = spl[1];
                    return fnl;
                }
            }
            return "";
        }

        /// <summary>
        /// Gets the value of the segmented switch.
        /// </summary>
        /// <param name="value">the key of the segment.</param>
        /// <returns>The value the was returned by the key.</returns>
        public string ValueOfValuedSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                if (ContainsValuedSwitch(value))
                {
                    var arr = args.ToArray();
                    var dat = arr[IndexOfValuedSwitch(value)];
                    var spl = dat.Split('=');
                    var fnl = spl[1];
                    return fnl;
                }
            }
            return "";
        }
        #endregion

        #region Before
        /// <summary>
        /// Gets an argument just before the one that was provided from the array.
        /// </summary>
        /// <param name="arg">The argument after the wanted argument.</param>
        /// <returns>The argument that was before the one specified.</returns>
        public string BeforeArg(string arg)
        {
            if (!(IsEmpty()))
            {
                if (Count() > 0)
                    return GetArgAtPosition(IndexOfArg(arg) - 1);
            }
            return "";
        }

        /// <summary>
        /// Gets an argument just before the one that was provided from the array.
        /// </summary>
        /// <param name="value">The argument after the wanted argument.</param>
        /// <returns>The argument that was before the one specified.</returns>
        public string BeforeSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                if (Count() > 0)
                    return GetArgAtPosition(IndexOfSwitch(value) - 1);
            }
            return "";
        }

        /// <summary>
        /// Gets an argument just before the one that was provided from the array.
        /// </summary>
        /// <param name="value">The argument after the wanted argument.</param>
        /// <returns>The argument that was before the one specified.</returns>
        public string BeforeSegmentedSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                if (Count() > 0)
                    return GetArgAtPosition(IndexOfSegmentedSwitch(value) - 1);
            }
            return "";
        }

        /// <summary>
        /// Gets an argument just before the one that was provided from the array.
        /// </summary>
        /// <param name="value">The argument after the wanted argument.</param>
        /// <returns>The argument that was before the one specified.</returns>
        public string BeforeValuedSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                if (Count() > 0)
                    return GetArgAtPosition(IndexOfValuedSwitch(value) - 1);
            }
            return "";
        }

        /// <summary>
        /// Gets an argument just before the one that was provided from the array.
        /// </summary>
        /// <param name="variable">The argument after the wanted argument.</param>
        /// <returns>The argument that was before the one specified.</returns>
        public string BeforeVariable(CommandArgVariable variable)
        {
            if (!(IsEmpty()))
            {
                if (Count() > 0)
                    return GetArgAtPosition(IndexOfVariable(variable) - 1);
            }
            return "";
        }
        #endregion

        #region After
        /// <summary>
        /// Gets an argument right after the one that was provided from the array.
        /// </summary>
        /// <param name="arg">The argument before the wanted argument.</param>
        /// <returns>The argument that was after the one specified.</returns>
        public string AfterArg(string arg)
        {
            if (!(IsEmpty()))
            {
                var index = IndexOfArg(arg);
                if (index < Count())
                    return GetArgAtPosition(index + 1);
            }
            return "";
        }

        /// <summary>
        /// Gets an argument right after the one that was provided from the array.
        /// </summary>
        /// <param name="value">The argument before the wanted argument.</param>
        /// <returns>The argument that was after the one specified.</returns>
        public string AfterSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                var index = IndexOfSwitch(value);
                if (index < Count())
                    return GetArgAtPosition(index + 1);
            }
            return "";
        }

        /// <summary>
        /// Gets an argument right after the one that was provided from the array.
        /// </summary>
        /// <param name="value">The argument before the wanted argument.</param>
        /// <returns>The argument that was after the one specified.</returns>
        public string AfterSegmentedSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                var index = IndexOfSegmentedSwitch(value);
                if (index < Count())
                    return GetArgAtPosition(index + 1);
            }
            return "";
        }

        /// <summary>
        /// Gets an argument right after the one that was provided from the array.
        /// </summary>
        /// <param name="value">The argument before the wanted argument.</param>
        /// <returns>The argument that was after the one specified.</returns>
        public string AfterValuedSwitch(string value)
        {
            if (!(IsEmpty()))
            {
                var index = IndexOfValuedSwitch(value);
                if (index < Count())
                    return GetArgAtPosition(index + 1);
            }
            return "";
        }

        /// <summary>
        /// Gets an argument right after the one that was provided from the array.
        /// </summary>
        /// <param name="variable">The argument before the wanted argument.</param>
        /// <returns>The argument that was after the one specified.</returns>
        public string AfterVariable(CommandArgVariable variable)
        {
            if (!(IsEmpty()))
            {
                var index = IndexOfVariable(variable);
                if (index < Count())
                    return GetArgAtPosition(index + 1);
            }
            return "";
        }
        #endregion

        #region Skip

        //public static IEnumerable<string> Skip(int count)
        //{
            
        //}

        #endregion

        #region StartsWith
        /// <summary>
        /// Checks to see if the argument array starts with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the argument was found at the begining of the array.</returns>
        public bool StartsWithArgument(string value)
        {
            if (IsEmpty()) return false;
            var x = args.ToArray();
            if (x[0] == value) return true;
            return false;
        }

        /// <summary>
        /// Checks to see if the argument array starts with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the switch was found at the begining of the array.</returns>
        public bool StartsWithSwitch(string value)
        {
            if (IsEmpty()) return false;
            var x = args.ToArray();
            if (x[0] == value || x[0].Contains(value)) return true;
            return false;
        }

        /// <summary>
        /// Checks to see if the argument array starts with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the segmented switch was found at the begining of the array.</returns>
        public bool StartsWithSegmentedSwitch(string value)
        {
            if (!(IsEmpty()) && (StartsWithSwitch($"{value}:"))) return true;
            return false;
        }

        /// <summary>
        /// Checks to see if the argument array starts with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the valued switch was found at the begining of the array.</returns>
        public bool StartsWithValuedSwitch(string value)
        {
            if (!(IsEmpty()) && (StartsWithSwitch($"{value}="))) return true;
            return false;
        }

        /// <summary>
        /// Checks to see if the argument array starts with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the variable was found at the begining of the array.</returns>
        public bool StartsWithVariable(CommandArgVariable variable)
        {
            if (!(IsEmpty()) && (StartsWithArgument($"{variable.GetHeader()}{variable.GetValue()}"))) return true;
            return false;
        }
        #endregion

        #region EndsWith
        /// <summary>
        /// Checks to see if the argument array ends with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the argument was found at the end of the array.</returns>
        public bool EndsWithArgument(string value)
        {
            if (IsEmpty()) return false;
            var x = args.ToArray();
            if (x[Count()] == value) return true;
            return false;
        }

        /// <summary>
        /// Checks to see if the argument array ends with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the switch was found at the end of the array.</returns>
        public bool EndsWithSwitch(string value)
        {
            if (IsEmpty()) return false;
            var x = args.ToArray();
            if (x[x.Length] == value || x[x.Length].Contains(value)) return true;
            return false;
        }

        /// <summary>
        /// Checks to see if the argument array ends with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the segmented switch was found at the end of the array.</returns>
        public bool EndsWithSegmentedSwitch(string value)
        {
            if (!(IsEmpty()) && (EndsWithSwitch($"{value}:"))) return true;
            return false;
        }

        /// <summary>
        /// Checks to see if the argument array ends with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the valued switch was found at the end of the array.</returns>
        public bool EndsWithValuedSwitch(string value)
        {
            if (!(IsEmpty()) && (EndsWithSwitch($"{value}="))) return true;
            return false;
        }

        /// <summary>
        /// Checks to see if the argument array ends with the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the variable was found at the ends of the array.</returns>
        public bool EndsWithVariable(CommandArgVariable variable)
        {
            if (!(IsEmpty()) && (EndsWithArgument($"{variable.GetHeader()}{variable.GetValue()}"))) return true;
            return false;
        }
        #endregion

        public class CommandArgVariable
        {
            private char _header;
            private string _value;

            public CommandArgVariable(char header, string value)
            {
                _header = header;
                _value = value;
            }

            public char GetHeader()
            {
                return _header;
            }

            public string GetValue()
            {
                return _value;
            }
        }
    }
}
