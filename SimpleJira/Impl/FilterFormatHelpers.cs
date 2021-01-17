using System;

namespace SimpleJira.Impl
{
    internal static class FilterFormatHelpers
    {
        public static string Format(DateTime datetime)
        {
            return datetime.ToString(datetime == datetime.Date ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm");
        }
    }
}