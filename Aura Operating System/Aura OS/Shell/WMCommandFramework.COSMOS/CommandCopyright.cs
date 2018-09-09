using System;
using System.Collections.Generic;
using System.Text;
using Hal = Cosmos.HAL;

namespace WMCommandFramework.COSMOS
{
    public class CommandCopyright
    {
        private string _developer = "";
        private int _baseYear = 2018;

        internal CommandCopyright()
        {
            _baseYear = CurrentYear;
            _developer = "*";
        }

        /// <summary>
        /// Creates a new Copyright class that holds all needed copyright information for the class.
        /// </summary>
        /// <param name="developer">The developer of the command or application.</param>
        public CommandCopyright(string developer)
        {
            _baseYear = CurrentYear;
            if (developer == null || developer == "" || developer == "*")
                _developer = "Vanros Corperation";
            else
                _developer = developer;
        }

        /// <summary>
        /// Creates a new Copyright class that holds all needed copyright information for the class.
        /// </summary>
        /// <param name="developer">The developer of the command or application.</param>
        /// <param name="baseYear">The inital year the command or application was first developed.</param>
        public CommandCopyright(string developer, int baseYear)
        {
            if (baseYear < 1990 && !(baseYear > 1990 || baseYear == 1990))
                baseYear = 1990;
            else if (baseYear > CurrentYear)
                baseYear = CurrentYear;
            else
                _baseYear = baseYear;
            if (developer == null || developer == "" || developer == "*")
                _developer = "Vanros Corperation";
            else
                _developer = developer;
        }

        /// <summary>
        /// The developer of the command or application.
        /// </summary>
        private string Developer
        {
            get
            {
                if (_developer == "" || _developer == null)
                    return "Vanros Corperation";
                else if (_developer == "*")
                    return "";
                else
                    return _developer;
            }
        }

        /// <summary>
        /// The initial year the command or application was first developed.
        /// </summary>
        private int CreationYear
        {
            get => _baseYear;
        }

        /// <summary>
        /// Converts all provided copyright information into a simple string.
        /// </summary>
        /// <returns>A simple copyright string. I.E. Copyright (c) 2018 Vanros Corperation, All Rights Reserved!</returns>
        public override string ToString()
        {
            if (CreationYear == CurrentYear)
                return GetCopyrightString(Developer, CreationYear.ToString());
            else if (CreationYear < CurrentYear)
                return GetCopyrightString(Developer, $"{CreationYear}-{CurrentYear}");
            else if (CreationYear > CurrentYear)
                return GetCopyrightString(Developer, $"{CurrentYear}");
            else
                return GetCopyrightString(Developer, CurrentYear.ToString());
        }

        private static string GetCopyrightString(string developer, string yearStamp)
        {
            return $"Copyright (c) {yearStamp} {developer}, All Rights Reserved!";
        }

        internal static CommandCopyright VanrosCopyright()
        {
            return new CommandCopyright("", 2017);
        }

        internal static CommandCopyright VanrosCopyright(int year)
        {
            if (year < 2015)
                year = 2015;
            return new CommandCopyright("", year);
        }

        private static int CurrentYear
        {
            get
            {
                return Hal.RTC.Year;
            }
        }
    }
}
