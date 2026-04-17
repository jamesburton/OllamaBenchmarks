using System;

public static class DateUtils
{
    public static int DaysBetween(DateTime start, DateTime end)
    {
        TimeSpan difference = end - start;
        return Math.Abs(difference.Days);
    }

    public static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}