using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
        {
            throw new ArgumentException("Input sequence is empty.");
        }

        var sortedValues = values.OrderBy(v => v).ToList();
        int count = sortedValues.Count;

        double mean = sortedValues.Average();
        double min = sortedValues.Min();
        double max = sortedValues.Max();

        double median;
        if (count % 2 == 1)
        {
            median = sortedValues[count / 2];
        }
        else
        {
            median = (sortedValues[(count - 1) / 2] + sortedValues[count / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}