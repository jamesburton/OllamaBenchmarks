using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null || !values.Any())
        {
            throw new ArgumentException("Sequence cannot be empty.");
        }

        var list = values.ToList();
        double mean = list.Average();
        double min = list.Min();
        double max = list.Max();

        var sorted = list.OrderBy(n => n).ToList();
        int count = sorted.Count;
        double median;

        if (count % 2 == 0)
        {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            median = sorted[count / 2];
        }

        return (mean, median, min, max);
    }
}