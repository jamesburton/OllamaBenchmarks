using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!System.Linq.Enumerable.Any(values))
            throw new ArgumentException("Sequence is empty.");

        double mean = System.Linq.Enumerable.Average(values);
        double min = System.Linq.Enumerable.Min(values);
        double max = System.Linq.Enumerable.Max(values);

        var sorted = System.Linq.Enumerable.ToArray(values.OrderBy(v => v));
        int n = sorted.Length;
        double median = n % 2 == 1 ? sorted[n / 2] : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;

        return (mean, median, min, max);
    }
}