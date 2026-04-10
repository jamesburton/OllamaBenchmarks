using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null || !values.Any())
        {
            throw new ArgumentException("The input sequence cannot be empty.");
        }

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        var sortedList = values.OrderBy(v => v).ToList();
        int count = sortedList.Count;
        double median;

        if (count % 2 == 0)
        {
            median = (sortedList[count / 2 - 1] + sortedList[count / 2]) / 2.0;
        }
        else
        {
            median = sortedList[count / 2];
        }

        return (mean, median, min, max);
    }
}