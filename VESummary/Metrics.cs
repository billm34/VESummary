using System;
using System.Globalization;

namespace TestSummarizer
{
    public class Metrics
    {
        protected internal string buildNumber;
        protected internal string resultsPath;
        protected internal string xlsPath;
        protected internal bool isValid;
        protected internal string emailTextFile;
        protected internal string tableTemplate;
        protected internal bool updateAvailable;

        protected int GetCurrentWeekNumber()
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = DateTime.Now;
            Calendar cal = dfi.Calendar;

            return (cal.GetWeekOfYear(date1, dfi.CalendarWeekRule, dfi.FirstDayOfWeek));
        }

        // ConvertToPercent: Converts a three digit number to add a decimal place and percent sign.
        protected string ConvertToPercent(double number)
        {
            string percent;

            percent = number.ToString(CultureInfo.CurrentCulture);
            percent = percent.Insert((percent.Length - 1),".");
            percent += "%";
            return (percent);
        }

        public string GetCurrentDate()
        {
            DateTime dt = DateTime.Now;
            string date;

#if DEBUG
            date = "Last Updated: " + dt.ToString("MMM") + " " + dt.ToString("dd") + " " + dt.ToString("yyyy") + " " + dt.ToString("t");
#else
            date = "Last Updated: " + dt.ToString("MMM") + " " + dt.ToString("dd") + " " + dt.ToString("yyyy");
#endif
            return (date);
        }
    }
}
