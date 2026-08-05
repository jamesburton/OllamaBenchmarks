using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null || !values.Any())
            throw new ArgumentException();

        var list = values.ToList();
        double mean = list.Average();
        double min = list.Min();
        double max = list.Max();

        list.Sort();
        int count = list.Count;
        double median;

        if (count % 2 == 1)
            median = list[count / 2];
        else
            median = (list[count / 2 - 1] + list[count / 2]) / 2.0;

        return (mean, median, min, max);
    }
}