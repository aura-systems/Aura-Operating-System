/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Time Implementation
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.HAL;

namespace Alve_OS.System
{
    public static class Time
    {

        static int Hour() { return RTC.Hour; }

        static int Minute() { return RTC.Minute; }

        static int Second() { return RTC.Second; }

        static int Century() { return RTC.Century; }

        static int Year() { return RTC.Year; }

        static int Month() { return RTC.Month; }

        static int DayOfMonth() { return RTC.DayOfTheMonth; }

        static int DayOfWeek() { return RTC.DayOfTheWeek; }

        /// <summary>
        /// Hour String
        /// </summary>
        /// <returns>Actual Hour</returns>
        public static string HourString() {
            int inthour = Hour();
            string stringhour = inthour.ToString();

            if (stringhour.Length == 1)
            {
                stringhour = "0" + stringhour;
            }
            return stringhour;
        }

        /// <summary>
        /// Minutes String
        /// </summary>
        /// <returns>Actual Minutes</returns>
        public static string MinuteString()
        {
            int intminute = Minute();
            string stringminute = intminute.ToString();

            if (stringminute.Length == 1)
            {
                stringminute = "0" + stringminute;
            }
            return stringminute;
        }

        /// <summary>
        /// Year String
        /// </summary>
        /// <returns>Actual Seconds</returns>
        public static string SecondString()
        {
            int intsecond = Second();
            string stringsecond = intsecond.ToString();

            if (stringsecond.Length == 1)
            {
                stringsecond = "0" + stringsecond;
            }
            return stringsecond;
        }

        /// <summary>
        /// Year String
        /// </summary>
        /// <returns>Actual Year</returns>
        public static string YearString()
        {
            int intyear = Year();
            string stringyear = intyear.ToString();

            if (stringyear.Length == 2)
            {
                stringyear = "20" + stringyear;
            }
            return stringyear;
        }

        /// <summary>
        /// Month String
        /// </summary>
        /// <returns>Actual Month</returns>
        public static string MonthString()
        {
            int intmonth = Month();
            string stringmonth = intmonth.ToString();

            if (stringmonth.Length == 1)
            {
                stringmonth = "0" + stringmonth;
            }
            return stringmonth;
        }

        /// <summary>
        /// Day String
        /// </summary>
        /// <returns>Actual Day</returns>
        public static string DayString()
        {
            int intday = DayOfMonth();
            string stringday = intday.ToString();

            if (stringday.Length == 1)
            {
                stringday = "0" + stringday;
            }
            return stringday;
        }

    }
}
