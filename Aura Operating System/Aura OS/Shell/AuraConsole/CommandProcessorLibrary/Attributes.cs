using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework.Attributes
{
    public class OutdatedCommand : Attribute
    {
        private bool cancel = false;
        private Command replacement = null;

        public OutdatedCommand(Command replacement = null)
        {
            cancel = false;
            this.replacement = replacement;
        }

        public OutdatedCommand(bool cancel, Command replacement = null)
        {
            this.cancel = cancel;
            this.replacement = replacement;
        }

        public bool CancelExecution => cancel;
        public Command ReplacementCommand => replacement;
    }

    public sealed class AttributeManager
    {
        public static bool GetAttribute<T>(object containingClass, out T output) where T: Attribute
        {
            var attrib = containingClass.GetType().GetCustomAttributes(typeof(T), true) as T[];
            if (attrib == null || attrib.Length == 0)
            {
                output = null;
                return false;
            }
            else
            {
                var x = attrib[0];
                output = (T)x;
                return true;
            }
        }
    }
}
