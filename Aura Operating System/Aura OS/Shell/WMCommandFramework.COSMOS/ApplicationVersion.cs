using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework.COSMOS
{
    public class ApplicationVersion
    {
        private string _name;
        private CommandCopyright _copyright;
        private CommandVersion _version;

        /// <summary>
        /// The version information for the current application.
        /// </summary>
        /// <param name="name">The name of the application.</param>
        /// <param name="copyright">The copyright of the developer that developed the application.</param>
        /// <param name="version">The current version of the application.</param>
        public ApplicationVersion(string name, CommandCopyright copyright, CommandVersion version)
        {
            _name = name;
            _copyright = copyright;
            _version = version;
        }

        /// <summary>
        /// The name of the application.
        /// </summary>
        /// <returns>The name.</returns>
        public string Name()
        {
            return _name;
        }

        /// <summary>
        /// The copyright of the developer of the current application.
        /// </summary>
        /// <returns>The copyright of the developer.</returns>
        public CommandCopyright Copyright()
        {
            return _copyright;
        }

        /// <summary>
        /// The current version of the application.
        /// </summary>
        /// <returns>The current version.</returns>
        public CommandVersion Version()
        {
            return _version;
        }
        
        internal static ApplicationVersion CommandFrameworkVersion
        {
            get => new ApplicationVersion("CommandFramework", CommandCopyright.VanrosCopyright() ,new CommandVersion(1,1,0,"STABLE"));
        }
    }
}

//, new CommandCopyright("Vanros Corperation", 2017)
