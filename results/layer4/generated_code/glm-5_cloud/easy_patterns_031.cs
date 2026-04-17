using System;

public static class Classifier
{
    public static string ClassifyDay(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Saturday => "weekend",
            DayOfWeek.Sunday => "weekend",
            _ => "weekday"
        };
    }
}