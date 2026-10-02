/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Time Implementation
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   John Welsh <djlw78@gmail.com>
*                   Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;

namespace Aura_OS.System
{
    public static class Time
    {

        // Each public method reads DateTime.Now once and passes the snapshot down,
        // so the fields of one string all come from the same instant.
        // GEN3-GAP(tz-rtc): firmware clock as-is, no timezone.

        static int Hour(DateTime now) { return now.Hour; }

        static int Minute(DateTime now) { return now.Minute; }

        static int Second(DateTime now) { return now.Second; }

        static int Century(DateTime now) { return now.Year / 100; }

        // 4-digit year (the gen2 RTC year was 2-digit).
        static int Year(DateTime now) { return now.Year; }

        static int Month(DateTime now) { return now.Month; }

        static int DayOfMonth(DateTime now) { return now.Day; }

        // 0 = Sunday (the gen2 RTC day of week was 1-based).
        static int DayOfWeek(DateTime now) { return (int)now.DayOfWeek; }

        static string getTime24(DateTime now, bool hour, bool min, bool sec)
        {
            string timeStr = "";
            if (hour)
            {
                if (Hour(now).ToString().Length == 1)
                {
                    timeStr += "0" + Hour(now).ToString();
                }
                else
                {
                    timeStr += Hour(now).ToString();
                }
            }
            if (min)
            {
                if (Minute(now).ToString().Length == 1)
                {
                    timeStr += ":";
                    timeStr += "0" + Minute(now).ToString();
                }
                else
                {
                    timeStr += ":";
                    timeStr += Minute(now).ToString();
                }
            }
            if (sec)
            {
                if (Second(now).ToString().Length == 1)
                {
                    timeStr += ":";
                    timeStr += "0" + Second(now).ToString();
                }
                else
                {
                    timeStr += ":";
                    timeStr += Second(now).ToString();
                }
            }
            return timeStr;
        }

        static string getTime12(DateTime now, bool hour, bool min, bool sec)
        {
            string timeStr = "";
            if (hour)
            {
                if (Hour(now) > 12)
                    timeStr += Hour(now) - 12;
                else if (Hour(now) == 0)
                    timeStr += 12;
                else
                    timeStr += Hour(now);
            }
            if (min)
            {
                if (Minute(now).ToString().Length == 1)
                {
                    timeStr += ":";
                    timeStr += "0" + Minute(now).ToString();
                }
                else
                {
                    timeStr += ":";
                    timeStr += Minute(now).ToString();
                }
            }
            if (sec)
            {
                if (Second(now).ToString().Length == 1)
                {
                    timeStr += ":";
                    timeStr += "0" + Second(now).ToString();
                }
                else
                {
                    timeStr += ":";
                    timeStr += Second(now).ToString();
                }
            }
            if (hour)
            {
                if (Hour(now) >= 12)
                    timeStr += " PM";
                else
                    timeStr += " AM";
            }
            return timeStr;
        }

        /// <summary>
        /// Hour String
        /// </summary>
        /// <returns>Actual Hour</returns>
        public static string TimeString(bool hour, bool min, bool sec)
        {
            DateTime now = DateTime.Now;

            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    return getTime24(now, hour, min, sec);
                case "en_US":
                    return getTime12(now, hour, min, sec);
                case "nl_NL":
                    return getTime24(now, hour, min, sec);
                case "it_IT":
                    return getTime12(now, hour, min, sec);
                default:
                    return getTime12(now, hour, min, sec);
            }
        }

        /// <summary>
        /// Year String
        /// </summary>
        /// <returns>Actual Year</returns>
        public static string YearString()
        {
            int intyear = Year(DateTime.Now);
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
            int intmonth = Month(DateTime.Now);
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
            int intday = DayOfMonth(DateTime.Now);
            string stringday = intday.ToString();

            if (stringday.Length == 1)
            {
                stringday = "0" + stringday;
            }
            return stringday;
        }

    }
}