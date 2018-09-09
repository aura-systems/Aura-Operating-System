using System;

namespace WMCommandFramework.COSMOS
{
    public abstract class Command
    {
        private bool _throwSyntaxError = false;

        /// <summary>
        /// Produce a syntax error.
        /// </summary>
        public bool ThrowSyntax
        {
            get => _throwSyntaxError;
            set => _throwSyntaxError = value;
        }

        public abstract string Name();
        public abstract string Description();
        public abstract string Syntax();
        public abstract string[] Aliases();
        public abstract CommandVersion Version();
        public virtual CommandCopyright Copyright() { return new CommandCopyright(); }
        public abstract void Invoke(CommandInvoker invoker, CommandArgs args);
    }
}
