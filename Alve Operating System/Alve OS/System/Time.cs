using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;

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

        public static string HourString() {
            int inthour = Hour();
            string stringhour = inthour.ToString();

            if (stringhour.Length == 1)
            {
                stringhour = "0" + stringhour;
            }
            return stringhour;
        }

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
